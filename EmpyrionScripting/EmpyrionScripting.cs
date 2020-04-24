using Eleon.Modding;
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
using System.Threading;
using System.Threading.Tasks;

namespace EmpyrionScripting
{

    public sealed partial class EmpyrionScripting : ModInterface, IMod, IDisposable
    {
        public static event EventHandler StopScriptsEvent;

        private const string TargetsKeyword  = "Targets:";
        private const string ScriptKeyword   = "Script:";
        private const string CsKeyword       = "C#:";
        ModGameAPI legacyApi;

        public static EmpyrionScripting EmpyrionScriptingInstance { get; set; }
        public static ItemInfos ItemInfos { get; set; }
        public string SaveGameModPath { get; set; }
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

        public EmpyrionScripting()
        {
            EmpyrionScriptingInstance     = this;
            EmpyrionConfiguration.ModName = "EmpyrionScripting";
            DeviceLock     .Log           = Log;
            ConveyorHelpers.Log           = Log;
            ScriptExecQueue.Log           = Log;
            SetupHandlebarsComponent();
        }

        public void Init(IModApi modAPI)
        {
            ModApi = modAPI;

            ModApi.Log("EmpyrionScripting Mod started: IModApi");
            try
            {
                SetupHandlebarsComponent();

                Localization    = new Localization(ModApi.Application?.GetPathFor(AppFolder.Content));
                ItemInfos       = new ItemInfos(ModApi.Application?.GetPathFor(AppFolder.Content), Localization);
                SaveGameModPath = Path.Combine(ModApi.Application?.GetPathFor(AppFolder.SaveGame), "Mods", EmpyrionConfiguration.ModName);

                LoadConfiguration();
                SaveGamesScripts = new SaveGamesScripts(modAPI) { SaveGameModPath = SaveGameModPath };
                SaveGamesScripts.ReadSaveGamesScripts();

                TaskTools.Log = ModApi.LogError;

                CsCompiler = new CsCompiler.CsCompiler(SaveGameModPath);

                ModApi.Application.OnPlayfieldLoaded    += Application_OnPlayfieldLoaded;
                ModApi.Application.OnPlayfieldUnloading += Application_OnPlayfieldUnloading;

                StopScriptsEvent += (S, E) =>
                {
                    PlayfieldData.Values.ForEach(P => { 
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

            PlayfieldData.TryAdd(playfield.Name, data = new PlayfieldScriptData(this){
                PlayfieldName = playfield.Name,
                Playfield     = playfield, 
            });

            ModApi.Log($"StartScripts for {playfield.Name} pending");
            TaskTools.Delay(Configuration.Current.DelayStartForNSecondsOnPlayfieldLoad, () => {
                ModApi.Log($"StartScripts for {playfield.Name}");
                data.PauseScripts = false;
            });

            data.Playfield.OnEntityLoaded   += data.Playfield_OnEntityLoaded;
            data.Playfield.OnEntityUnloaded += data.Playfield_OnEntityUnloaded;
        }

        private void Application_OnPlayfieldUnloading(IPlayfield playfield)
        {
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
                });
            }, "ScriptInfos");

        }

        private void DisplayScriptInfos()
        {
            PlayfieldData.Values.ForEach(PF => { 
                if (Configuration.Current.LogLevel != LogLevel.Debug) Log($"ScriptInfos: {PF.ScriptExecQueue.ScriptRunInfo.Count} ExecQueue:{PF.ScriptExecQueue.ExecQueue.Count} WaitForExec:{PF.ScriptExecQueue.WaitForExec.Count}", LogLevel.Message);
                PF.ScriptExecQueue.ScriptRunInfo
                    .OrderBy(I => I.Key)
                    .ForEach(I => Log($"Script: {I.Key,-50} #{I.Value.Count,5} LastStart:{I.Value.LastStart} ExecTime:{I.Value.ExecTime} {(I.Value.RunningInstances > 0 ? $" !!!running!!! {I.Value.RunningInstances} times" : "")}", I.Value.RunningInstances > 0 ? LogLevel.Error : LogLevel.Debug));
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
            if (entity.Type == EntityType.Proxy || playfieldData.PauseScripts) return 0;

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
                            var trackfile = GetTrackingFileName(entity, data.Script.GetHashCode().ToString());
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
                if (Configuration.Current.ScriptTrackingError) File.WriteAllText(GetTrackingFileName(entity, string.Empty) + ".error", error.ToString());
                return 0;
            }
        }

        private string GetTrackingFileName(IEntity entity, string scriptid)
        {
            var trackfile = Path.Combine(SaveGameModPath, "ScriptTracking", entity == null ? "" : entity.Id.ToString(), $"{entity?.Id}-{entity?.Type}-{scriptid}.hbs");
            Directory.CreateDirectory(Path.GetDirectoryName(trackfile));
            return trackfile;
        }

        private int ProcessAllSaveGameScripts(PlayfieldScriptData playfieldData, IEntity entity)
        {
            Log($"ProcessAllSaveGameScripts: {entity.Name}:{entity.Type} Pause:{playfieldData.PauseScripts}", LogLevel.Debug);
            if (entity.Type == EntityType.Proxy || playfieldData.PauseScripts) return 0;

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
                if (Configuration.Current.ScriptTrackingError) File.WriteAllText(GetTrackingFileName(entity, "SaveGameScript") + ".error", error.ToString());
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
            int count = 0;
            scriptLocations
                .Where(S => !string.IsNullOrEmpty(S))
                .ForEach(S =>
                {
                    if (playfieldData.PauseScripts) return;

                    var path = S.NormalizePath();

                    if (SaveGamesScripts.SaveGameScripts.TryGetValue(path + ".hbs", out var hbsCode))
                    {
                        var data = new ScriptSaveGameRootData(entityScriptData)
                        {
                            ScriptLanguage = ScriptLanguage.Handlebar,
                            Script         = hbsCode,
                            ScriptId       = entityScriptData.E.Id + "/" + S,
                            ScriptPath     = Path.GetDirectoryName(path)
                        };
                        playfieldData.ScriptExecQueue.Add(data);

                        Interlocked.Increment(ref count);
                    }
                    else if (SaveGamesScripts.SaveGameScripts.TryGetValue(path + ".cs", out var csCode))
                    {
                        var data = new ScriptSaveGameRootData(entityScriptData)
                        {
                            ScriptLanguage = ScriptLanguage.Cs,
                            Script         = csCode,
                            ScriptId       = entityScriptData.E.Id + "/" + S,
                            ScriptPath     = Path.GetDirectoryName(path)
                        };
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
                                    Script      = F.Value,
                                    ScriptId    = entityScriptData.E.Id + "/" + F.Key,
                                    ScriptPath  = Path.GetDirectoryName(F.Key)
                                };
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

                var result = data.ScriptLanguage == ScriptLanguage.Handlebar
                    ? ExecuteHandlebarScript(playfieldData, data, data.Script).Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries)
                    : ExecuteCsScript       (playfieldData, data, data.Script).Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);

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
                        if (playfieldData.PauseScripts) return;

                        if (data.DisplayType == null) L.SetText(string.Join("\n", result));
                        else
                        {
                            var text = L.GetText().Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);

                            L.SetText(string.Join("\n", data.DisplayType.AppendAtEnd 
                                    ? text  .Concat(result).TakeLast(data.DisplayType.Lines)
                                    : result.Concat(text  ).Take    (data.DisplayType.Lines)
                                ));
                        }

                        if (data.ColorChanged          ) L.SetTextColor      (data.Color);
                        if (data.BackgroundColorChanged) L.SetBackgroundColor(data.BackgroundColor);
                        if (data.FontSizeChanged       ) L.SetFontSize       (data.FontSize);
                    });
            }
            catch (Exception ctrlError)
            {
                if (Configuration.Current.ScriptTrackingError) File.WriteAllText(GetTrackingFileName(data.E.GetCurrent(), data.Script.GetHashCode().ToString()) + ".error", ctrlError.ToString());

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
                generator = CsCompiler.GetExec(Configuration.Current.CsScriptsAllowed, data, script);
                playfieldData.LcdCompileCache.TryAdd(script, generator);
            }                                                      

            return generator(data);
        }

        public void Game_Exit()
        {
            ModApi.Log("Mod exited:Game_Exit");
            StopScriptsEvent?.Invoke(this, EventArgs.Empty);
        }

        public void Game_Update()
        {
            if (PlayfieldData.Count > 0 && !PlayfieldData.Values.Any(PF => PF.PauseScripts) && (DateTime.Now - LastAlive).TotalSeconds > 120) RestartAllScriptsForPlayfieldServer();
            if (!ThreadPool.QueueUserWorkItem(QueueScriptExecuting, null)) Log($"EmpyrionScripting Mod: Game_Update NorThreadPoolFree", LogLevel.Debug);
        }

        private void QueueScriptExecuting(object state)
        {
            PlayfieldData.Values.ForEach(PF => { 
                Log($"QueueScriptExecuting: QueueCount:{PF.ScriptExecQueue.QueueCount} ScriptsCount:{PF.ScriptExecQueue.ScriptsCount}", LogLevel.Debug);
                for (int i = Configuration.Current.ScriptsParallelExecution - 1; i >= 0 && PF.ScriptExecQueue.ExecNext(); i--);
            });
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
