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
        [HandlebarTag("not")]
        public static void NotBlockHelper(TextWriter output, object root, dynamic context, object[] arguments)
        {
            if (arguments.Length != 1) throw new HandlebarsException("{{not}} helper must have exactly one argument: (testvalue)");

            try
            {
                var test = arguments[0]?.ToString();

                if      (string.IsNullOrEmpty(test))                output.Write(bool.TrueString);
                else if (bool.TryParse(test, out var boolresult))   output.Write(!boolresult);
                else if (int.TryParse(test,  out var intresult))    output.Write(intresult == 0);
            }
            catch (Exception error)
            {
                throw new HandlebarsException($"{{not}} {EmpyrionScripting.ErrorFilter(error)}");
            }
        }

        [HandlebarTag("ok")]
        public static void OkBlockHelper(TextWriter output, object root, HelperOptions options, dynamic context, object[] arguments)
            => OkIfBlockHelper("ok", output, root, options, context as object, arguments);

        [HandlebarTag("if")]
        public static void IfBlockHelper(TextWriter output, object root, HelperOptions options, dynamic context, object[] arguments)
            => OkIfBlockHelper("if", output, root, options, context as object, arguments);


        public static void OkIfBlockHelper(string name, TextWriter output, object root, HelperOptions options, dynamic context, object[] arguments)
        {
            if (arguments.Length != 1) throw new HandlebarsException("{{"+ name + "}} helper must have exactly one argument: (testvalue)");

            try
            {
                var test = arguments[0]?.ToString();

                if (string.IsNullOrEmpty(test)) options.Inverse(output, context as object);
                else if(bool.TryParse(test, out var boolresult))
                {
                    if(boolresult) options.Template(output, context as object);
                    else           options.Inverse(output,  context as object);
                }
                else if(int.TryParse(test, out var intresult))
                {
                    if(intresult == 0) options.Inverse(output, context as object); 
                    else               options.Template(output, context as object);
                }
                else options.Template(output, context as object);

            }
            catch (Exception error)
            {
                throw new HandlebarsException($"{{" + name + "}} " +EmpyrionScripting.ErrorFilter(error));
            }
        }

        [HandlebarTag("test")]
        public static void TestBlockHelper(TextWriter output, object root, HelperOptions options, dynamic context, object[] arguments) {
            if (arguments.Length != 3) throw new HandlebarsException("{{test}} helper must have exactly three argument: (testvalue) 'eq'|'le'|'leq'|'ge'|'geq'|'in' (compareto)");

            try
            {
                var left    = arguments[0];
                var op      = HandlebarsUtils.IsUndefinedBindingResult(arguments[1]) 
                                ? arguments[1].GetType().GetField("Value").GetValue(arguments[1])
                                : arguments[1]?.ToString();
                var right   = arguments[2];

                var renderTemplate = false;

                switch (op)
                {
                    case "eq" : renderTemplate = Compare(left, right) == 0; break;
                    case "neq": renderTemplate = Compare(left, right) != 0; break;
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
                throw new HandlebarsException($"{{test}} [{arguments?.Aggregate(string.Empty, (s, a) => s + $"{a} ")}]:{EmpyrionScripting.ErrorFilter(error)}");
            }
        }

        private static bool In(object left, object right)
        {
            if (left == null || right == null) return false;

            var list = right.ToString().Split(new []{ ',', ';', '#', '+' }, StringSplitOptions.RemoveEmptyEntries).Select(T => T.Trim());
            var Converter = TypeDescriptor.GetConverter(left.GetType());

            return list.Any(I =>
            {
                try
                {
                    var rangeDelimiter = I.IndexOf('-', 1);
                    return rangeDelimiter == -1
                        ? ((IComparable)left).CompareTo((IComparable)Converter.ConvertFromString(I)) == 0
                        : ((IComparable)left).CompareTo((IComparable)Converter.ConvertFromString(I.Substring(0, rangeDelimiter))) >= 0 &&
                          ((IComparable)left).CompareTo((IComparable)Converter.ConvertFromString(I.Substring(rangeDelimiter + 1))) <= 0;
                }
                catch
                {
                    return false;
                }
            });
        }

        private static int Compare(object left, object right)
        {
            if (left == null && right == null) return  0;
            if (left != null && right == null) return -1;
            if (left == null && right != null) return  1;

            var r = TypeDescriptor.GetConverter(left.GetType()).ConvertFromString(right.ToString());
            return ((IComparable)left).CompareTo((IComparable)r);
        }

    }
}
