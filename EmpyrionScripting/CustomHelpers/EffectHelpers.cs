using EmpyrionScripting.Interface;
using HandlebarsDotNet;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using UnityEngine;

namespace EmpyrionScripting.CustomHelpers
{
    [HandlebarHelpers]
    public static class EffectHelpers
    {
        [HandlebarTag("intervall")]
        public static void IntervallBlockHelper(TextWriter output, object rootObject, HelperOptions options, dynamic context, object[] arguments)
        {
            if (arguments.Length != 1) throw new HandlebarsException("{{intervall seconds}} helper must have exactly one argument: (value)");

            var root = rootObject as IScriptRootData;
            double.TryParse(arguments[0]?.ToString(), out double intervall);

            try
            {
                if(root.CycleCounter % (2 * intervall) < intervall) options.Template(output, context as object);
                else                                                options.Inverse (output, context as object);
            }
            catch (Exception error)
            {
                output.Write("{{intervall}} error " + EmpyrionScripting.ErrorFilter(error));
            }
        }

        [HandlebarTag("scroll")]
        public static void ScrollBlockHelper(TextWriter output, object rootObject, HelperOptions options, dynamic context, object[] arguments)
        {
            if (arguments.Length != 2 && arguments.Length != 3) throw new HandlebarsException("{{scroll lines delay [step]}} helper must have at least two argument: (lines) (delay) [step]");

            var root = rootObject as IScriptRootData;
            int.TryParse(arguments[0]?.ToString(), out int lines);
            int.TryParse(arguments[1]?.ToString(), out int delay);
            if(!int.TryParse(arguments.Get(2)?.ToString(), out var step)) step = 1;

            try
            {
                var content = new StringWriter();
                options.Template(content, context as object);

                Scroll(root, content.ToString(), lines, delay, step).ForEach(L => output.Write($"{L}\n"));
            }
            catch (Exception error)
            {
                output.Write("{{scroll}} error " + EmpyrionScripting.ErrorFilter(error));
            }
        }

        public static IList<string> Scroll(IScriptRootData root, string content, int lines, int delay, int step)
        {
            var textlines = content.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
            var outputLines = new List<string>(textlines);
            var overlapp = textlines.Length - lines;
            if (overlapp > 0)
            {
                var skip = root.CycleCounter / delay * step % textlines.Length;
                outputLines = textlines.Skip(skip).Take(lines).ToList();
                outputLines.AddRange(textlines.Take(skip + lines - textlines.Length));
            }

            return outputLines;
        }

        [HandlebarTag("selectlines")]
        public static void SelectLinesBlockHelper(TextWriter output, object rootObject, HelperOptions options, dynamic context, object[] arguments)
        {
            if (arguments.Length != 2 && arguments.Length != 3) throw new HandlebarsException("{{selectlines lines from to}} helper must have at least two argument: (lines) (from) [to]");

            var root = rootObject as IScriptRootData;
            int.TryParse(arguments[0]?.ToString(), out int lines);
            int.TryParse(arguments[1]?.ToString(), out int from);
            int.TryParse(arguments.Get(2)?.ToString(), out int to);

            try
            {
                var content = new StringWriter();
                options.Template(content, context as object);
                var textlines = content.ToString().Split('\n');
                var overlapp = textlines.Length - lines;
                if (overlapp <= 0)
                {
                    output.Write(content.ToString());
                }
                else
                {
                    output.Write(string.Join("\n", textlines.Skip(from).Take(to)));
                    output.Write("\n");
                }
            }
            catch (Exception error)
            {
                output.Write("{{selectlines}} error " + EmpyrionScripting.ErrorFilter(error));
            }
        }

        [HandlebarTag("color")]
        public static void ColorHelper(TextWriter output, object rootObject, dynamic context, object[] arguments)
        {
            if (arguments.Length != 1) throw new HandlebarsException("{{color}} helper must have exactly one argument: '(rgb hex)'");

            try
            {
                var root = rootObject as IScriptRootData;
                int.TryParse(arguments[0]?.ToString(), NumberStyles.HexNumber, null, out int color);
                root.Color        = new Color((color & 0xff0000) >> 16, (color & 0x00ff00) >> 8, color & 0x0000ff);
                root.ColorChanged = true;
            }
            catch (Exception error)
            {
                output.Write("{{color}} error " + EmpyrionScripting.ErrorFilter(error));
            }
        }

        [HandlebarTag("bgcolor")]
        public static void BGColorHelper(TextWriter output, object rootObject, dynamic context, object[] arguments)
        {
            if (arguments.Length != 1) throw new HandlebarsException("{{bgcolor}} helper must have exactly one argument: '(rgb hex)'");

            try
            {
                var root = rootObject as IScriptRootData;
                int.TryParse(arguments[0]?.ToString(), NumberStyles.HexNumber, null, out int color);
                root.BackgroundColor        = new Color((color & 0xff0000) >> 16, (color & 0x00ff00) >> 8, color & 0x0000ff);
                root.BackgroundColorChanged = true;
            }
            catch (Exception error)
            {
                output.Write("{{bgcolor}} error " + EmpyrionScripting.ErrorFilter(error));
            }
        }

        [HandlebarTag("fontsize")]
        public static void FontSizeHelper(TextWriter output, object rootObject, dynamic context, object[] arguments)
        {
            if (arguments.Length != 1) throw new HandlebarsException("{{fontsize}} helper must have exactly one argument: (number)");

            try
            {
                var root = rootObject as IScriptRootData;
                int.TryParse(arguments[0]?.ToString(), out int fontSize);
                root.FontSize        = fontSize;
                root.FontSizeChanged = true;
            }
            catch (Exception error)
            {
                output.Write("{{fontsize}} error " + EmpyrionScripting.ErrorFilter(error));
            }
        }


    }
}
