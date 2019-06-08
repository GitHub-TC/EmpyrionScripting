using Eleon.Modding;
using HandlebarsDotNet;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;

namespace EmpyrionScripting
{
    public class EmpyrionScripting : ModInterface, IMod
    {
        public static event EventHandler StopApplicationEvent;

        private const string TargetsKeyword = "Targets:";
        private const string ScriptKeyword = "Script:";

        ModGameAPI legacyApi;

        ConcurrentDictionary<string, Func<object, string>> LcdCompileCache = new ConcurrentDictionary<string, Func<object, string>>();

        public static ItemInfos ItemInfos { get; set; }
        public static Localization Localization { get; set; }
        public static IModApi ModApi { get; private set; }

        public void Init(IModApi modAPI)
        {
            ModApi = modAPI;

            ModApi.Log("EmpyrionScripting Mod started: IModApi");
            try
            {
                RegisterHelper();

                Localization = new Localization(ModApi.Application?.GetPathFor(AppFolder.Content));
                ItemInfos    = new ItemInfos   (ModApi.Application?.GetPathFor(AppFolder.Content), Localization);

                ModApi.Application.OnPlayfieldLoaded += Application_OnPlayfieldLoaded;
                ModApi.Application.OnPlayfieldUnloaded += Application_OnPlayfieldUnloaded;
            }
            catch (Exception error)
            {
                ModApi.LogError($"EmpyrionScripting Mod init finish: {error}");
            }

            ModApi.Log("EmpyrionScripting Mod init finish");

        }

        public void Shutdown()
        {
        }

        public EmpyrionScripting()
        {
            RegisterHelper();
        }

        private void RegisterHelper()
        {
            Handlebars.Configuration.TextEncoder = null;

            Handlebars.RegisterHelper("test",       CustomHelpers.TestBlockHelper);
            Handlebars.RegisterHelper("datetime",   CustomHelpers.DateTimeHelper);
            Handlebars.RegisterHelper("i18n",       CustomHelpers.I18NHelper);
            Handlebars.RegisterHelper("intervall",  CustomHelpers.IntervallBlockHelper);
            Handlebars.RegisterHelper("color",      CustomHelpers.ColorHelper);
            Handlebars.RegisterHelper("bgcolor",    CustomHelpers.BGColorHelper);
            Handlebars.RegisterHelper("fontsize",   CustomHelpers.FontSizeHelper);
            Handlebars.RegisterHelper("device",     CustomHelpers.DeviceBlockHelper);
            Handlebars.RegisterHelper("items",      CustomHelpers.ItemsBlockHelper);
            Handlebars.RegisterHelper("itemlist",   CustomHelpers.ItemListBlockHelper);
            Handlebars.RegisterHelper("scroll",     CustomHelpers.ScrollBlockHelper);
            Handlebars.RegisterHelper("format",     CustomHelpers.FormatHelper);
            Handlebars.RegisterHelper("move",       CustomHelpers.ItemMoveHelper);
            
        }

        private void Application_OnPlayfieldLoaded(string playfieldName)
        {
            TaskTools.Intervall(1000, UpdateLCDs);
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

        private void UpdateLCDs()
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
                .ForAll(ProcessAllScripts);
        }

        private void ProcessAllScripts(IEntity entity)
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
                            Script = lcd.GetText()
                        };

                        AddTargets(data, N.Substring(ScriptKeyword.Length));
                        ProcessInTextTargets(data);
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

        private void ProcessInTextTargets(ScriptRootData data)
        {
            if (!data.Script.StartsWith(TargetsKeyword)) return;
            var firstLineEndPos = data.Script.IndexOf('\n');
            AddTargets(data, data.Script.Substring(TargetsKeyword.Length, firstLineEndPos - TargetsKeyword.Length));
            data.Script = data.Script.Substring(firstLineEndPos + 1);
        }

        private void AddTargets(ScriptRootData data, string targets)
        {
            data.LcdTargets.AddRange(CustomHelpers.GetUniqueNames(data.E.S, targets).Values.Where(N => !N.StartsWith(ScriptKeyword)));
        }

        private void ProcessScript(ScriptRootData data)
        {
            var lcdTargets = data.LcdTargets.Select(T => data.E.S.GetCurrent().GetDevice<ILcd>(T)).Where(T => T != null).ToArray();

            if (lcdTargets.Length > 0)
            {
                data.FontSize        = lcdTargets[0].GetFontSize();
                data.Color           = lcdTargets[0].GetColor();
                data.BackgroundColor = lcdTargets[0].GetBackground();
            }

            var initFontSize        = data.FontSize;
            var initColor           = data.Color;
            var initBackgroundColor = data.BackgroundColor;

            data.ScriptDebugLcd?.SetText("");
            data.ScriptDebugLcd?.SetText(data.ScriptDebugLcd?.GetText() + $"\nTargets:" + data.LcdTargets.Aggregate("", (s, c) => $"{s};{c}"));

            try
            {
                string result = ExecuteHandlebarScript(data, data.Script);

                data.LcdTargets.ForEach(T =>
                {
                    var targetLCD = data.E.S.GetCurrent().GetDevice<ILcd>(T);
                    if (targetLCD == null) return;

                    targetLCD.SetText(string.Join("\n", result.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries)));
                    if (initColor           != data.Color)              targetLCD.SetColor     (data.Color);
                    if (initBackgroundColor != data.BackgroundColor)    targetLCD.SetBackground(data.BackgroundColor);
                    if (initFontSize        != data.FontSize)           targetLCD.SetFontSize  (data.FontSize);
                });
            }
            catch (Exception ctrlError)
            {
                ModApi.LogError(ctrlError.ToString());
                data.ScriptDebugLcd?.SetText(data.ScriptDebugLcd?.GetText() + $"\n{ctrlError.Message} {DateTime.Now.ToLongTimeString()}");
                data.LcdTargets.ForEach(T => data.E.S.GetCurrent().GetDevice<ILcd>(T)?.SetText($"{ctrlError.Message} {DateTime.Now.ToLongTimeString()}"));
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
