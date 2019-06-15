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
        public static void DeviceBlockHelper(TextWriter output, dynamic context, object[] arguments)
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
    }
}
