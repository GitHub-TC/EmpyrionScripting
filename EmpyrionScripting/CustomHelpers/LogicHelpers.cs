using HandlebarsDotNet;
using System;
using System.ComponentModel;
using System.IO;
using System.Linq;

namespace EmpyrionScripting.CustomHelpers
{
    [HandlebarHelpers]
    public static class LogicHelpers
    {
        [HandlebarTag("test")]
        public static void TestBlockHelper(TextWriter output, HelperOptions options, dynamic context, object[] arguments) {
            if (arguments.Length != 3) throw new HandlebarsException("{{test}} helper must have exactly three argument: (testvalue) 'eq'|'le'|'leq'|'ge'|'geq'|'in' (compareto)");

            try
            {
                var left    = arguments[0];
                var op      = HandlebarsUtils.IsUndefinedBindingResult(arguments[1]) 
                                ? arguments[1].GetType().GetField("Value").GetValue(arguments[1])
                                : arguments[1] as string;
                var right   = arguments[2];

                var renderTemplate = false;

                switch (op)
                {
                    case "eq" : renderTemplate = Compare(left, right) == 0; break;
                    case "le" : renderTemplate = Compare(left, right) <  0; break;
                    case "leq": renderTemplate = Compare(left, right) <= 0; break;
                    case "ge" : renderTemplate = Compare(left, right) >  0; break;
                    case "geq": renderTemplate = Compare(left, right) >= 0; break;
                    case "in" : renderTemplate = In(left, right); break;
                }

                if (renderTemplate) options.Template(output, context as object);
                else                options.Inverse (output, context as object);
            }
            catch (Exception error)
            {
                throw new HandlebarsException($"{{test}} [{arguments?.Aggregate(string.Empty, (s, a) => s + $"{a}")}]:{error.Message}");
            }
        }

        private static bool In(object left, object right)
        {
            var list = right.ToString().Split(',').Select(T => T.Trim());
            var Converter = TypeDescriptor.GetConverter(left.GetType());

            return list.Any(I =>
            {
                var rangeDelimiter = I.IndexOf('-', 1);
                return rangeDelimiter == -1
                    ? ((IComparable)left).CompareTo((IComparable)Converter.ConvertFromString(I)) == 0
                    : ((IComparable)left).CompareTo((IComparable)Converter.ConvertFromString(I.Substring(0, rangeDelimiter))) >= 0 &&
                      ((IComparable)left).CompareTo((IComparable)Converter.ConvertFromString(I.Substring(rangeDelimiter + 1))) <= 0;
            });
        }

        private static int Compare(object left, object right)
        {
            var r = TypeDescriptor.GetConverter(left.GetType()).ConvertFromString(right.ToString());
            return ((IComparable)left).CompareTo((IComparable)r);
        }

    }
}
