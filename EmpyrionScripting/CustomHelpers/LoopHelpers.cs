using EmpyrionScripting.CsHelper;
using EmpyrionScripting.Interface;
using EmpyrionScripting.Internal.Interface;
using HandlebarsDotNet;
using System;
using System.Collections;
using System.Collections.Generic;
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
                if (!CsScriptFunctions.FunctionNeedsMainThread(error, root)) output.Write("{{steps}} error " + EmpyrionScripting.ErrorFilter(error));
            }
        }

        public static IOrderedEnumerable<object> OrderedList(IEnumerable<object> array, string orderedByFields)
        {
            var orderBy = orderedByFields
                .Split(new[] { ';', ',' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(O => O.Trim())
                .Select(O => new { Ascending = O.StartsWith("+"), Property = array.First().GetType().GetProperty(O.Substring(1)) })
                .Where(P => P.Property != null)
                .ToArray();

            return orderBy.Length == 0 || !array.Any()
                ? null
                : orderBy.Skip(1).Aggregate(
                    orderBy[0].Ascending
                        ? array.OrderBy          (V => orderBy[0].Property.GetValue(V))
                        : array.OrderByDescending(V => orderBy[0].Property.GetValue(V)),
                    (L, O) => O.Ascending
                        ? L.ThenBy             (V => O.Property.GetValue(V))
                        : L.ThenByDescending   (V => O.Property.GetValue(V)));
        }

        [HandlebarTag("loop")]
        public static void LoopUpHelper(TextWriter output, object rootObject, HelperOptions options, dynamic context, object[] arguments)
        {
            if (arguments.Length != 1) throw new HandlebarsException("{{loop array/dictionary}} helper must have one argument: (array/dictionary)");

            var root = rootObject as IScriptRootData;
            try
            {
                if      (arguments[0] is object[]   arraydata) arraydata.ForEach(value => options.Template(output, value));
                else if (arguments[0] is IList       listdata) for (int i = 0; i < listdata.Count; i++) options.Template(output, listdata[i]);
                else if (arguments[0] is IDictionary dictdata) dictdata.Keys.Cast<object>().ForEach(key => options.Template(output, new KeyValuePair<object, object>(key, dictdata[key])));
                else if (arguments[0] is IEnumerable enumdata) enumdata.Cast<object>().ForEach(value => options.Template(output, value));
                else                                           options.Inverse(output, (object)context);
            }
            catch (Exception error)
            {
                if (!CsScriptFunctions.FunctionNeedsMainThread(error, root)) output.Write("{{loop}} error " + EmpyrionScripting.ErrorFilter(error));
            }
        }

        [HandlebarTag("sortedeach")]
        public static void SortedEachHelper(TextWriter output, object rootObject, HelperOptions options, dynamic context, object[] arguments)
        {
            if (arguments.Length != 2 && arguments.Length != 3) throw new HandlebarsException("{{sortedeach array sortedBy [reverse]}} helper must have two argument: (array) (sortedBy) (true|false)");

            var root = rootObject as IScriptRootData;

            try
            {
                var array   = ((IEnumerable)arguments[0]).OfType<object>();
                var orderBy = arguments[1]?.ToString();
                if (!bool.TryParse(arguments.Get(2)?.ToString(), out var reverse)) reverse = false;

                var orderedArray = array.Count() == 0 || string.IsNullOrEmpty(orderBy) 
                    ? reverse ? array.OrderByDescending(d => d) : array.OrderBy(d => d)
                    : OrderedList(array, (reverse ? '-' : '+') + orderBy);

                if (orderedArray == null) options.Inverse(output, (object)context);
                else                      orderedArray.ForEach(V => options.Template(output, V), () => root.TimeLimitReached);
            }
            catch (Exception error)
            {
                if (!CsScriptFunctions.FunctionNeedsMainThread(error, root)) output.Write("{{sortedeach}} error " + EmpyrionScripting.ErrorFilter(error));
            }
        }

        [HandlebarTag("sort")]
        public static void SortHelper(TextWriter output, object rootObject, HelperOptions options, dynamic context, object[] arguments)
        {
            if (arguments.Length != 2 && arguments.Length != 3) throw new HandlebarsException("{{sort array sortedBy [reverse]}} helper must have two argument: (array) (sortedBy) (true|false)");

            var root = rootObject as IScriptRootData;

            try
            {
                var array   = ((IEnumerable)arguments[0]).OfType<object>();
                var orderBy = arguments[1]?.ToString();
                if (!bool.TryParse(arguments.Get(2)?.ToString(), out var reverse)) reverse = false;

                var orderedArray = array.Count() == 0 || string.IsNullOrEmpty(orderBy) 
                    ? null 
                    : OrderedList(array, (reverse ? '-' : '+') + orderBy);

                if (orderedArray == null) options.Inverse (output, (object)context);
                else                      options.Template(output, orderedArray.ToArray());
            }
            catch (Exception error)
            {
                if (!CsScriptFunctions.FunctionNeedsMainThread(error, root)) output.Write("{{sort}} error " + EmpyrionScripting.ErrorFilter(error));
            }
        }

        [HandlebarTag("orderedeach")]
        public static void OrderedEachHelper(TextWriter output, object rootObject, HelperOptions options, dynamic context, object[] arguments)
        {
            if (arguments.Length != 2) throw new HandlebarsException("{{orderedeach array '+/-sortedBy1,+/-sortedBy2,...'}} helper must have two argument: (array) (+/-sortedBy)");

            var root = rootObject as IScriptRootData;

            try
            {
                var array   = ((IEnumerable)arguments[0]).OfType<object>();
                var orderBy = arguments[1]?.ToString();
                if (!bool.TryParse(arguments.Get(2)?.ToString(), out var reverse)) reverse = false;

                var orderedArray = array.Count() == 0 || string.IsNullOrEmpty(orderBy) 
                    ? null 
                    : OrderedList(array, orderBy);

                if (orderedArray == null) options.Inverse(output, (object)context);
                else                      orderedArray.ForEach(V => options.Template(output, V), () => root.TimeLimitReached);
            }
            catch (Exception error)
            {
                if (!CsScriptFunctions.FunctionNeedsMainThread(error, root)) output.Write("{{orderedeach}} error " + EmpyrionScripting.ErrorFilter(error));
            }
        }

        [HandlebarTag("order")]
        public static void OrderHelper(TextWriter output, object rootObject, HelperOptions options, dynamic context, object[] arguments)
        {
            if (arguments.Length != 2) throw new HandlebarsException("{{order array '+/-sortedBy1,+/-sortedBy2,...'}} helper must have two argument: (array) (+/-sortedBy)");

            var root = rootObject as IScriptRootData;

            try
            {
                var array   = ((IEnumerable)arguments[0]).OfType<object>();
                var orderBy = arguments[1]?.ToString();
                if (!bool.TryParse(arguments.Get(2)?.ToString(), out var reverse)) reverse = false;

                var orderedArray = array.Count() == 0 || string.IsNullOrEmpty(orderBy) 
                    ? null 
                    : OrderedList(array, orderBy);

                if (orderedArray == null) options.Inverse (output, (object)context);
                else                      options.Template(output, orderedArray.ToArray());
            }
            catch (Exception error)
            {
                if (!CsScriptFunctions.FunctionNeedsMainThread(error, root)) output.Write("{{order}} error " + EmpyrionScripting.ErrorFilter(error));
            }
        }

    }
}
