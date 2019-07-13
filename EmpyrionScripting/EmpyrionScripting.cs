using Eleon.Modding;
using EmpyrionNetAPITools;
using EmpyrionNetAPITools.Extensions;
using EmpyrionScripting.CustomHelpers;
using EmpyrionScripting.DataWrapper;
using HandlebarsDotNet;
using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace EmpyrionScripting
{

    public class EmpyrionScripting : ModInterface, IMod
    {
        public static event EventHandler StopScriptsEvent;

        private const string TargetsKeyword  = "Targets:";
        private const string ScriptKeyword   = "Script:";
        ModGameAPI legacyApi;

        ConcurrentDictionary<string, Func<object, string>> LcdCompileCache = new ConcurrentDictionary<string, Func<object, string>>();
        
        public static ItemInfos ItemInfos { get; set; }
        public string SaveGameModPath { get; set; }
        public static ConfigurationManager<Configuration> Configuration { get; private set; }
        public static Localization Localization { get; set; }
        public static IModApi ModApi { get; private set; }
        public SaveGamesScripts SaveGamesScripts { get; private set; }
        public string L { get; private set; }
        public static bool DeviceLockAllowed => (CycleCounter % Configuration.Current.DeviceLockOnlyAllowedEveryXCycles) == 0;

        public bool StopScripts { get; private set; }
        public IEntity[] CurrentEntities { get; private set; }

        private static int CycleCounter;

        public EmpyrionScripting()
        {
            EmpyrionConfiguration.ModName = "EmpyrionScripting";
            SetupHandlebarsComponent();
        }

        public void Init(IModApi modAPI)
        {
            ModApi = modAPI;

            ModApi.Log("EmpyrionScripting Mod started: IModApi");
            try
            {
                SetupHandlebarsComponent();

                Localization = new Localization(ModApi.Application?.GetPathFor(AppFolder.Content));
                ItemInfos = new ItemInfos(ModApi.Application?.GetPathFor(AppFolder.Content), Localization);
                SaveGameModPath = Path.Combine(ModApi.Application?.GetPathFor(AppFolder.SaveGame), "Mods", EmpyrionConfiguration.ModName);

                LoadConfiguration();
                SaveGamesScripts = new SaveGamesScripts(modAPI) { SaveGameModPath = SaveGameModPath };
                SaveGamesScripts.ReadSaveGamesScripts();

                ModApi.Application.OnPlayfieldLoaded   += Application_OnPlayfieldLoaded;
                ModApi.Application.OnPlayfieldUnloaded += Application_OnPlayfieldUnloaded;

                StopScriptsEvent += (S, E) =>
                {
                    ModApi.Log($"StopScriptsEvent: {(StopScripts ? "always stopped" : "scripts running")}");
                    StopScripts = true;
                };
            }
            catch (Exception error)
            {
                ModApi.LogError($"EmpyrionScripting Mod init finish: {error}");
            }

            ModApi.Log("EmpyrionScripting Mod init finish");

        }

        private void LoadConfiguration()
        {
            ConfigurationManager<Configuration>.Log = ModApi.Log;
            Configuration = new ConfigurationManager<Configuration>()
            {
                ConfigFilename = Path.Combine(SaveGameModPath, "Configuration.json")
            };
            Configuration.Load();
            Configuration.Save();
        }

        public void Shutdown()
        {
            StopScriptsEvent.Invoke(this, EventArgs.Empty);
        }

        private void SetupHandlebarsComponent()
        {
            Handlebars.Configuration.TextEncoder = null;
            HelpersTools.ScanHandlebarHelpers();
        }

        private void Application_OnPlayfieldLoaded(string playfieldName)
        {
            StopScripts = false;
            ModApi.Log($"StartScripts for {playfieldName}");

            StartScriptIntervall(Configuration.Current.InGameScriptsIntervallMS,   () =>
            {
                Interlocked.Increment(ref CycleCounter);
                UpdateScripts(ProcessAllInGameScripts);
            });

            StartScriptIntervall(Configuration.Current.SaveGameScriptsIntervallMS, () => UpdateScripts(ProcessAllSaveGameScripts));
        }

        private void StartScriptIntervall(int intervall, Action action)
        {
            if (intervall > 0)
            {
                var exec = TaskTools.Intervall(intervall, action);
                StopScriptsEvent += (S, A) => exec.Set();
            }
        }

        private void Application_OnPlayfieldUnloaded(string playfieldName)
        {
            ModApi.Log($"StopScripts for {playfieldName}");
            StopScriptsEvent.Invoke(this, EventArgs.Empty);
        }

        // Called once early when the game starts (but not again if player quits from game to title menu and starts (or resumes) a game again
        // Hint: treat this like a constructor for your mod
        public void Game_Start(ModGameAPI legacyAPI)
        {
            legacyApi = legacyAPI;
            legacyApi?.Console_Write("EmpyrionScripting Mod started: Game_Start");
        }

        public static string ErrorFilter(Exception error) => Configuration.Current.LogLevel == EmpyrionNetAPIDefinitions.LogLevel.Debug ? error.ToString() : error.Message;

        private void UpdateScripts(Action<IEntity> process)
        {
            if (ModApi.Playfield          == null) return;
            if (ModApi.Playfield.Entities == null) return;

            CurrentEntities = ModApi.Playfield.Entities
                .Values
                .Where(E => E.Type == EntityType.BA ||
                            E.Type == EntityType.CV ||
                            E.Type == EntityType.SV ||
                            E.Type == EntityType.HV)
                .ToArray();

            CurrentEntities
                .AsParallel()
                .ForAll(process);
        }

        private void ProcessAllInGameScripts(IEntity entity)
        {
            if (entity.Type == EntityType.Proxy || StopScripts) return;

            try
            {
                var entityScriptData = new ScriptRootData(CurrentEntities, ModApi.Playfield, entity);

                var deviceNames = entityScriptData.E.S.AllCustomDeviceNames.Where(N => N.StartsWith(ScriptKeyword)).ToArray();

                Parallel.ForEach(deviceNames, N =>
                {
                    if (StopScripts) return;

                    var lcd = entity.Structure.GetDevice<ILcd>(N);
                    if (lcd == null) return;

                    try
                    {
                        var data = new ScriptRootData(entityScriptData)
                        {
                            Script = lcd.GetText(),
                            Error  = L,
                        };

                        AddTargetsAndDisplayType(data, N.Substring(ScriptKeyword.Length));

                        if (Configuration.Current.ScriptTracking)
                        {
                            var trackfile = GetTrackingFileName(entity, data.Script.GetHashCode().ToString());
                            if(!File.Exists(trackfile)) File.WriteAllText(trackfile, data.Script);
                        }

                        ProcessScript(data);
                    }
                    catch (Exception lcdError)
                    {
                        if(Configuration.Current.LogLevel >= EmpyrionNetAPIDefinitions.LogLevel.Debug)
                            ModApi.Log($"UpdateLCDs ({entity.Id}/{entity.Name}):LCD: {lcdError}");
                    }
                });
            }
            catch (Exception error)
            {
                File.WriteAllText(GetTrackingFileName(entity, string.Empty) + ".error", error.ToString());
            }
        }

        private string GetTrackingFileName(IEntity entity, string scriptid)
        {
            var trackfile = Path.Combine(SaveGameModPath, "ScriptTracking", entity == null ? "" : entity.Id.ToString(), $"{entity?.Id}-{entity?.Type}-{scriptid}.hbs");
            Directory.CreateDirectory(Path.GetDirectoryName(trackfile));
            return trackfile;
        }

        private void ProcessAllSaveGameScripts(IEntity entity)
        {
            if (entity.Type == EntityType.Proxy || StopScripts) return;

            try
            {
                var entityScriptData = new ScriptSaveGameRootData(CurrentEntities, ModApi.Playfield, entity)
                {
                    MainScriptPath = SaveGamesScripts.MainScriptPath,
                    ModApi         = ModApi
                };

                ExecFoundSaveGameScripts(entityScriptData, 
                    Path.Combine(SaveGamesScripts.MainScriptPath, Enum.GetName(typeof(EntityType), entity.Type)),
                    Path.Combine(SaveGamesScripts.MainScriptPath, entity.Name),
                    Path.Combine(SaveGamesScripts.MainScriptPath, ModApi.Playfield.Name),
                    Path.Combine(SaveGamesScripts.MainScriptPath, ModApi.Playfield.Name, Enum.GetName(typeof(EntityType), entity.Type)),
                    Path.Combine(SaveGamesScripts.MainScriptPath, ModApi.Playfield.Name, entity.Name),
                    Path.Combine(SaveGamesScripts.MainScriptPath, entity.Id.ToString())
                    );
            }
            catch (Exception error)
            {
                File.WriteAllText(GetTrackingFileName(entity, "SaveGameScript") + ".error", error.ToString());
            }
        }

        public void ExecFoundSaveGameScripts(ScriptSaveGameRootData entityScriptData, params string[] scriptLocations)
        {
            scriptLocations
                .AsParallel()
                .ForAll(S =>
                {
                    if (StopScripts) return;

                    var path = S.NormalizePath();

                    if (SaveGamesScripts.SaveGameScripts.TryGetValue(path + SaveGamesScripts.ScriptExtension, out var C)) ProcessScript(new ScriptSaveGameRootData(entityScriptData) {
                        Script     = C,
                        ScriptPath = Path.GetDirectoryName(path)
                    });
                    else SaveGamesScripts.SaveGameScripts
                        .Where(F => Path.GetDirectoryName(F.Key) == path)
                        .AsParallel()
                        .ForEach(F => ProcessScript(new ScriptSaveGameRootData(entityScriptData) {
                            Script     = F.Value,
                            ScriptPath = Path.GetDirectoryName(F.Key)
                        }));
                });
        }

        private static void AddTargetsAndDisplayType(ScriptRootData data, string targets)
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

            data.LcdTargets.AddRange(data.E.S.AllCustomDeviceNames.GetUniqueNames(targets).Where(N => !N.StartsWith(ScriptKeyword)));
        }

        private void ProcessScript<T>(T data) where T : ScriptRootData
        {
            try
            {
                if (StopScripts) return;

                var result = ExecuteHandlebarScript(data, data.Script).Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);

                if (result.Length > 0 && result[0].StartsWith(TargetsKeyword))
                {
                    AddTargetsAndDisplayType(data, result[0].Substring(TargetsKeyword.Length));
                    result = result.Skip(1).ToArray();
                }

                if (data.DisplayType != null) result = result
                                                .SkipWhile(string.IsNullOrWhiteSpace)
                                                .Reverse()
                                                .SkipWhile(string.IsNullOrWhiteSpace)
                                                .Reverse()
                                                .ToArray();

                data.LcdTargets
                    .Select(L => data.E.S.GetCurrent().GetDevice<ILcd>(L))
                    .Where(L => L != null)
                    .ForEach(L =>
                    {
                        if (StopScripts) return;

                        if (data.DisplayType == null) L.SetText(string.Join("\n", result));
                        else
                        {
                            var text = L.GetText().Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);

                            L.SetText(string.Join("\n", data.DisplayType.AppendAtEnd 
                                    ? text  .Concat(result).TakeLast(data.DisplayType.Lines)
                                    : result.Concat(text  ).Take    (data.DisplayType.Lines)
                                ));
                        }

                        if (data.ColorChanged          ) L.SetColor     (data.Color);
                        if (data.BackgroundColorChanged) L.SetBackground(data.BackgroundColor);
                        if (data.FontSizeChanged       ) L.SetFontSize  (data.FontSize);
                    });
            }
            catch (Exception ctrlError)
            {
                File.WriteAllText(GetTrackingFileName(data.E.GetCurrent(), data.Script.GetHashCode().ToString()) + ".error", ctrlError.ToString());

                if (StopScripts) return;
                data.LcdTargets.ForEach(L => data.E.S.GetCurrent().GetDevice<ILcd>(L)?.SetText($"{ctrlError.Message} {DateTime.Now.ToLongTimeString()}"));
            }
        }

        public string ExecuteHandlebarScript<T>(T data, string script)
        {
            if(!LcdCompileCache.TryGetValue(script, out Func<object, string> generator))
            {
                generator = Handlebars.Compile(script);
                LcdCompileCache.TryAdd(script, generator);
            }

            return generator(data);
        }

        public void Game_Exit()
        {
            StopScriptsEvent?.Invoke(this, EventArgs.Empty);

            ModApi.Log("Mod exited");
        }

        public void Game_Update()
        {
        }

        // called for legacy game events (e.g. Event_Player_ChangedPlayfield) and answers to requests (e.g. Event_Playfield_Stats)
        public void Game_Event(CmdId eventId, ushort seqNr, object data)
        {
        }

    }

}
