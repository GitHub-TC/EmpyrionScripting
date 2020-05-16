using EmpyrionScripting.CsHelper;
using EmpyrionScripting.DataWrapper;
using HandlebarsDotNet;
using System;
using System.IO;

namespace EmpyrionScripting.CustomHelpers
{
    [HandlebarHelpers]
    public class DialogHelpers
    {

        static bool running = false;

        [HandlebarTag("dialog")]
        public static void DialogHelper(TextWriter output, object rootObject, HelperOptions options, dynamic context, object[] arguments)
        {
            //if (arguments.Length != 2) throw new HandlebarsException("{{devices structure names}} helper must have exactly two argument: (structure) (name;name*;*;name)");

            var player = arguments[0] as LimitedPlayerData;
            //var namesSearch = arguments[1].ToString();

            var root = rootObject as ScriptRootData;


            try
            {
                if (player == null || running || player.Id == 0) return;

                //TaskTools.Delay(10, () => { 
                //running = true;
                //var success = 
                //    EmpyrionScripting.ModApi.Application.ShowDialogBox(player.Id,
                //    new Eleon.Modding.DialogConfig()
                //    {
                //        TitleText = "Title",
                //        BodyText = "Body",
                //        ButtonIdxForEnter = 1,
                //        ButtonIdxForEsc = 2,
                //        ButtonTexts = new[] { "A", "B" },
                //        CloseOnLinkClick = true,
                //        InitialContent = "Init",
                //        MaxChars = 200,
                //        Placeholder = "Placeholder"
                //    },
                //    (buttonIdx, linkId, content, customValue) =>
                //    {
                //        running = false;
                //        EmpyrionScripting.Log($"DialogCallback: Button={buttonIdx} Link={linkId} Content={content}", EmpyrionNetAPIDefinitions.LogLevel.Message);
                //    },
                //    0);

                //if (!success) running = false;

                //EmpyrionScripting.Log($"DialogSuccess:{success}", EmpyrionNetAPIDefinitions.LogLevel.Message);
                //});

                //var uniqueNames = structure.AllCustomDeviceNames.GetUniqueNames(namesSearch);

                //var blocks = uniqueNames
                //    .SelectMany(N => structure.GetCurrent().GetDevicePositions(N)
                //        .Select(V => new BlockData(structure.GetCurrent(), V))).ToArray();
                //if (blocks != null && blocks.Length > 0) options.Template(output, blocks);
                //else options.Inverse(output, context as object);
            }
            catch (Exception error)
            {
                if (!CsScriptFunctions.FunctionNeedsMainThread(error, root)) output.Write("{{devices}} error " + EmpyrionScripting.ErrorFilter(error));
            }
        }
    }
}           
