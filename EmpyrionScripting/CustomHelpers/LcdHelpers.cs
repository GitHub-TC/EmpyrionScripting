using Eleon.Modding;
using EmpyrionScripting.DataWrapper;
using HandlebarsDotNet;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
    }
}
