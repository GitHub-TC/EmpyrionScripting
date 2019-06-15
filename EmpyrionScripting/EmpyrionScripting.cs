using Eleon.Modding;
using EmpyrionNetAPITools;
using EmpyrionScripting.CustomHelpers;
using EmpyrionScripting.DataWrapper;
using HandlebarsDotNet;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace EmpyrionScripting
{
    public class Configuration
    {
        public int InGameScriptsIntervallMS { get; set; } = 1000;
        public int SaveGameScriptsIntervallMS { get; set; } = 1000;
        [JsonConverter(typeof(StringEnumConverter))]
        public DeviceLock.Locking Locking { get; set; } = DeviceLock.Locking.AlwaysLock;
    }

    public class EmpyrionScripting : ModInterface, IMod
    {
        public static event EventHandler StopApplicationEvent;

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

                ModApi.Application.OnPlayfieldLoaded += Application_OnPlayfieldLoaded;
                ModApi.Application.OnPlayfieldUnloaded += Application_OnPlayfieldUnloaded;
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
        }

        private void SetupHandlebarsComponent()
        {
            Handlebars.Configuration.TextEncoder = null;
            HelpersTools.ScanHandlebarHelpers();
        }

        private void Application_OnPlayfieldLoaded(string playfieldName)
        {
            TaskTools.Intervall(Configuration.Current.InGameScriptsIntervallMS,   () => UpdateScripts(ProcessAllInGameScripts));
            TaskTools.Intervall(Configuration.Current.SaveGameScriptsIntervallMS, () => UpdateScripts(ProcessAllSaveGameScripts));
        }

        private void Application_OnPlayfieldUnloaded(string playfieldName)
        {
            StopApplicationEvent.Invoke(this, EventArgs.Empty);
        }

        // Called once early when the game starts (but not again if player quits from game to title menu and starts (or resumes) a game again
        // Hint: treat this like a constructor for your mod
        public void Game_Start(ModGameAPI legacyAPI)
        {
            legacyApi = legacyAPI;
            legacyApi?.Console_Write("EmpyrionScripting Mod started: Game_Start");
        }

        private void UpdateScripts(Action<IEntity> process)
        {
            if (ModApi.Playfield          == null) return;
            if (ModApi.Playfield.Entities == null) return;

            ModApi.Playfield.Entities
                .Values
                .Where(E => E.Type == EntityType.BA ||
                            E.Type == EntityType.CV ||
                            E.Type == EntityType.SV || 
                            E.Type == EntityType.HV)
                .AsParallel()
                .ForAll(process);
        }

        private void ProcessAllInGameScripts(IEntity entity)
        {

            try
            {
                var entityScriptData = new ScriptRootData(ModApi.Playfield, entity);

                var deviceNames = entityScriptData.E.S.AllCustomDeviceNames.Where(N => N.StartsWith(ScriptKeyword)).ToArray();
                //ModApi.Log($"UpdateLCDs ({entity.Id}/{entity.Name}):LCDs: {deviceNames.Aggregate(string.Empty, (N, S) => N + ";" + S)}");

                Parallel.ForEach(deviceNames, N =>
                {
                    var lcd = entity.Structure.GetDevice<ILcd>(N);
                    if (lcd == null) return;

                    //ModApi.Log($"UpdateLCDs Test ({entity.Id}/{entity.Name}/{entity.Type}):[{i}]{lcdText}");// + entity.Structure.GetDeviceTypeNames().Aggregate("", (s, l) => s + "\n" + l));
                    try
                    {
                        var data = new ScriptRootData(entityScriptData)
                        {
                            Script = lcd.GetText(),
                            Error  = L,
                        };

                        AddTargetsAndDisplayType(data, N.Substring(ScriptKeyword.Length));
                        ProcessScript(data);
                    }
                    catch //(Exception lcdError)
                    {
                        //ModApi.Log($"UpdateLCDs ({entity.Id}/{entity.Name}):LCD: {lcdError}");
                    }
                });
            }
            catch (Exception error)
            {
                ModApi.LogError($"ProcessAllScripts ({entity.Id}/{entity.Name}): {error}");
            }
        }

        private void ProcessAllSaveGameScripts(IEntity entity)
        {

            try
            {
                var entityScriptData = new ScriptSaveGameRootData(ModApi.Playfield, entity)
                {
                    MainScriptPath = SaveGamesScripts.MainScriptPath
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
                ModApi.Log($"SaveGameScript ({entity.Id}/{entity.Name}):{error}");
            }
        }

        public void ExecFoundSaveGameScripts(ScriptSaveGameRootData entityScriptData, params string[] scriptLocations)
        {
            scriptLocations
                .AsParallel()
                .ForAll(S =>
                {
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

            data.LcdTargets.AddRange(data.E.S.GetUniqueNames(targets).Values.Where(N => !N.StartsWith(ScriptKeyword)));
        }

        private void ProcessScript<T>(T data) where T : ScriptRootData
        {
            try
            {
                var result = ExecuteHandlebarScript(data, data.Script).Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);

                if (result.Length > 0 && result[0].StartsWith(TargetsKeyword))
                {
                    AddTargetsAndDisplayType(data, result[0].Substring(TargetsKeyword.Length));
                    result = result.Skip(1).ToArray();
                }

                data.LcdTargets
                    .Select(L => data.E.S.GetCurrent().GetDevice<ILcd>(L))
                    .Where(L => L != null)
                    .ForEach(L =>
                    {
                        if (data.DisplayType == null) L.SetText(string.Join("\n", result));
                        else
                        {
                            var text    = L.GetText().Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);

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
                ModApi.LogError(ctrlError.ToString());
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
            StopApplicationEvent?.Invoke(this, EventArgs.Empty);

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
