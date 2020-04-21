using EmpyrionScripting.Internal.Interface;
using HandlebarsDotNet;
using System;
using System.Collections;
using System.IO;
using System.Linq;

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

        [HandlebarTag("sortedeach")]
        public static void SortedEachHelper(TextWriter output, object rootObject, HelperOptions options, dynamic context, object[] arguments)
        {
            if (arguments.Length != 2 && arguments.Length != 3) throw new HandlebarsException("{{sortedeach array sortedBy [reverse]}} helper must have two argument: (array) (sortedBy) (true|false)");

            var root = rootObject as IScriptRootData;

            try
            {
                var array       = ((IEnumerable)arguments[0]).OfType<object>();
                var sortedBy    = arguments[1]?.ToString();
                if (!bool.TryParse(arguments.Get(2)?.ToString(), out var reverse)) reverse = false;

                if (array.Count() == 0) {
                    options.Inverse(output, (object)context);
                    return;
                }

                var getProperty = string.IsNullOrEmpty(sortedBy) ? null : array.First().GetType().GetProperty(sortedBy);

                var sortedArray = reverse
                    ? array.OrderByDescending(V => getProperty == null ? V : getProperty.GetValue(V))
                    : array.OrderBy(V => getProperty == null ? V : getProperty.GetValue(V));

                sortedArray.ForEach(V => options.Template(output, V));
            }
            catch (Exception error)
            {
                output.Write("{{sortedeach}} error " + EmpyrionScripting.ErrorFilter(error));
            }
        }

        [HandlebarTag("sort")]
        public static void SortHelper(TextWriter output, object rootObject, HelperOptions options, dynamic context, object[] arguments)
        {
            if (arguments.Length != 2 && arguments.Length != 3) throw new HandlebarsException("{{sort array sortedBy [reverse]}} helper must have two argument: (array) (sortedBy) (true|false)");

            var root = rootObject as IScriptRootData;

            try
            {
                var array = ((IEnumerable)arguments[0]).OfType<object>();
                var sortedBy = arguments[1]?.ToString();
                if (!bool.TryParse(arguments.Get(2)?.ToString(), out var reverse)) reverse = false;

                if (array.Count() == 0)
                {
                    options.Inverse(output, (object)context);
                    return;
                }

                var getProperty = string.IsNullOrEmpty(sortedBy) ? null : array.First().GetType().GetProperty(sortedBy);

                var sortedArray = reverse
                    ? array.OrderByDescending(V => getProperty == null ? V : getProperty.GetValue(V))
                    : array.OrderBy(V => getProperty == null ? V : getProperty.GetValue(V));

                options.Template(output, sortedArray);
            }
            catch (Exception error)
            {
                output.Write("{{sort}} error " + EmpyrionScripting.ErrorFilter(error));
            }
        }

    }
}
