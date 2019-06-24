﻿using Eleon.Modding;
using EmpyrionScripting.DataWrapper;
using HandlebarsDotNet;
using System;
using System.Globalization;
using System.IO;
using UnityEngine;

namespace EmpyrionScripting.CustomHelpers
{
    [HandlebarHelpers]
    public static class LcdHelpers
    {
        [HandlebarTag("settext")]
        public static void SetTextHelper(TextWriter output, dynamic context, object[] arguments)
        {
            if (arguments.Length != 2) throw new HandlebarsException("{{settext lcddevice text}} helper must have exactly two argument: (structure) (text)");

            var block = arguments[0] as BlockData;
            var lcd   = block?.GetStructure()?.GetDevice<ILcd>(block.Position);
            var text  = arguments[1]?.ToString();

            try
            {
                lcd?.SetText(text);
            }
            catch (Exception error)
            {
                output.Write("{{settext}} error " + error.Message);
            }
        }

        [HandlebarTag("settextblock")]
        public static void SetTextBlockHelper(TextWriter output, HelperOptions options, dynamic context, object[] arguments)
        {
            if (arguments.Length != 1) throw new HandlebarsException("{{settextblock lcddevice}} helper must have exactly one argument: (structure)");

            var block = arguments[0] as BlockData;
            var lcd   = block?.GetStructure()?.GetDevice<ILcd>(block.Position);

            try
            {
                using (var text = new StringWriter())
                {
                    options.Template(text, context as object);
                    lcd?.SetText(text.ToString());
                }
            }
            catch (Exception error)
            {
                output.Write("{{settextblock}} error " + error.Message);
            }
        }

        [HandlebarTag("gettext")]
        public static void GetTextHelper(TextWriter output, HelperOptions options, dynamic context, object[] arguments)
        {
            if (arguments.Length != 1) throw new HandlebarsException("{{gettext lcddevice}} helper must have exactly one argument: (structure)");

            var block = arguments[0] as BlockData;
            var lcd   = block?.GetStructure()?.GetDevice<ILcd>(block.Position);

            try
            {
                if (lcd == null) options.Inverse (output, context as object);
                else             options.Template(output, lcd.GetText());
            }
            catch (Exception error)
            {
                output.Write("{{settext}} error " + error.Message);
            }
        }

        [HandlebarTag("setfontsize")]
        public static void SetFontSizeHelper(TextWriter output, dynamic context, object[] arguments)
        {
            if (arguments.Length != 2) throw new HandlebarsException("{{setfontsize lcddevice size}} helper must have exactly two argument: (structure) (size)");

            var block = arguments[0] as BlockData;
            var lcd = block?.GetStructure()?.GetDevice<ILcd>(block.Position);
            int.TryParse(arguments[1]?.ToString(), out var size);

            try
            {
                lcd?.SetFontSize(size);
            }
            catch (Exception error)
            {
                output.Write("{{setfontsize}} error " + error.Message);
            }
        }

        [HandlebarTag("setcolor")]
        public static void SetColorHelper(TextWriter output, dynamic context, object[] arguments)
        {
            if (arguments.Length != 2) throw new HandlebarsException("{{setcolor lcddevice color}} helper must have exactly two argument: (structure) (rgb color)");

            var block = arguments[0] as BlockData;
            var lcd = block?.GetStructure()?.GetDevice<ILcd>(block.Position);

            try
            {
                int.TryParse(arguments[1]?.ToString(), NumberStyles.HexNumber, null, out int color);
                lcd?.SetColor(new Color((color & 0xff0000) >> 16, (color & 0x00ff00) >> 8, color & 0x0000ff));
            }
            catch (Exception error)
            {
                output.Write("{{setcolor}} error " + error.Message);
            }
        }

        [HandlebarTag("setbgcolor")]
        public static void SetBGColorHelper(TextWriter output, dynamic context, object[] arguments)
        {
            if (arguments.Length != 2) throw new HandlebarsException("{{setbgcolor lcddevice color}} helper must have exactly two argument: (structure) (rgb color)");

            var block = arguments[0] as BlockData;
            var lcd = block?.GetStructure()?.GetDevice<ILcd>(block.Position);

            try
            {
                int.TryParse(arguments[1]?.ToString(), NumberStyles.HexNumber, null, out int color);
                lcd?.SetBackground(new Color((color & 0xff0000) >> 16, (color & 0x00ff00) >> 8, color & 0x0000ff));
            }
            catch (Exception error)
            {
                output.Write("{{setbgcolor}} error " + error.Message);
            }
        }

    }
}