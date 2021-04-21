using Eleon.Modding;
using EmpyrionNetAPIAccess;
using EmpyrionNetAPIDefinitions;
using EmpyrionNetAPITools;
using EmpyrionNetAPITools.Extensions;
using EmpyrionScripting.CustomHelpers;
using EmpyrionScripting.DataWrapper;
using EmpyrionScripting.Interface;
using EmpyrionScripting.Internal.Interface;
using HandlebarsDotNet;
using Microsoft.CodeAnalysis;
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
using StaticCsCompiler = EmpyrionScripting.CsCompiler;

namespace EmpyrionScripting
{

    public sealed partial class EmpyrionScripting : ModInterface, IMod, IDisposable
    {
        public static event EventHandler StopScriptsEvent;

        private const string TargetsKeyword = "Targets:";
        private const string ScriptKeyword = "Script:";
        private const string CsKeyword = "C#:";
        ModGameAPI legacyApi;

        public static EmpyrionScripting EmpyrionScriptingInstance { get; set; }
        public static ConfigEcfAccess ConfigEcfAccess { get; set; }
        public static ItemInfos ItemInfos { get; set; }
        public static string SaveGameModPath { get; set; }
        public static ConfigurationManager<Configuration> Configuration { get; set; } = new ConfigurationManager<Configuration>() { Current = new Configuration() };
        public static Localization Localization { get; set; }
        public static IModApi ModApi { get; set; }
        public SaveGamesScripts SaveGamesScripts { get; set; }
        public string L { get; private set; }
        public DateTime LastAlive { get; private set; }
        public int InGameScriptsCount { get; private set; }
        public int SaveGameScriptsCount { get; private set; }

        public CsCompiler.CsCompiler CsCompiler { get; set; }

        public ConcurrentDictionary<string, PlayfieldScriptData> PlayfieldData { get; set; } = new ConcurrentDictionary<string, PlayfieldScriptData>();
        public static string ScriptingModInfoData { get; private set; } = string.Empty;
        public static ConcurrentDictionary<string, string> ScriptingModScriptsInfoData { get; } = new ConcurrentDictionary<string, string>();

        private static readonly Assembly CurrentAssembly = Assembly.GetAssembly(typeof(EmpyrionScripting));
        public static string Version { get; } = $"{CurrentAssembly.GetAttribute<AssemblyTitleAttribute>()?.Title } by {CurrentAssembly.GetAttribute<AssemblyCompanyAttribute>()?.Company} Version:{CurrentAssembly.GetAttribute<AssemblyFileVersionAttribute>()?.Version}";

        public EmpyrionScripting()
        {
            EmpyrionScriptingInstance     = this;
            EmpyrionConfiguration.ModName = "EmpyrionScripting";
            DeviceLock     .Log           = Log;
            ConveyorHelpers.Log           = Log;
            ScriptExecQueue.Log           = Log;
            ConfigEcfAccess.Log           = Log;
            Localization   .Log           = Log;
            SetupHandlebarsComponent();
        }

        public void Init(IModApi modAPI)
        {
            ModApi = modAPI;

            ModApi.Log("EmpyrionScripting Mod started: IModApi");
            try
            {
                SetupHandlebarsComponent();

                Localization = new Localization(ModApi.Application?.GetPathFor(AppFolder.Content), EmpyrionConfiguration.DedicatedYaml.CustomScenarioName);
                SaveGameModPath = Path.Combine(ModApi.Application?.GetPathFor(AppFolder.SaveGame), "Mods", EmpyrionConfiguration.ModName);
                ModApi.Application.GameEntered += Application_GameEntered;

                LoadConfiguration();
                SaveGamesScripts = new SaveGamesScripts(modAPI) { SaveGameModPath = SaveGameModPath };
                SaveGamesScripts.ReadSaveGamesScripts();

                TaskTools.Log = ModApi.LogError;
                StaticCsCompiler.CsCompiler.Log = Log;

                CsCompiler = new CsCompiler.CsCompiler(SaveGameModPath);
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

        private void Application_GameEntered(bool hasEntered)
        {
            ModApi.Log($"Application_GameEntered {hasEntered}");
            if (hasEntered) InitEcfConfigData();
            ModApi.Log("Application_GameEntered init finish");
        }

        private static void InitEcfConfigData()
        {
            if (ConfigEcfAccess != null) return;

            ConfigEcfAccess = new ConfigEcfAccess();
            ConfigEcfAccess.ReadConfigEcf(
                ModApi.Application?.GetPathFor(AppFolder.Content),
                EmpyrionConfiguration.DedicatedYaml.CustomScenarioName,
                Path.Combine(ModApi.Application?.GetPathFor(AppFolder.SaveGame), "blocksmap.dat"), ModApi);
            ItemInfos = new ItemInfos(ConfigEcfAccess, Localization);
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
            Configuration = new ConfigurationManager<Configuration>()
            {
                ConfigFilename = Path.Combine(SaveGameModPath, "Configuration.json")
            };
            Configuration.Load();
            if (Configuration.LoadException != null || !File.Exists(Configuration.ConfigFilename)) Configuration.Save();
        }

        public void Shutdown()
        {
            ModApi.Log("Mod exited:Shutdown");
            StopScriptsEvent.Invoke(this, EventArgs.Empty);
        }

        private void SetupHandlebarsComponent()
        {
            Handlebars.Configuration.TextEncoder = null;
            HandlebarsHelpers.ScanHandlebarHelpers();
        }

        private void Application_OnPlayfieldLoaded(IPlayfield playfield)
        {
            PlayfieldScriptData data = null;

            InitEcfConfigData();

            PlayfieldData.TryAdd(playfield.Name, data = new PlayfieldScriptData(this){
                PlayfieldName = playfield.Name,
                Playfield     = playfield, 
            });

            UpdateScriptingModInfoData();

            ModApi.Log($"StartScripts for {playfield.Name} pending");
            TaskTools.Delay(Configuration.Current.DelayStartForNSecondsOnPlayfieldLoad, () => {
                ModApi.Log($"StartScripts for {playfield.Name}");
                data.PauseScripts = false;
            });

            data.Playfield.OnEntityLoaded   += data.Playfield_OnEntityLoaded;
            data.Playfield.OnEntityUnloaded += data.Playfield_OnEntityUnloaded;
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
            output.AppendLine($"DetailedScriptsInfoData:{Configuration.Current.DetailedScriptsInfoData}");
            output.AppendLine($"InGameScriptsIntervallMS:{Configuration.Current.InGameScriptsIntervallMS}");
            output.AppendLine($"SaveGameScriptsIntervallMS:{Configuration.Current.SaveGameScriptsIntervallMS}");
            output.AppendLine();
            output.AppendLine(DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss"));

            ScriptingModInfoData = output.ToString();
        }

        private void Application_OnPlayfieldUnloading(IPlayfield playfield)
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

        public void StartAllScriptsForPlayfieldServer()
        {
            ModApi.Log($"StartAllScriptsForPlayfieldServer: InGame:{Configuration.Current.InGameScriptsIntervallMS}ms SaveGame:{Configuration.Current.SaveGameScriptsIntervallMS}ms ");

            if(ModApi.Application.Mode == ApplicationMode.Client || ModApi.Application.Mode == ApplicationMode.SinglePlayer)
            {
                StartScriptIntervall(Configuration.Current.InGameScriptsIntervallMS, () =>
                {
                    if(PlayfieldData.Count == 0 && ModApi.ClientPlayfield != null) Application_OnPlayfieldLoaded(ModApi.ClientPlayfield); 
                }, "CheckClientPlayfield");
            }

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
                    PF.ScriptExecQueue.CheckForEmergencyRestart();
                });
            }, "ScriptInfos");

        }

        private void DisplayScriptInfos()
        {
            UpdateScriptingModInfoData();

            PlayfieldData.Values.ForEach(PF => {
                var output = new StringBuilder();
                var totalExecTime = new TimeSpan();
                Log($"ScriptInfos[{PF.PlayfieldName}]: RunCount:{PF.ScriptExecQueue.ScriptRunInfo.Count} ExecQueue:{PF.ScriptExecQueue.ExecQueue.Count} WaitForExec:{PF.ScriptExecQueue.WaitForExec.Count} Sync:{PF.ScriptExecQueue.ScriptNeedsMainThread.Count(S => S.Value)} TimeLimitReached:{PF.ScriptExecQueue.ScriptLoopTimeLimitReached} Total:{PF.ScriptExecQueue.ScriptNeedsMainThread.Count()}", LogLevel.Message);
                PF.ScriptExecQueue.ScriptRunInfo
                    .OrderBy(I => I.Key)
                    .ForEach(I =>
                    {
                        var line = $"Script: {I.Key,-50} {(PF.ScriptExecQueue.ScriptNeedsMainThread.TryGetValue(I.Key, out var sync) && sync ? ">SYNC<" : "")} #{I.Value.Count,5} LastStart:{I.Value.LastStart} ExecTime:{I.Value.ExecTime} TimeLimitReached:{I.Value.TimeLimitReached} {(I.Value.RunningInstances > 0 ? $" !!!running!!! {I.Value.RunningInstances} times" : "")}";
                        totalExecTime += I.Value.ExecTime;
                        if(Configuration.Current.DetailedScriptsInfoData) output.AppendLine(line);
                        Log(line, I.Value.RunningInstances > 0 ? LogLevel.Error : LogLevel.Debug);
                    });

                output.Insert(0, $"RunCount:{PF.ScriptExecQueue.ScriptRunInfo.Count} ExecQueue:{PF.ScriptExecQueue.ExecQueue.Count} WaitForExec:{PF.ScriptExecQueue.WaitForExec.Count} Sync:{PF.ScriptExecQueue.ScriptNeedsMainThread.Count(S => S.Value)} TimeLimitReached:{PF.ScriptExecQueue.ScriptLoopTimeLimitReached} Total:{PF.ScriptExecQueue.ScriptNeedsMainThread.Count()} TotalExecTime:{totalExecTime}\n");
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

        // Called once early when the game starts (but not again if player quits from game to title menu and starts (or resumes) a game again
        // Hint: treat this like a constructor for your mod
        public void Game_Start(ModGameAPI legacyAPI)
        {
            legacyApi = legacyAPI;
            legacyApi?.Console_Write("EmpyrionScripting Mod started: Game_Start");
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
                var entityScriptData = new ScriptRootData(playfieldData, playfieldData.AllEntities, playfieldData.CurrentEntities, playfieldData.Playfield, entity,
                    playfieldData.PersistendData, (EventStore)playfieldData.EventStore.GetOrAdd(entity.Id, id => new EventStore(entity)));

                var deviceNames = entityScriptData.E.S.AllCustomDeviceNames.Where(N => N.StartsWith(ScriptKeyword) || N.StartsWith(CsKeyword)).ToArray();
                Log($"ProcessAllInGameScripts: #{deviceNames.Length}", LogLevel.Debug);

                int count = 0;
                Parallel.ForEach(deviceNames, N =>
                {
                    if (playfieldData.PauseScripts) return;

                    var lcd = entity.Structure.GetDevice<ILcd>(N);
                    if (lcd == null) return;

                    try
                    {
                        Log($"ProcessAllInGameScripts: {N}", LogLevel.Debug);

                        var data = new ScriptRootData(entityScriptData)
                        {
                            ScriptLanguage  = N.StartsWith(ScriptKeyword) ? ScriptLanguage.Handlebar : ScriptLanguage.Cs,
                            Script          = lcd.GetText(),
                            Error           = L,
                        };

                        AddTargetsAndDisplayType(data, N.Substring(N.IndexOf(':') + 1));

                        if (Configuration.Current.ScriptTracking)
                        {
                            var trackfile = GetTrackingFileName(entity, data);
                            if(!File.Exists(trackfile)) File.WriteAllText(trackfile, data.Script);
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
            var trackfile = Path.Combine(SaveGameModPath, "ScriptTracking", Path.GetFileName(root.ScriptId));
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
                            Script         = hbsCode,
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
                            Script         = csCode,
                            ScriptId       = entityScriptData.E.Id + "/" + S,
                            ScriptPath     = Path.GetDirectoryName(path)
                        };
                        AddTargetsAndDisplayType(data, Path.GetFileNameWithoutExtension(S) + "*");
                        playfieldData.ScriptExecQueue.Add(data);

                        Interlocked.Increment(ref count);
                    }
                    else
                    {
                        SaveGamesScripts.SaveGameScripts
                            .Where(F => Path.GetDirectoryName(F.Key) == path)
                            .ForEach(F => {
                                var data = new ScriptSaveGameRootData(entityScriptData)
                                {
                                    ScriptLanguage  = Path.GetExtension(F.Key).Equals(".cs", StringComparison.InvariantCultureIgnoreCase) ? ScriptLanguage.Cs : ScriptLanguage.Handlebar,
                                    Script          = F.Value,
                                    ScriptId        = entityScriptData.E.Id + "/" + F.Key,
                                    ScriptPath      = Path.GetDirectoryName(F.Key)
                                };
                                AddTargetsAndDisplayType(data, Path.GetFileNameWithoutExtension(F.Key) + "*");
                                playfieldData.ScriptExecQueue.Add(data);

                                Interlocked.Increment(ref count);
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

            data.LcdTargets.AddRange(data.E.S.AllCustomDeviceNames.GetUniqueNames(targets).Where(N => !N.StartsWith(ScriptKeyword) && !N.StartsWith(CsKeyword)));
        }

        public void ProcessScript<T>(PlayfieldScriptData playfieldData, T data) where T : IScriptRootData
        {
            try
            {
                if (playfieldData.PauseScripts) return;

                var stringBlankLines = data.ScriptLanguage == ScriptLanguage.Handlebar ? StringSplitOptions.RemoveEmptyEntries : StringSplitOptions.None;

                var result = 
                    (data.ScriptLanguage == ScriptLanguage.Handlebar
                        ? ExecuteHandlebarScript(playfieldData, data, data.Script)
                        : ExecuteCsScript       (playfieldData, data, data.Script)
                    ).Split(new[] { '\n' }, stringBlankLines)
                    .ToList();

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

                data.LcdTargets
                    .Select(L => new { Lcd = data.E.S.GetCurrent().GetDevice<ILcd>(L), Name = L })
                    .Where(L => L.Lcd != null)
                    .ForEach(L =>
                    {
                        if (playfieldData.PauseScripts) return;
                        if (data.DisplayType == null && data.ScriptLoopTimeLimitReached()) return; // avoid flicker displays with part of informations

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
                data.LcdTargets.ForEach(L => data.E.S.GetCurrent().GetDevice<ILcd>(L)?.SetText($"{ctrlError.Message} {DateTime.Now.ToLongTimeString()}"));
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
            if (!playfieldData.LcdCompileCache.TryGetValue(script, out Func<object, string> generator))
            {
                generator = CsCompiler.GetExec(Configuration.Current.CsScriptsAllowedFor, data, script);
                playfieldData.LcdCompileCache.TryAdd(script, generator);
            }                                                      

            return generator(data);
        }

        public void Game_Exit()
        {
            ModApi.Log("Mod exited:Game_Exit");
            try { StopScriptsEvent?.Invoke(this, EventArgs.Empty); }
            catch (Exception error) { Log($"Game_Exit: StopScriptsEvent: {error}", LogLevel.Error); }
        }

        public void Game_Update()
        {
            if (PlayfieldData == null) return;

            try
            {
                if (PlayfieldData.Count > 0 && !PlayfieldData.Values.Any(PF => PF.PauseScripts) && (DateTime.Now - LastAlive).TotalSeconds > 120) RestartAllScriptsForPlayfieldServer();
            }
            catch (Exception error){ Log($"Game_Update: RestartAllScriptsForPlayfieldServer: {error}", LogLevel.Error); }

            try{ PlayfieldData.Values.ForEach(PF => PF.ScriptExecQueue.ExecNext(Configuration.Current.ScriptsParallelExecution, Configuration.Current.ScriptsSyncExecution)); }
            catch (Exception error) { Log($"Game_Update: ScriptExecQueue.ExecNext: {error}", LogLevel.Error); }
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
        }

        public void Dispose()
        {
            ModApi?.Log("EmpyrionScripting Mod: Dispose");
        }
    }

}
