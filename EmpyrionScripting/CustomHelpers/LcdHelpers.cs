using Eleon.Modding;
using EmpyrionScripting.CsHelper;
using EmpyrionScripting.DataWrapper;
using EmpyrionScripting.Internal.Interface;
using HandlebarsDotNet;
using System;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;
using UnityEngine;

namespace EmpyrionScripting.CustomHelpers
{
    public static class LcdHelpersExtension
    {
        private static Regex colorChange = new Regex("(?<pre>.*)\\[c\\]\\[(?<color>[0-9a-fA-F]+)\\](?<post>.*)");

        public static string FormatToHtml(this string yamlText)
            => colorChange.Replace(yamlText, m => $"{m.Groups["pre"]}<color=#{m.Groups["color"]}>{m.Groups["post"]}").Replace("[-][/c]", "</color>")
                    .Replace("[b]", "<b>").Replace("[/b]", "</b>")
                    .Replace("[i]", "<i>").Replace("[/i]", "</i>")
                    .Replace("[u]", "<u>").Replace("[/u]", "</u>");
    }

    [HandlebarHelpers]
    public static class LcdHelpers
    {
        [HandlebarTag("settext")]
        public static void SetTextHelper(TextWriter output, object rootObject, dynamic context, object[] arguments)
        {
            if (arguments.Length != 2) throw new HandlebarsException("{{settext lcddevice text}} helper must have exactly two argument: (structure) (text)");

            var root = rootObject as IScriptRootData;
            if (!root.Running) return; // avoid flicker displays with part of informations

            try
            {
                var block = arguments[0] as BlockData;
                var lcd   = block?.GetStructure()?.GetDevice<ILcd>(block.Position);
                var text  = arguments[1]?.ToString();

                lcd?.SetText(text);
            }
            catch (Exception error)
            {
                if (!CsScriptFunctions.FunctionNeedsMainThread(error, root)) output.Write("{{settext}} error " + EmpyrionScripting.ErrorFilter(error));
            }
        }

        [HandlebarTag("settextblock")]
        public static void SetTextBlockHelper(TextWriter output, object rootObject, HelperOptions options, dynamic context, object[] arguments)
        {
            if (arguments.Length != 1) throw new HandlebarsException("{{settextblock lcddevice}} helper must have exactly one argument: (structure)");

            var root = rootObject as IScriptRootData;
            if (!root.Running) return; // avoid flicker displays with part of informations

            try
            {
                var block = arguments[0] as BlockData;
                var lcd   = block?.GetStructure()?.GetDevice<ILcd>(block.Position);

                using var text = new StringWriter();
                options.Template(text, context as object);

                if (!root.Running) return; // avoid flicker displays with part of informations
                lcd?.SetText(text.ToString());
            }
            catch (Exception error)
            {
                if (!CsScriptFunctions.FunctionNeedsMainThread(error, root)) output.Write("{{settextblock}} error " + EmpyrionScripting.ErrorFilter(error));
            }
        }

        [HandlebarTag("gettext")]
        public static void GetTextHelper(TextWriter output, object root, HelperOptions options, dynamic context, object[] arguments)
        {
            if (arguments.Length != 1) throw new HandlebarsException("{{gettext lcddevice}} helper must have exactly one argument: (structure)");

            try
            {
                var block = arguments[0] as BlockData;
                var lcd   = block?.GetStructure()?.GetDevice<ILcd>(block.Position);

                if (lcd == null) options.Inverse (output, context as object);
                else             options.Template(output, lcd.GetText());
            }
            catch (Exception error)
            {
                if (!CsScriptFunctions.FunctionNeedsMainThread(error, root)) output.Write("{{settext}} error " + EmpyrionScripting.ErrorFilter(error));
            }
        }

        [HandlebarTag("setfontsize")]
        public static void SetFontSizeHelper(TextWriter output, object root, dynamic context, object[] arguments)
        {
            if (arguments.Length != 2) throw new HandlebarsException("{{setfontsize lcddevice size}} helper must have exactly two argument: (structure) (size)");

            try
            {
                var block = arguments[0] as BlockData;
                var lcd = block?.GetStructure()?.GetDevice<ILcd>(block.Position);
                int.TryParse(arguments[1]?.ToString(), out var size);

                lcd?.SetFontSize(size);
            }
            catch (Exception error)
            {
                if (!CsScriptFunctions.FunctionNeedsMainThread(error, root)) output.Write("{{setfontsize}} error " + EmpyrionScripting.ErrorFilter(error));
            }
        }

        [HandlebarTag("setcolor")]
        public static void SetColorHelper(TextWriter output, object root, dynamic context, object[] arguments)
        {
            if (arguments.Length != 2) throw new HandlebarsException("{{setcolor lcddevice color}} helper must have exactly two argument: (structure) (rgb color)");

            try
            {
                var block = arguments[0] as BlockData;
                var lcd = block?.GetStructure()?.GetDevice<ILcd>(block.Position);

                int.TryParse(arguments[1]?.ToString(), NumberStyles.HexNumber, null, out int color);
                lcd?.SetTextColor(new Color((color & 0xff0000) >> 16, (color & 0x00ff00) >> 8, color & 0x0000ff));
            }
            catch (Exception error)
            {
                if (!CsScriptFunctions.FunctionNeedsMainThread(error, root)) output.Write("{{setcolor}} error " + EmpyrionScripting.ErrorFilter(error));
            }
        }

        [HandlebarTag("setbgcolor")]
        public static void SetBGColorHelper(TextWriter output, object root, dynamic context, object[] arguments)
        {
            if (arguments.Length != 2) throw new HandlebarsException("{{setbgcolor lcddevice color}} helper must have exactly two argument: (structure) (rgb color)");

            try
            {
                var block = arguments[0] as BlockData;
                var lcd = block?.GetStructure()?.GetDevice<ILcd>(block.Position);

                int.TryParse(arguments[1]?.ToString(), NumberStyles.HexNumber, null, out int color);
                lcd?.SetBackgroundColor(new Color((color & 0xff0000) >> 16, (color & 0x00ff00) >> 8, color & 0x0000ff));
            }
            catch (Exception error)
            {
                if (!CsScriptFunctions.FunctionNeedsMainThread(error, root)) output.Write("{{setbgcolor}} error " + EmpyrionScripting.ErrorFilter(error));
            }
        }

    }
}
