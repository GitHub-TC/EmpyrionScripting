﻿using HandlebarsDotNet;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace EmpyrionScripting.CustomHelpers
{
    public static class EffectHelpers
    {
        public static void IntervallBlockHelper(TextWriter output, HelperOptions options, dynamic context, object[] arguments)
        {
            if (arguments.Length != 1) throw new HandlebarsException("{{intervall seconds}} helper must have exactly one argument: (value)");

            double.TryParse(arguments[0] as string, out double intervall);

            try
            {
                if((TimeSpan.FromTicks(DateTime.Now.Ticks).TotalSeconds % (2 * intervall)) < intervall) options.Template(output, context as object);
                else                                                                                    options.Inverse (output, context as object);
            }
            catch (Exception error)
            {
                output.Write("{{intervall}} error " + error.Message);
            }
        }

        public static void ScrollBlockHelper(TextWriter output, HelperOptions options, dynamic context, object[] arguments)
        {
            if (arguments.Length != 2) throw new HandlebarsException("{{scroll lines delay}} helper must have exactly two argument: (lines) (delay)");

            int.TryParse(arguments[0] as string, out int lines);
            int.TryParse(arguments[1] as string, out int delay);

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
                    var skip = (TimeSpan.FromTicks(DateTime.Now.Ticks).TotalSeconds % (delay * overlapp)) / delay;
                    output.Write(string.Join("\n", textlines.Skip((int)skip).Take(lines)));
                }
            }
            catch (Exception error)
            {
                output.Write("{{scroll}} error " + error.Message);
            }
        }

        public static void ColorHelper(TextWriter output, dynamic context, object[] arguments)
        {
            if (arguments.Length != 2) throw new HandlebarsException("{{color}} helper must have exactly two argument: '@root (rgb hex)'");

            try
            {
                var root = arguments[0] as ScriptRootData;
                int.TryParse(arguments[1] as string, NumberStyles.HexNumber, null, out int color);
                root.Color = new Color((color & 0xff0000) >> 16, (color & 0x00ff00) >> 8, color & 0x0000ff);
            }
            catch (Exception error)
            {
                output.Write("{{color}} error " + error.Message);
            }
        }

        public static void BGColorHelper(TextWriter output, dynamic context, object[] arguments)
        {
            if (arguments.Length != 2) throw new HandlebarsException("{{bgcolor}} helper must have exactly two argument: '@root (rgb hex)'");

            try
            {
                var root = arguments[0] as ScriptRootData;
                int.TryParse(arguments[1] as string, NumberStyles.HexNumber, null, out int color);
                root.BackgroundColor = new Color((color & 0xff0000) >> 16, (color & 0x00ff00) >> 8, color & 0x0000ff);
            }
            catch (Exception error)
            {
                output.Write("{{bgcolor}} error " + error.Message);
            }
        }

        public static void FontSizeHelper(TextWriter output, dynamic context, object[] arguments)
        {
            if (arguments.Length != 2) throw new HandlebarsException("{{fontsize}} helper must have exactly two argument: @root (number)");

            try
            {
                var root = arguments[0] as ScriptRootData;
                int.TryParse(arguments[1] as string, out int fontSize);
                root.FontSize = fontSize;
            }
            catch (Exception error)
            {
                output.Write("{{fontsize}} error " + error.Message);
            }
        }


    }
}
