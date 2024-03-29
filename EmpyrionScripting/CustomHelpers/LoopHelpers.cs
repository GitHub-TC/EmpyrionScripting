﻿using EmpyrionScripting.CsHelper;
using EmpyrionScripting.Interface;
using EmpyrionScripting.Internal.Interface;
using HandlebarsDotNet;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
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
            if(string.IsNullOrEmpty(orderedByFields))   return array.OrderBy          (x => x);
            if(orderedByFields == "+")                  return array.OrderBy          (x => x);
            if(orderedByFields == "-")                  return array.OrderByDescending(x => x);

            var isDictionary = array.FirstOrDefault() is IDictionary;

            var orderBy = orderedByFields
                .Split(new[] { ';', ',' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(O => O.Trim())
                .Select(O => new { Ascending = O.StartsWith("+"), Name = O.Substring(1), Property = isDictionary ? null : array.First().GetType().GetProperty(O.Substring(1)) })
                .Where(P => isDictionary || P.Property != null)
                .ToArray();

            return orderBy.Length == 0 || !array.Any()
                ? null
                : orderBy.Skip(1).Aggregate(
                    orderBy[0].Ascending
                        ? array.OrderBy          (V => isDictionary ? GetNativeValue(((IDictionary<string, object>)V)[orderBy[0].Name]) : orderBy[0].Property.GetValue(V))
                        : array.OrderByDescending(V => isDictionary ? GetNativeValue(((IDictionary<string, object>)V)[orderBy[0].Name]) : orderBy[0].Property.GetValue(V)),
                    (L, O) => O.Ascending
                        ? L.ThenBy             (V => isDictionary ? GetNativeValue(((IDictionary<string, object>)V)[O.Name]) : O.Property.GetValue(V))
                        : L.ThenByDescending   (V => isDictionary ? GetNativeValue(((IDictionary<string, object>)V)[O.Name]) : O.Property.GetValue(V)));
        }

        public static object GetNativeValue(object v)
        {
            if (!(v is string vs)) return v;

            if (bool  .TryParse(vs, out var b                                                  )) return b;
            if (int   .TryParse(vs, NumberStyles.Integer, null, out var i                      )) return i;
            if (double.TryParse(vs, NumberStyles.Float, CultureInfo.CurrentCulture, out var fc )) return fc;
            if (double.TryParse(vs, NumberStyles.Float, CultureInfo.InvariantCulture, out var f)) return f;

            return v;
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
            if (arguments.Length < 1 || arguments.Length > 3) throw new HandlebarsException("{{sortedeach array [sortedBy]}} helper must have at least one argument: (array) [+/-sortedBy]");

            var root = rootObject as IScriptRootData;

            try
            {
                var array   = ((IEnumerable)arguments[0]).OfType<object>();
                var orderBy = arguments.Get(1)?.ToString() ?? string.Empty;
                if (!bool.TryParse(arguments.Get(2)?.ToString(), out var reverse)) reverse = false;

                if (!orderBy.StartsWith("+") && !orderBy.StartsWith("-")) orderBy = reverse ? $"-{orderBy}" : $"+{orderBy}";

                var orderedArray = array.Count() == 0 ? null : OrderedList(array, orderBy);

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
            if (arguments.Length < 1 || arguments.Length > 3) throw new HandlebarsException("{{sort array sortedBy}} helper must have at least one argument: (array) [+/-sortedBy]");

            var root = rootObject as IScriptRootData;

            try
            {
                var array   = ((IEnumerable)arguments[0]).OfType<object>();
                var orderBy = arguments.Get(1)?.ToString() ?? string.Empty;
                if (!bool.TryParse(arguments.Get(2)?.ToString(), out var reverse)) reverse = false;

                if(!orderBy.StartsWith("+") && !orderBy.StartsWith("-")) orderBy = reverse ? $"-{orderBy}" : $"+{orderBy}";

                var orderedArray = array.Count() == 0
                    ? null 
                    : OrderedList(array, orderBy);

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
            if (arguments.Length < 1) throw new HandlebarsException("{{orderedeach array ['+/-sortedBy1,+/-sortedBy2,...']}} helper must have at least one argument: (array) [+/-sortedBy]");

            var root = rootObject as IScriptRootData;

            try
            {
                var array   = ((IEnumerable)arguments[0]).OfType<object>();
                var orderBy = arguments.Get(1)?.ToString() ?? string.Empty;
                if (!bool.TryParse(arguments.Get(2)?.ToString(), out var reverse)) reverse = false;

                var orderedArray = array.Count() == 0
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
            if (arguments.Length < 1) throw new HandlebarsException("{{order array ['+/-sortedBy1,+/-sortedBy2,...']}} helper must have at least one argument: (array) [+/-sortedBy]");

            var root = rootObject as IScriptRootData;

            try
            {
                var array   = ((IEnumerable)arguments[0]).OfType<object>();
                var orderBy = arguments.Get(1)?.ToString() ?? string.Empty;
                if (!bool.TryParse(arguments.Get(2)?.ToString(), out var reverse)) reverse = false;

                var orderedArray = array.Count() == 0
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
