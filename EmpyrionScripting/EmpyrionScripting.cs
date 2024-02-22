using Eleon.Modding;
using EmpyrionNetAPIAccess;
using EmpyrionNetAPIDefinitions;
using EmpyrionNetAPITools;
using EmpyrionNetAPITools.Extensions;
using EmpyrionScripting.CsCompiler;
using EmpyrionScripting.CustomHelpers;
using EmpyrionScripting.DataWrapper;
using EmpyrionScripting.Interface;
using EmpyrionScripting.Internal.Interface;
using HandlebarsDotNet;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using YamlDotNet.RepresentationModel;
using static EmpyrionScripting.CsCompiler.CsCompiler;
using static EmpyrionScripting.SaveGamesScripts;
using StaticCsCompiler = EmpyrionScripting.CsCompiler;

namespace EmpyrionScripting
{

    public sealed partial class EmpyrionScripting : ModInterface, IMod, IDisposable
    {
        public static event EventHandler StopScriptsEvent;

        private const string TargetsKeyword = "Targets:";
        private const string ScriptKeyword = "Script:";
        private const string CsKeyword = "C#:";
        ModGameAPI LegacyApi;

        public class DediLegacyModBase : EmpyrionModBase
        {
            public override void Initialize(ModGameAPI dediAPI){}
        }

        public DediLegacyModBase DediLegacyMod { get; set; }

        public static EmpyrionScripting EmpyrionScriptingInstance { get; set; }
        public static ConfigEcfAccess ConfigEcfAccess { get; set; }
        public static ItemInfos ItemInfos { get; set; }
        public static string SaveGameModPath { get; set; }
        public static ConfigurationManager<Configuration> Configuration { get; set; } = new ConfigurationManager<Configuration>() { Current = new Configuration() };
        public static Dictionary<string, object> GameOptionsYamlSettings { get; private set; } = new Dictionary<string, object>();

        public static Localization Localization { get; set; }
        public static IModApi ModApi { get; set; }
        public bool WithinCsCompiler { get; set; }
        public ModGameAPI ModGameDediAPI { get; set; }

        public SaveGamesScripts SaveGamesScripts { get; set; }
        public string L { get; private set; }
        public DateTime LastAlive { get; private set; }
        public int InGameScriptsCount { get; private set; }
        public int SaveGameScriptsCount { get; private set; }
        public static bool AppFoldersLogged { get; private set; }

        public CsCompiler.CsCompiler CsCompiler { get; set; }

        public static SqlDbAccess SqlDbAccess { get; } = new SqlDbAccess();

        public ConcurrentDictionary<string, PlayfieldScriptData> PlayfieldData { get; set; } = new ConcurrentDictionary<string, PlayfieldScriptData>();
        public static string ScriptingModInfoData { get; private set; } = string.Empty;
        public static ConcurrentDictionary<string, string> ScriptingModScriptsInfoData { get; } = new ConcurrentDictionary<string, string>();
        public static ConcurrentDictionary<string, LoadedAssemblyInfo> AddOnAssemblies { get; set; } = new ConcurrentDictionary<string, LoadedAssemblyInfo>();
        private static readonly Assembly CurrentAssembly = Assembly.GetAssembly(typeof(EmpyrionScripting));
        public static string Version { get; } = $"{CurrentAssembly.GetAttribute<AssemblyTitleAttribute>()?.Title } by {CurrentAssembly.GetAttribute<AssemblyCompanyAttribute>()?.Company} Version:{CurrentAssembly.GetAttribute<AssemblyFileVersionAttribute>()?.Version}";

        public EmpyrionScripting()
        {
            EmpyrionScriptingInstance         = this;
            EmpyrionConfiguration.ModName     = "EmpyrionScripting";
            DeviceLock     .Log               = Log;
            ConveyorHelpers.Log               = Log;
            ScriptExecQueue.Log               = Log;
            ConfigEcfAccess.Log               = Log;
            Localization   .Log               = Log;
            PlayerCommandsDediHelper    .Log  = Log;
            StaticCsCompiler.CsCompiler .Log  = Log;
            SqlDbAccess.Log                   = Log;
            SetupHandlebarsComponent();
        }

        public void Init(IModApi modAPI)
        {
            ModApi = modAPI;
            ModApi.Log($"EmpyrionScripting Mod started: IModApi {ModApi.Application.Mode}");

            if (ModApi.Application.Mode == ApplicationMode.DedicatedServer)
            {
                PlayerCommandsDediHelper = new PlayerCommandsDediHelper(modAPI);
                return;
            }

            try
            {
                SetupHandlebarsComponent();

                SaveGameModPath = Path.Combine(ModApi.Application?.GetPathFor(AppFolder.SaveGame), "Mods", EmpyrionConfiguration.ModName);
                ModApi.Application.GameEntered += Application_GameEntered;

                LoadConfiguration();
                SaveGamesScripts = new SaveGamesScripts(modAPI) { SaveGameModPath = SaveGameModPath };
                SaveGamesScripts.ReadSaveGamesScripts();

                TaskTools.Log = ModApi.LogError;
                StaticCsCompiler.CsCompiler.Log = Log;

                CsCompiler = new StaticCsCompiler.CsCompiler(SaveGameModPath, ModApi, typeof(EmpyrionScripting).Assembly) { ScriptErrorTracking = ScriptErrorTracking };
                CsCompiler.ConfigurationChanged += CsCompiler_ConfigurationChanged;

                ModApi.Application.OnPlayfieldLoaded    += Application_OnPlayfieldLoaded;
                ModApi.Application.OnPlayfieldUnloading += Application_OnPlayfieldUnloading;

                StopScriptsEvent += (S, E) =>
                {
                    PlayfieldData?.Values.ForEach(P =>
                    {
                        ModApi.Log($"StopScriptsEvent: ({P.PlayfieldName}) {(P.PauseScripts ? "always stopped" : "scripts running")}");
                        P.PauseScripts = true;
                    });
                };

                StartAllScriptsForPlayfieldServer();
            }
            catch (Exception error)
            {
                ModApi.LogError($"EmpyrionScripting Mod init finish: {error}");
            }

            ModApi.Log("EmpyrionScripting Mod init finish");

        }

        private static void ScriptErrorTracking<T>(T rootObjectCompileTime, List<string> messages) where T : IScriptRootModData
        {
            if (Configuration.Current.ScriptTrackingError)
            {
                File.AppendAllText(rootObjectCompileTime is ScriptSaveGameRootData root
                    ? GetTrackingFileName(root)
                    : GetTrackingFileName(rootObjectCompileTime.E.GetCurrent(), rootObjectCompileTime.Script.GetHashCode().ToString()) + ".error",
                    string.Join("\n", messages));
            }
        }

        private void Application_GameEntered(bool hasEntered)
        {
            try 
            { 
                ModApi.Log($"Application_GameEntered {hasEntered}");
                if (hasEntered) InitGameDependedData(false);
                ModApi.Log("Application_GameEntered init finish");
            }
            catch (Exception error)
            {
                ModApi.LogError($"Application_GameEntered: {error}");
            }
        }

        private void CheckAddOnAssemblies()
        {
            ModApi.Log($"CheckAddOnAssemblies: #{Configuration?.Current?.AddOnAssemblies?.Length}");
            var processed = AddOnAssemblies.Select(dll => dll.Key).ToList();

            Configuration?.Current?.AddOnAssemblies?.ForEach(dll => {
                string dllPath = dll;
                try
                {
                    dllPath = Path.Combine(SaveGameModPath, dll).NormalizePath();
                    if (!AddOnAssemblies.ContainsKey(dllPath)) LoadCustomAssembly(AddOnAssemblies, SaveGameModPath, dllPath);
                    else processed.Remove(dllPath);
                }
                catch (Exception error)
                {
                    ModApi.LogError($"CheckAddOnAssembly: {dll} ({dllPath}) -> {error}");
                }
            });

            processed.ForEach(dll => AddOnAssemblies.TryRemove(dll, out var customAssembly));
        }

        private void InitGameDependedData(bool forceInit)
        {
            ModApi.Log($"InitGameDependedData: forceInit:{forceInit}");

            if (forceInit || !AppFoldersLogged)
            {
                AppFoldersLogged = true;
                ModApi.Log($"InitGameDependedData [ForceInit:{forceInit}]:\n" +
                    $"AppFolder.Content:{ModApi.Application?.GetPathFor(AppFolder.Content)}\n" +
                    $"AppFolder.Mod:{ModApi.Application?.GetPathFor(AppFolder.Mod)}\n" +
                    $"AppFolder.Root:{ModApi.Application?.GetPathFor(AppFolder.Root)}\n" +
                    $"AppFolder.SaveGame:{ModApi.Application?.GetPathFor(AppFolder.SaveGame)}\n" +
                    $"AppFolder.Dedicated:{ModApi.Application?.GetPathFor(AppFolder.Dedicated)}\n" +
                    $"AppFolder.Cache:{ModApi.Application?.GetPathFor(AppFolder.Cache)}\n" +
                    $"AppFolder.ActiveScenario:{ModApi.Application?.GetPathFor(AppFolder.ActiveScenario)} -> CurrentScenario:{CurrentScenario}");
            }
            SaveGameModPath = Path.Combine(ModApi.Application?.GetPathFor(AppFolder.SaveGame), "Mods", EmpyrionConfiguration.ModName);
            if(forceInit) LoadConfiguration();

            if(forceInit || Localization    == null) Localization = new Localization(ModApi.Application?.GetPathFor(AppFolder.Content), CurrentScenario);
            if(forceInit || ConfigEcfAccess == null) InitEcfConfigData();

            try { ReadGameOptionsYaml(); } catch (Exception error) { ModApi.LogError($"ReadGameOptionsYaml: {error}");
}
        }

        public static void ReadGameOptionsYaml() 
        { 
            var gameoptionsYaml = Path.Combine(SaveGameModPath, "..", "..", "gameoptions.yaml");
            if (!File.Exists(gameoptionsYaml)) return;

            using var input = new StringReader(File.ReadAllText(gameoptionsYaml));

            var yaml = new YamlStream();
            yaml.Load(input);

            var root = (YamlMappingNode)yaml.Documents[0].RootNode;
            var options = root.GetChild<YamlSequenceNode>("Options");

            var searchForMode = ModApi?.Application?.Mode == ApplicationMode.SinglePlayer ? "SP" : "MP";

            var ValidForNode = options?.Children.Where(n => n.ToString().Contains(searchForMode)).FirstOrDefault() as YamlMappingNode;
            GameOptionsYamlSettings = ValidForNode?.Children
                .Where(n => n.Key is YamlScalarNode && n.Value is YamlScalarNode)
                .ToDictionary(n => n.Key.ToString(), n => GetTypedValue(n.Value.ToString()));
        }

        private static object GetTypedValue(string value)
        {
            if(bool.TryParse(value, out var boolResult)) return boolResult;
            if(int .TryParse(value, out var intResult )) return intResult;

            return value;
        }

        private static string CurrentScenario
            =>
            string.IsNullOrEmpty(Configuration.Current.OverrideScenarioPath)
            ? string.IsNullOrEmpty(EmpyrionConfiguration.DedicatedYaml.CustomScenarioName)
                ? ModApi.Application?.GetPathFor(AppFolder.ActiveScenario)
                : EmpyrionConfiguration.DedicatedYaml.CustomScenarioName
            : Configuration.Current.OverrideScenarioPath;

        public PlayerCommandsDediHelper PlayerCommandsDediHelper { get; private set; }
        public int GameUpdateCounter;

        private static void InitEcfConfigData()
        {
            ConfigEcfAccess = new ConfigEcfAccess();
            ConfigEcfAccess.ReadConfigEcf(
                ModApi.Application?.GetPathFor(AppFolder.Content),
                CurrentScenario,
                Path.Combine(ModApi.Application?.GetPathFor(AppFolder.SaveGame), "blocksmap.dat"), ModApi);
            ItemInfos = new ItemInfos(ConfigEcfAccess, Localization);
            Configuration_ProcessIdsLists();
        }

        private void CsCompiler_ConfigurationChanged(object sender, EventArgs e)
        {
            PlayfieldData.ForEach(P =>
            {
                ModApi.Log($"CsCompiler_ConfigurationChanged: {P.Key} {(P.Value == null ? "null" : (P.Value.PauseScripts ? "always stopped" : "scripts running"))}");

                DisplayScriptInfos();
                P.Value?.ScriptExecQueue?.Clear();
                P.Value?.LcdCompileCache?.Clear();
            });
        }

        private void LoadConfiguration()
        {
            ConfigurationManager<Configuration>.Log = ModApi.Log;
            if (!string.IsNullOrEmpty(Configuration.ConfigFilename)) return;

            Configuration = new ConfigurationManager<Configuration>()
            {
                ConfigFilename = Path.Combine(SaveGameModPath, "Configuration.json")
            };
            Configuration.ConfigFileLoaded += (s, e) =>
            {
                ModApi.Log($"ConfigurationChanged/Loaded: {Configuration.ConfigFilename}");

                SqlDbAccess.SaveGamePath    = Path.Combine(SaveGameModPath, "..");
                SqlDbAccess.ElevatedQueries = Configuration.Current.DBQueries.Elevated;
                SqlDbAccess.PlayerQueries   = Configuration.Current.DBQueries.Player;
                CheckAddOnAssemblies();
                Configuration_ProcessIdsLists();
            };
            Configuration.Load();
            if (Configuration.LoadException == null || !File.Exists(Configuration.ConfigFilename)) Configuration.Save();
        }

        private static void Configuration_ProcessIdsLists()
        {
            if (ConfigEcfAccess == null) return;

            var lists = new IdLists
            {
                BlockIdMapping = ConfigEcfAccess.BlockIdMapping,
                IdBlockMapping = ConfigEcfAccess.IdBlockMapping
            };

            Configuration.Current.StructureTank.ForEach(tank => ItemNameId.ProcessAllowedItemsMapping(tank.Value, ConfigEcfAccess.BlockIdMapping));

            Configuration.Current.StructureTank.ForEach(tank => {
                var keyName = tank.Key switch
                {
                    StructureTankType.Oxygen   => "Oxygen",
                    StructureTankType.Fuel     => "Fuel",
                    StructureTankType.Pentaxid => "Pentaxid",
                    _                          => "?",
                };

                if(!Configuration.Current.Ids.ContainsKey(keyName)) Configuration.Current.Ids[keyName] = tank.Value.Aggregate("", (s, i) => i.ItemId > 0 ? $"{s},{i.ItemName}" : s) + ",";
            });

            lists.ProcessLists(Configuration.Current.Ids);

            if (ConfigEcfAccess.BlockIdMapping.TryGetValue(Configuration.Current.GardenerSalary     .ItemName, out var id)) Configuration.Current.GardenerSalary    .ItemId = id;
            if (ConfigEcfAccess.BlockIdMapping.TryGetValue(Configuration.Current.DeconstructSalary  .ItemName, out     id)) Configuration.Current.DeconstructSalary .ItemId = id;
            if (ConfigEcfAccess.BlockIdMapping.TryGetValue(Configuration.Current.RecycleSalary      .ItemName, out     id)) Configuration.Current.RecycleSalary     .ItemId = id;

            ItemNameId.ProcessAllowedItemsMapping(Configuration.Current.DeconstructBlockSubstitutions, ConfigEcfAccess.BlockIdMapping);
            Configuration.Current.DeconstructBlockSubstitution = Configuration.Current.DeconstructBlockSubstitutions.ToDictionary(i => i.ItemId, i => i.Amount);

            ItemNameId.ProcessAllowedItemsMapping(Configuration.Current.HarvestCores, ConfigEcfAccess.BlockIdMapping);
            Configuration.Current.HarvestCoreTypes = Configuration.Current.HarvestCores.Select(i => i.ItemId).ToArray();

            Configuration.Current.MappedIds = lists.MappedIds;
            Configuration.Current.NamedIds  = lists.NamedIds;

            Configuration.Current.Ids       = lists.NamedIds;
            if (Configuration.LoadException == null || !File.Exists(Configuration.ConfigFilename)) Configuration.Save();
        }

        public void Shutdown()
        {
            ModApi.Log("Mod exited:Shutdown");

            try{ StopScriptsEvent.Invoke(this, EventArgs.Empty);}
            catch (Exception error) { ModApi.Log($"StopScriptsEvent:{error}"); }

            ModApi.Log("Mod exited:Shutdown finished");
        }

        private void SetupHandlebarsComponent()
        {
            Handlebars.Configuration.TextEncoder = null;
            HandlebarsHelpers.ScanHandlebarHelpers();
        }

        private void Application_OnPlayfieldLoaded(IPlayfield playfield)
        {
            try
            {
                PlayfieldScriptData data = null;

                InitGameDependedData(ModApi.Application.Mode == ApplicationMode.SinglePlayer);

                if (!PlayfieldData.TryAdd(playfield.Name, data = new PlayfieldScriptData(this)
                {
                    PlayfieldName   = playfield.Name,
                    Playfield       = playfield,
                })) Log($"PlayfieldData.TryAdd failed {playfield.Name}", LogLevel.Error);

                UpdateScriptingModInfoData();

                ModApi.Log($"StartScripts for {playfield.Name} pending");
                TaskTools.Delay(Configuration.Current.DelayStartForNSecondsOnPlayfieldLoad, () => {
                    ModApi.Log($"StartScripts for {playfield.Name}");
                    data.PauseScripts = false;

                    if (ModApi.Application.Mode == ApplicationMode.SinglePlayer)
                    {
                        ModApi.Log(playfield.Entities?.Aggregate($"Player:{playfield.Players.FirstOrDefault().Value?.Name} PlayerDriving:{playfield.Players.FirstOrDefault().Value?.DrivingEntity?.Name}", (L, E) => L + $" {E.Key}:{E.Value.Name}"));
                    
                        data.AddEntity(playfield.Players.FirstOrDefault().Value?.DrivingEntity);
                        playfield.Entities?.ForEach(E => data.AddEntity(E.Value));
                    }
                });

                data.Playfield.OnEntityLoaded   += data.Playfield_OnEntityLoaded;
                data.Playfield.OnEntityUnloaded += data.Playfield_OnEntityUnloaded;
            }
            catch (Exception error)
            {
                ModApi.LogError($"Application_OnPlayfieldLoaded: {error}");
            }
        }

        private void UpdateScriptingModInfoData()
        {
            var output = new StringBuilder();

            output.AppendLine($"Version: {Version}");
            output.AppendLine($"Configuration.json: {(Configuration.LoadException == null ? "OK" : Configuration.LoadException.Message)}");
            output.AppendLine($"DefaultCsCompilerConfiguration.json: {(CsCompiler.DefaultConfiguration.LoadException == null ? "OK" : CsCompiler.DefaultConfiguration.LoadException.Message)}");
            output.AppendLine($"CsCompilerConfiguration.json: {(CsCompiler.Configuration.LoadException == null ? "OK" : CsCompiler.Configuration.LoadException.Message)}");
            output.AppendLine($"CsCompilerLearnMode:{CsCompiler.Configuration.Current.WithinLearnMode}");
            output.AppendLine($"CsScriptsAllowedFor:{Configuration.Current.CsScriptsAllowedFor}");
            output.AppendLine($"LogLevel:{Configuration.Current.LogLevel}");
            output.AppendLine($"DetailedScriptsInfoData:{Configuration.Current.DetailedScriptsInfoData}");
            output.AppendLine($"DelayStartForNSecondsOnPlayfieldLoad:{Configuration.Current.DelayStartForNSecondsOnPlayfieldLoad}");
            output.AppendLine($"ExecMethod:{Configuration.Current.ExecMethod}");
            output.AppendLine($"InGameScriptsIntervallMS:{Configuration.Current.InGameScriptsIntervallMS}");
            output.AppendLine($"SaveGameScriptsIntervallMS:{Configuration.Current.SaveGameScriptsIntervallMS}");
            output.AppendLine($"DeviceLockOnlyAllowedEveryXCycles:{Configuration.Current.DeviceLockOnlyAllowedEveryXCycles}");
            output.AppendLine($"ScriptLoopBackgroundTimeLimiterMS:{Configuration.Current.ScriptLoopBackgroundTimeLimiterMS}");
            output.AppendLine($"ScriptLoopSyncTimeLimiterMS:{Configuration.Current.ScriptLoopSyncTimeLimiterMS}");
            output.AppendLine($"ScriptsParallelExecution:{Configuration.Current.ScriptsParallelExecution}");
            output.AppendLine($"ScriptsSyncExecution:{Configuration.Current.ScriptsSyncExecution}");
            output.AppendLine($"EntityAccessMaxDistance:{Configuration.Current.EntityAccessMaxDistance}");
            output.AppendLine();
            output.AppendLine(DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss"));

            ScriptingModInfoData = output.ToString();
        }

        private void Application_OnPlayfieldUnloading(IPlayfield playfield)
        {
            try
            {
                ScriptingModScriptsInfoData.TryRemove(playfield.Name, out _);

                if(!PlayfieldData.TryRemove(playfield.Name, out var data)) return;

                data.Playfield.OnEntityLoaded   -= data.Playfield_OnEntityLoaded;
                data.Playfield.OnEntityUnloaded -= data.Playfield_OnEntityUnloaded;

                ModApi.Log($"PauseScripts for {playfield.Name} {(data.PauseScripts ? "always stopped" : "scripts running")}");
                data.PauseScripts = true;

                DisplayScriptInfos();
                data.ScriptExecQueue?.Clear();
                data.LcdCompileCache?.Clear();
                data.PersistendData?.Clear();

                var stores = data.EventStore?.Values.ToArray();
                data.EventStore?.Clear();
                stores?.ForEach(S => ((EventStore)S).Dispose());
            }
            catch (Exception error)
            {
                ModApi.LogError($"Application_OnPlayfieldUnloading: {error}");
            }
        }

        public void StartAllScriptsForPlayfieldServer()
        {
            ModApi.Log($"StartAllScriptsForPlayfieldServer: InGame:{Configuration.Current.InGameScriptsIntervallMS}ms SaveGame:{Configuration.Current.SaveGameScriptsIntervallMS}ms ");

            StartScriptIntervall(Configuration.Current.InGameScriptsIntervallMS, () =>
            {
                PlayfieldData.Values.ForEach(PF => { 
                    Log($"InGameScript: {PF.PauseScripts}", LogLevel.Debug);
                    LastAlive = DateTime.Now;
                    if (PF.PauseScripts) return;

                    InGameScriptsCount = UpdateScripts(PF, ProcessAllInGameScripts, "InGameScript");
                    PF.ScriptExecQueue.ScriptsCount = InGameScriptsCount + SaveGameScriptsCount;
                });
            }, "InGameScript");

            StartScriptIntervall(Configuration.Current.SaveGameScriptsIntervallMS, () =>
            {
                PlayfieldData.Values.ForEach(PF => { 
                    Log($"SaveGameScript: {PF.PauseScripts}", LogLevel.Debug);
                    LastAlive = DateTime.Now;
                    if (PF.PauseScripts) return;

                    SaveGameScriptsCount = UpdateScripts(PF, ProcessAllSaveGameScripts, "SaveGameScript");
                    PF.ScriptExecQueue.ScriptsCount = InGameScriptsCount + SaveGameScriptsCount;
                });
            }, "SaveGameScript");

            StartScriptIntervall(60000, () =>
            {
                PlayfieldData.Values.ForEach(PF => {                 
                    Log($"ScriptInfos: Pause:{PF.PauseScripts} {PF.ScriptExecQueue.ScriptRunInfo.Count} ExecQueue:{PF.ScriptExecQueue.ExecQueue.Count} WaitForExec:{PF.ScriptExecQueue.WaitForExec.Count}", LogLevel.Debug);
                    LastAlive = DateTime.Now;
                    if (PF.PauseScripts || Configuration.Current.LogLevel > LogLevel.Message) return;

                    DisplayScriptInfos();
                    PF.ScriptExecQueue.CheckForEmergencyRestart(PF);
                });
            }, "ScriptInfos");
        }

        private void DisplayScriptInfos()
        {
            UpdateScriptingModInfoData();

            PlayfieldData.Values.ForEach(PF => {
                var output = new StringBuilder();
                var totalExecTime = new TimeSpan();
                Log($"ScriptInfos[{PF.PlayfieldName}]: RunCount:{PF.ScriptExecQueue.ScriptRunInfo.Count} ExecQueue:{PF.ScriptExecQueue.ExecQueue.Count} WaitForExec:{PF.ScriptExecQueue.WaitForExec.Count} BackgroundWorkerToDoCount:{PF.ScriptExecQueue.BackgroundWorkerToDo.Count} Sync:{PF.ScriptExecQueue.ScriptNeedsMainThread.Count(S => S.Value)} GameUpdateTimeLimitReached:{PF.ScriptExecQueue.GameUpdateScriptLoopTimeLimitReached} Total:{PF.ScriptExecQueue.ScriptNeedsMainThread.Count()} LostItems:{PF.MoveLostItems.Aggregate((string)null, (s, i) => s == null ? $"{i.Id}#{i.Count}->{i.Source}" : $"{s}, {i.Id}#{i.Count}->{i.Source}")}", LogLevel.Message);
                PF.ScriptExecQueue.ScriptRunInfo
                    .OrderBy(I => I.Key)
                    .ForEach(I =>
                    {
                        var line = $"Script: {I.Key,-50} {(PF.ScriptExecQueue.ScriptNeedsMainThread.TryGetValue(I.Key, out var sync) && sync ? ">SYNC<" : "")} #{I.Value.Count,5} LastStart:{I.Value.LastStart} ExecTime:{I.Value.ExecTime} TimeLimitReached:{I.Value._TimeLimitReached} {(I.Value._RunningInstances > 0 ? $" !!!running!!! {I.Value._RunningInstances} times" : "")}";
                        totalExecTime += I.Value.ExecTime;
                        if(Configuration.Current.DetailedScriptsInfoData) output.AppendLine(line);
                        Log(line, I.Value._RunningInstances > 0 ? LogLevel.Error : LogLevel.Debug);
                    });

                output.Insert(0, $"SQL Queries #{SqlDbAccess?.QueryCounter} take {SqlDbAccess?.OverallQueryTime}\n - Player:{SqlDbAccess?.PlayerQueries?.Keys.Aggregate("", (l, k) => $"{l} {k}")}\n - Elevated:{SqlDbAccess?.ElevatedQueries?.Keys.Aggregate("", (l, k) => $"{l} {k}")}\n");
                output.Insert(0, $"RunCount:{PF.ScriptExecQueue.ScriptRunInfo.Count} ExecQueue:{PF.ScriptExecQueue.ExecQueue.Count} WaitForExec:{PF.ScriptExecQueue.WaitForExec.Count} BackgroundWorkerToDoCount:{PF.ScriptExecQueue.BackgroundWorkerToDo.Count} Sync:{PF.ScriptExecQueue.ScriptNeedsMainThread.Count(S => S.Value)} GameUpdateTimeLimitReached:{PF.ScriptExecQueue.GameUpdateScriptLoopTimeLimitReached} Total:{PF.ScriptExecQueue.ScriptNeedsMainThread.Count()} TotalExecTime:{totalExecTime}\n");
                var result = output.ToString();
                ScriptingModScriptsInfoData.AddOrUpdate(PF.PlayfieldName, result, (k, o) => result);
            });
        }

        private void StartScriptIntervall(int intervall, Action action, string name)
        {
            if (intervall <= 0) return;

            var exec = TaskTools.Intervall(intervall, action, name);
            StopScriptsEvent += (S, E) => exec.Set();
        }

        public void Game_Start(ModGameAPI legacyAPI)
        {
            LegacyApi = legacyAPI;
            LegacyApi?.Console_Write("EmpyrionScripting Mod started: Game_Start");

            DediLegacyMod = new DediLegacyModBase();
            DediLegacyMod?.Game_Start(legacyAPI);
        }

        public static string ErrorFilter(Exception error) => Configuration?.Current.LogLevel == EmpyrionNetAPIDefinitions.LogLevel.Debug ? error.ToString() : error.Message;

        private int UpdateScripts(PlayfieldScriptData playfieldData, Func<PlayfieldScriptData, IEntity, int> process, string name)
        {
            try
            {
                if (playfieldData.Playfield          == null) { ModApi.Log($"UpdateScripts no Playfield"); return 0; }
                if (playfieldData.Playfield.Entities == null) { ModApi.Log($"UpdateScripts no Entities"); return 0; }

                var timer = new Stopwatch();
                timer.Start();

                playfieldData.AllEntities     = playfieldData.Playfield.Entities.Values.ToArray();
                playfieldData.CurrentEntities = playfieldData.AllEntities
                    .Where(E => E.Type == EntityType.BA ||
                                E.Type == EntityType.CV ||
                                E.Type == EntityType.SV ||
                                E.Type == EntityType.HV)
                    .ToArray();

                Log($"CurrentEntities: {playfieldData.CurrentEntities.Length}", LogLevel.Debug);
                playfieldData.EntityCultureInfo.Clear();

                int count = 0;
                playfieldData.CurrentEntities.ForEach(E => count += process(playfieldData, E));

                timer.Stop();
                if(timer.Elapsed.TotalSeconds > 30) Log($"UpdateScripts: {name} RUNS {timer.Elapsed} !!!!", LogLevel.Message);
                else                                Log($"UpdateScripts: {name} take {timer.Elapsed}",      LogLevel.Debug);

                return count;
            }
            catch (Exception error)
            {
                ModApi.LogWarning("Next try because: " + ErrorFilter(error));

                return 0;
            }
        }

        public static void Log(string text, LogLevel level)
        {
            if(Configuration?.Current.LogLevel <= level){
                switch (level)
                {
                    case LogLevel.Debug     : ModApi?.Log(text);        break;
                    case LogLevel.Message   : ModApi?.Log(text);        break;
                    case LogLevel.Error     : ModApi?.LogError(text);   break;
                    default                 : ModApi?.Log(text);        break;
                }
            }
        }

        private int ProcessAllInGameScripts(PlayfieldScriptData playfieldData, IEntity entity)
        {
            Log($"ProcessAllInGameScripts: {entity.Name}:{entity.Type} Pause:{playfieldData.PauseScripts}", LogLevel.Debug);
            if (entity.Type == EntityType.Proxy || entity.Type == EntityType.Unknown || playfieldData.PauseScripts) return 0;

            try
            {
                if (!entity.Structure.IsPowered && entity.Structure.Fuel > 0) return 0;

                var entityScriptData = new ScriptRootData(playfieldData, playfieldData.AllEntities, playfieldData.CurrentEntities, playfieldData.Playfield, entity,
                    playfieldData.PersistendData, (EventStore)playfieldData.EventStore.GetOrAdd(entity.Id, id => new EventStore(entity)));

                var deviceNames = entityScriptData.E.S.AllCustomDeviceNames.Where(N => IsScriptLCD(N)).ToArray();
                Log($"ProcessAllInGameScripts: #{deviceNames.Length}", LogLevel.Debug);

                int count = 0;
                Parallel.ForEach(deviceNames, N =>
                {
                    if (playfieldData.PauseScripts) return;

                    var lcd = entity.Structure.GetDevice<ILcd>(N);
                    if (lcd == null) return;

                    var scriptPriority = int.TryParse(N.Substring(0, 1), out var prio) ? prio : 0;

                    if (scriptPriority == 0)
                    {
                        var lcdBlockPos = entity.Structure.GetDevicePositions(N);
                        var lcdBlock = lcdBlockPos.Count > 0 ? entity.Structure.GetBlock(lcdBlockPos.First()) : null;
                        if (lcdBlock != null)
                        {
                            lcdBlock.Get(out _, out _, out _, out var active);
                            if (!active) return;
                        }
                    }

                    try
                    {
                        Log($"ProcessAllInGameScripts: {N}", LogLevel.Debug);

                        var data = new ScriptRootData(entityScriptData)
                        {
                            ScriptPriority = scriptPriority,
                            ScriptLanguage = (char.IsDigit(N[0]) ? N.Substring(1) : N).StartsWith(ScriptKeyword) ? ScriptLanguage.Handlebar : ScriptLanguage.Cs,
                            Script         = lcd.GetText(),
                            Error          = L,
                        };

                        AddTargetsAndDisplayType(data, N.Substring(N.IndexOf(':') + 1));

                        if (Configuration.Current.ScriptTracking)
                        {
                            var trackfile = GetTrackingFileName(entity, data);
                            if (!File.Exists(trackfile)) File.WriteAllText(trackfile, data.Script);
                        }


                        data.ScriptId = entity.Id + "/" + N;
                        playfieldData.ScriptExecQueue.Add(data);

                        Interlocked.Increment(ref count);
                    }
                    catch (Exception lcdError)
                    {
                        Log($"UpdateLCDs ({entity.Id}/{entity.Name}):LCD: {lcdError}", LogLevel.Debug);
                    }
                });

                return count;
            }
            catch (Exception error)
            {
                if (Configuration.Current.ScriptTrackingError) File.AppendAllText(GetTrackingFileName(entity, "InGameScript") + ".error", error.ToString());
                return 0;
            }
        }

        private static bool IsScriptLCD(string name)
        {
            if (name.Length <= 1) return false;

            var N = char.IsDigit(name[0]) ? name.Substring(1) : name;
            return N.StartsWith(ScriptKeyword) || N.StartsWith(CsKeyword);
        }

        public static string GetTrackingFileName(IEntity entity, ScriptRootData root) 
            => GetTrackingFileName(entity, $"{root.Script.GetHashCode().ToString()}{(root.ScriptLanguage == ScriptLanguage.Cs ? ".cs" : ".hbs")}");

        public static string GetTrackingFileName(IEntity entity, string scriptInfo)
        {
            var trackfile = Path.Combine(SaveGameModPath, "ScriptTracking", entity == null ? "" : entity.Id.ToString(), $"{entity?.Id}-{entity?.Type}-{scriptInfo}");
            Directory.CreateDirectory(Path.GetDirectoryName(trackfile));
            return trackfile;
        }

        public static string GetTrackingFileName(ScriptSaveGameRootData root)
        {
            var trackfile = Path.Combine(SaveGameModPath, "ScriptTracking", Path.GetFileName(root.ScriptId) + (root.ScriptLanguage == ScriptLanguage.CompiledDll ? ".log" : ""));
            Directory.CreateDirectory(Path.GetDirectoryName(trackfile));
            return trackfile;
        }
        

        private int ProcessAllSaveGameScripts(PlayfieldScriptData playfieldData, IEntity entity)
        {
            Log($"ProcessAllSaveGameScripts: {entity.Name}:{entity.Type} Pause:{playfieldData.PauseScripts}", LogLevel.Debug);
            if (entity.Type == EntityType.Proxy || entity.Type == EntityType.Unknown || playfieldData.PauseScripts) return 0;

            try
            {
                var entityScriptData = new ScriptSaveGameRootData(playfieldData, 
                    playfieldData.AllEntities, playfieldData.CurrentEntities, playfieldData.Playfield, entity, playfieldData.PersistendData, 
                    (EventStore)playfieldData.EventStore.GetOrAdd(entity.Id, id => new EventStore(entity)))
                {
                    MainScriptPath = SaveGamesScripts.MainScriptPath,
                    ModApi = ModApi
                };

                if (entity.IsPoi)
                {
                    var lastVisitedTicks = entity.Structure.LastVisitedTicks;
                    if (playfieldData.LastPOIVisited.TryGetValue(entity.Id, out var ticks) && ticks == lastVisitedTicks) return 0;

                    playfieldData.LastPOIVisited.AddOrUpdate(entity.Id, lastVisitedTicks, (i, l) => lastVisitedTicks);
                }

                var count = ExecFoundSaveGameScripts(playfieldData, entityScriptData,
                    GetPathTo(Enum.GetName(typeof(EntityType), entity.Type)),
                    GetPathTo(entity.Name),
                    GetPathTo(playfieldData.Playfield.Name),
                    GetPathTo(playfieldData.Playfield.Name, Enum.GetName(typeof(EntityType), entity.Type)),
                    GetPathTo(playfieldData.Playfield.Name, entity.Name),
                    GetPathTo(entity.Id.ToString()),
                    SaveGamesScripts.MainScriptPath
                    );

                Log($"ProcessAllSaveGameScripts: #{count}", LogLevel.Debug);

                return count;
            }
            catch (Exception error)
            {
                if (Configuration.Current.ScriptTrackingError) File.AppendAllText(GetTrackingFileName(entity, "SaveGameScript") + ".error", error.ToString());
                return 0;
            }
        }

        private string GetPathTo(params string[] pathParts)
        {
            var path = new List<string>() { SaveGamesScripts.MainScriptPath };
            path.AddRange(pathParts);

            try  { return Path.Combine(path.ToArray()); }
            catch{ return null;                         }
        }

        public int ExecFoundSaveGameScripts(PlayfieldScriptData playfieldData, ScriptSaveGameRootData entityScriptData, params string[] scriptLocations)
        {
            scriptLocations                      .ForEach(L => Log($"SaveGameScriptLocation: {L}",  LogLevel.Debug));
            SaveGamesScripts.SaveGameScripts.Keys.ForEach(L => Log($"SaveGameScripts: {L}",         LogLevel.Debug));

            int count = 0;
            scriptLocations
                .Where(S => !string.IsNullOrEmpty(S))
                .ForEach(S =>
                {
                    if (playfieldData.PauseScripts) return;

                    var path = S.NormalizePath();

                    if (SaveGamesScripts.SaveGameScripts.TryGetValue(path + ".HBS", out var hbsCode))
                    {
                        var data = new ScriptSaveGameRootData(entityScriptData)
                        {
                            ScriptLanguage = ScriptLanguage.Handlebar,
                            Script         = hbsCode.ToString(),
                            ScriptId       = entityScriptData.E.Id + "/" + S,
                            ScriptPath     = Path.GetDirectoryName(path)
                        };
                        AddTargetsAndDisplayType(data, Path.GetFileNameWithoutExtension(S) + "*");
                        playfieldData.ScriptExecQueue.Add(data);

                        Interlocked.Increment(ref count);
                    }
                    else if (SaveGamesScripts.SaveGameScripts.TryGetValue(path + ".CS", out var csCode))
                    {
                        var data = new ScriptSaveGameRootData(entityScriptData)
                        {
                            ScriptLanguage = ScriptLanguage.Cs,
                            Script         = csCode.ToString(),
                            ScriptId       = entityScriptData.E.Id + "/" + S,
                            ScriptPath     = Path.GetDirectoryName(path)
                        };
                        AddTargetsAndDisplayType(data, Path.GetFileNameWithoutExtension(S) + "*");
                        playfieldData.ScriptExecQueue.Add(data);

                        Interlocked.Increment(ref count);
                    }
                    else if (SaveGamesScripts.SaveGameScripts.TryGetValue(path + ".DLL", out var dllCode))
                    {
                        if(dllCode is PreCompiledScript preCompiledScript && preCompiledScript.MainMethod != null)
                        {
                            var data = new ScriptSaveGameRootData(entityScriptData)
                            {
                                ScriptLanguage = ScriptLanguage.CompiledDll,
                                MainMethod     = preCompiledScript.MainMethod,
                                ScriptId       = entityScriptData.E.Id + "/" + S,
                                ScriptPath     = Path.GetDirectoryName(path)
                            };
                            AddTargetsAndDisplayType(data, Path.GetFileNameWithoutExtension(S) + "*");
                            playfieldData.ScriptExecQueue.Add(data);

                            Interlocked.Increment(ref count);
                        }
                    }
                    else
                    {
                        SaveGamesScripts.SaveGameScripts
                            .Where(F => Path.GetDirectoryName(F.Key) == path)
                            .ForEach(F => {
                                if (F.Value is PreCompiledScript preCompiledScript)
                                {
                                    if (preCompiledScript.MainMethod != null)
                                    {
                                        var data = new ScriptSaveGameRootData(entityScriptData)
                                        {
                                            ScriptLanguage  = ScriptLanguage.CompiledDll, 
                                            MainMethod      = preCompiledScript.MainMethod,
                                            ScriptId        = entityScriptData.E.Id + "/" + F.Key,
                                            ScriptPath      = Path.GetDirectoryName(F.Key)
                                        };
                                        AddTargetsAndDisplayType(data, Path.GetFileNameWithoutExtension(F.Key) + "*");
                                        playfieldData.ScriptExecQueue.Add(data);

                                        Interlocked.Increment(ref count);
                                    }
                                }
                                else if(!Path.GetExtension(F.Key).Equals(".dll", StringComparison.InvariantCultureIgnoreCase))
                                {
                                    var data = new ScriptSaveGameRootData(entityScriptData)
                                    {
                                        ScriptLanguage  = Path.GetExtension(F.Key).Equals(".cs", StringComparison.InvariantCultureIgnoreCase) ? ScriptLanguage.Cs : ScriptLanguage.Handlebar,
                                        Script          = F.Value.ToString(),
                                        ScriptId        = entityScriptData.E.Id + "/" + F.Key,
                                        ScriptPath      = Path.GetDirectoryName(F.Key)
                                    };
                                    AddTargetsAndDisplayType(data, Path.GetFileNameWithoutExtension(F.Key) + "*");
                                    playfieldData.ScriptExecQueue.Add(data);

                                    Interlocked.Increment(ref count);
                                }
                            });
                    }
                });

            return count;
        }

        private static void AddTargetsAndDisplayType(IScriptRootData data, string targets)
        {
            if (targets.StartsWith("["))
            {
                var typeEnd = targets.IndexOf(']');
                if(typeEnd > 0)
                {
                    var s = targets.Substring(1, typeEnd - 1);
                    var appendAtEnd = s.EndsWith("+");
                    int.TryParse(appendAtEnd ? s.Substring(0, s.Length - 1) : s.Substring(1), out int Lines);
                    data.DisplayType = new DisplayOutputConfiguration() { AppendAtEnd = appendAtEnd, Lines = Lines };

                    targets = targets.Substring(typeEnd + 1);
                }
            }

            data.LcdTargets.AddRange(data.E.S.AllCustomDeviceNames.GetUniqueNames(targets).Where(N => !IsScriptLCD(N)));
        }

        public void ProcessScript<T>(PlayfieldScriptData playfieldData, T data) where T : IScriptRootData
        {
            try
            {
                if (playfieldData.PauseScripts) return;
                //Log($"ProcessScript [{data.ScriptId}] {data.ScriptLanguage} -> {data}", LogLevel.Debug);

                var stringBlankLines = data.ScriptLanguage == ScriptLanguage.Handlebar ? StringSplitOptions.RemoveEmptyEntries : StringSplitOptions.None;

                var result = 
                    (data.ScriptLanguage == ScriptLanguage.Handlebar
                        ? ExecuteHandlebarScript(playfieldData, data, data.Script)
                        : data.ScriptLanguage == ScriptLanguage.CompiledDll
                            ? ExecuteMainMethodScript (playfieldData, data, (data as ScriptSaveGameRootData).MainMethod)
                            : ExecuteCsScript         (playfieldData, data, data.Script)
                    )
                    ?.Split(new[] { '\n' }, stringBlankLines)
                    ?.ToList();

                if (result == null) return;

                if (result.Count > 0 && result[0].StartsWith(TargetsKeyword))
                {
                    AddTargetsAndDisplayType(data, result[0].Substring(TargetsKeyword.Length));
                    result = result.Skip(1).ToList();
                }

                if (data.DisplayType != null) result = result
                                                .SkipWhile(string.IsNullOrWhiteSpace)
                                                .Reverse()
                                                .SkipWhile(string.IsNullOrWhiteSpace)
                                                .Reverse()
                                                .ToList();

                data.LcdTargets?
                    .Select(L => new { Lcd = data.E.S.GetCurrent().GetDevice<ILcd>(L), Name = L })
                    .Where(L => L.Lcd != null)
                    .ForEach(L =>
                    {
                        if (playfieldData.PauseScripts) return;
                        if (data.DisplayType == null && !data.Running) return; // avoid flicker displays with part of informations

                        List<string> saveResult = null;

                        var attrPos = L.Name.IndexOf('[');
                        if (attrPos > 0 && L.Name.EndsWith("]") && result.Count > 0)
                        {
                            saveResult = new List<string>(result);

                            var attrEndPos = L.Name.IndexOfAny(new[] { ']', ';' }, attrPos);
                            if (attrEndPos > 0) { 
                                result[0]                = $"<size={L.Name.Substring(attrPos + 1, attrEndPos - attrPos - 1)}>{result[0]}"; 
                                result[result.Count - 1] = $"{result[result.Count - 1]}</size>"; 
                            }
                        }

                        if (data.DisplayType == null) L.Lcd.SetText(string.Join("\n", result));
                        else
                        {
                            var text = L.Lcd.GetText().Split(new[] { '\n' }, stringBlankLines);

                            L.Lcd.SetText(string.Join("\n", data.DisplayType.AppendAtEnd 
                                    ? text  .Concat(result).TakeLast(data.DisplayType.Lines)
                                    : result.Concat(text  ).Take    (data.DisplayType.Lines)
                                ));
                        }

                        if (data.ColorChanged          ) L.Lcd.SetTextColor      (data.Color);
                        if (data.BackgroundColorChanged) L.Lcd.SetBackgroundColor(data.BackgroundColor);
                        if (data.FontSizeChanged       ) L.Lcd.SetFontSize       (data.FontSize);

                        result = saveResult ?? result;
                    });
            }
            catch (Exception ctrlError)
            {
                if (Configuration.Current.ScriptTrackingError)
                {
                    File.WriteAllText(data is ScriptSaveGameRootData root 
                        ? GetTrackingFileName(root)
                        : GetTrackingFileName(data.E.GetCurrent(), data.Script.GetHashCode().ToString()) + ".error", 
                        ctrlError.ToString());
                }

                if (playfieldData.PauseScripts) return;
                data.LcdTargets?.ForEach(L => data.E.S.GetCurrent().GetDevice<ILcd>(L)?.SetText($"{ctrlError.Message} {DateTime.Now.ToLongTimeString()}"));
            }
        }

        private string ExecuteMainMethodScript<T>(PlayfieldScriptData playfieldData, T data, MethodInfo mainMethod) where T : IScriptRootData
        {
            Log($"ExecuteMainMethodScript [{data.ScriptId}]:{mainMethod}", LogLevel.Debug);

            using var output = new StringWriter();

            var root = data as IScriptRootModData;
            root.ScriptOutput = output;
            string exceptionMessage = null;

            try
            {
                object result = null;

                if (mainMethod != null)
                {
                    if (root.CsRoot is ICsScriptRootFunctions csRoot) csRoot.ScriptRoot = root;
                    result = mainMethod.Invoke(null, new[] { root as IScriptModData });
                }

                if      (result is Action action)                       action();
                else if (result is Action<IScriptModData> simpleaction) simpleaction(root);
                else if (result is Func<IScriptModData, object> func)   output.Write(func(root)?.ToString());
                else if (result is Task task)                           task.RunSynchronously();
                else                                                    output.Write(result?.ToString());

                return exceptionMessage == null ? output.ToString() : $"{exceptionMessage}\n\nScript output up to exception:\n{output}";
            }
            catch (Exception error)
            {
                exceptionMessage = error.ToString();
                return root.IsElevatedScript ? error.ToString() : error.Message;
            }
            finally
            {
                if (!string.IsNullOrEmpty(exceptionMessage))
                {
                    Log($"DLL Run [{root.ScriptId}]:{exceptionMessage}\n{output}", LogLevel.Error);

                    ScriptErrorTracking(root, new[] { exceptionMessage }.ToList());
                }
            }
        }

        public string ExecuteHandlebarScript<T>(PlayfieldScriptData playfieldData, T data, string script)
        {
            if(!playfieldData.LcdCompileCache.TryGetValue(script, out Func<object, string> generator))
            {
                generator = Handlebars.Compile(script);
                playfieldData.LcdCompileCache.TryAdd(script, generator);
            }

            return generator(data);
        }

        public string ExecuteCsScript<T>(PlayfieldScriptData playfieldData, T data, string script) where T : IScriptRootData
        {
            if (playfieldData.LcdCompileCache.TryGetValue(script, out Func<object, string> generator)) return generator(data);
            if (WithinCsCompiler) return string.Empty;

            WithinCsCompiler = true;
            generator = CsCompiler.GetExec(Configuration.Current.CsScriptsAllowedFor, data, script);
            playfieldData.LcdCompileCache.TryAdd(script, generator);
            WithinCsCompiler = false;

            return generator(data);
        }

        public void Game_Exit()
        {
            ModApi.Log("Mod exited:Game_Exit");

            DediLegacyMod?.Game_Exit();

            try
            {
                ModApi.Application.GameEntered          -= Application_GameEntered;
                ModApi.Application.OnPlayfieldLoaded    -= Application_OnPlayfieldLoaded;
                ModApi.Application.OnPlayfieldUnloading -= Application_OnPlayfieldUnloading;
            }
            catch (Exception error) { Log($"Game_Exit: detach events: {error}", LogLevel.Error); }

            try { StopScriptsEvent?.Invoke(this, EventArgs.Empty); }
            catch (Exception error) { Log($"Game_Exit: StopScriptsEvent: {error}", LogLevel.Error); }

            ModApi.Log("Mod exited:Game_Exit finished");
        }

        public void Game_Update()
        {
            DediLegacyMod?.Game_Update();

            if (PlayfieldData == null) return;

            GameUpdateCounter++;
            if (GameUpdateCounter < Configuration.Current.UseEveryNthGameUpdateCycle) return;
            GameUpdateCounter = 0;

            try
            {
                if (PlayfieldData.Count > 0 && !PlayfieldData.Values.Any(PF => PF.PauseScripts) && (DateTime.Now - LastAlive).TotalSeconds > 120) RestartAllScriptsForPlayfieldServer();
            }
            catch (Exception error){ Log($"Game_Update: RestartAllScriptsForPlayfieldServer: {error}", LogLevel.Error); }

            try{ ScriptExecQueue.Exec(PlayfieldData.Values, Configuration.Current.ScriptsParallelExecution, Configuration.Current.ScriptsSyncExecution); }
            catch (Exception error) { Log($"Game_Update: ScriptExecQueue.Exec: {error}", LogLevel.Error); }

            try { PlayfieldData.Values.ForEach(PF => ConveyorHelpers.HandleMoveLostItems(PF)); }
            catch (Exception error) { Log($"Game_Update: HandleMoveLostItems: {error}", LogLevel.Error); }
        }

        public static void RestartAllScriptsForPlayfieldServer()
        {
            StopScriptsEvent?.Invoke(EmpyrionScriptingInstance, EventArgs.Empty);
            EmpyrionScriptingInstance?.PlayfieldData.Values.ForEach(PF => PF.PauseScripts = false);
            Configuration.Load();
            ModApi?.Log($"EmpyrionScripting Mod.Restart Threads: {EmpyrionScriptingInstance.LastAlive} <-> {DateTime.Now} : {Configuration.Current.LogLevel}");
            EmpyrionScriptingInstance.StartAllScriptsForPlayfieldServer();
        }

        // called for legacy game events (e.g. Event_Player_ChangedPlayfield) and answers to requests (e.g. Event_Playfield_Stats)
        public void Game_Event(CmdId eventId, ushort seqNr, object data)
        {
            Log($"EmpyrionScripting Mod: Game_Event {eventId} {seqNr} {data}", LogLevel.Debug);
            DediLegacyMod?.Game_Event(eventId, seqNr, data);
        }

        public void Dispose()
        {
            ModApi?.Log("EmpyrionScripting Mod: Dispose");
        }
    }

}
