using EmpyrionScripting.DataWrapper;
using HandlebarsDotNet;
using System;
using System.IO;

namespace EmpyrionScripting.CustomHelpers
{
    [HandlebarHelpers]
    public static class LoopHelpers
    {
        [HandlebarTag("steps")]
        public static void StepsHelper(TextWriter output, object rootObject, HelperOptions options, dynamic context, object[] arguments)
        {
            if (arguments.Length < 2) throw new HandlebarsException("{{steps start end [step] [delay]}} helper must have at least two argument: (start) (end)");

            var root = rootObject as IScriptRootData;
            int.TryParse(arguments[0]?.ToString(), out int start);
            int.TryParse(arguments[1]?.ToString(), out int end);

            int step  = arguments.Length > 2 && int.TryParse(arguments[2]?.ToString(), out int s) ? s : 1;
            int delay = arguments.Length > 3 && int.TryParse(arguments[3]?.ToString(), out int d) ? d : 1;

            try
            {
                var i = root.CycleCounter % (Math.Abs(delay) * ((Math.Abs(end - start) / Math.Abs(step)) + 1));
                var stepI = (int)(start < end ? i * Math.Abs(step) : start - (i * Math.Abs(step)));
                options.Template(output, (1 + stepI) / Math.Abs(delay));

            }
            catch (Exception error)
            {
                output.Write("{{steps}} error " + EmpyrionScripting.ErrorFilter(error));
            }
        }

    }
}
