using HandlebarsDotNet;
using System;
using System.IO;
using System.Linq;
using UnityEngine;

namespace EmpyrionScripting.CustomHelpers
{
    [HandlebarHelpers]
    public static class MathHelpers
    {
        [HandlebarTag("math")]
        public static void MathBlockHelper(TextWriter output, object root, HelperOptions options, dynamic context, object[] arguments)
        {
            if (arguments.Length != 3) throw new HandlebarsException("{{math}} helper must have exactly three argument: (lvalue) op (rvalue)");

            try
            {
                var op = HandlebarsUtils.IsUndefinedBindingResult(arguments[1])
                                ? arguments[1].GetType().GetField("Value").GetValue(arguments[1])
                                : arguments[1]?.ToString();

                if (arguments[0] is DateTime ||
                    arguments[0] is TimeSpan) CalcWithTime  (op, output, options, arguments);
                else                          CalcWithDouble(op, output, options, arguments);
            }
            catch (Exception error)
            {
                throw new HandlebarsException($"{{math}} [{arguments?.Aggregate(string.Empty, (s, a) => s + $"{a}")}]:{EmpyrionScripting.ErrorFilter(error)}");
            }
        }

        private static void CalcWithTime(object op, TextWriter output, HelperOptions options, object[] arguments)
        {
            switch (op)
            {
                case "+": if (arguments[0] is TimeSpan) options.Template(output, (TimeSpan)arguments[0] + (TimeSpan)arguments[2]);
                          else                          options.Template(output, (DateTime)arguments[0] + (TimeSpan)arguments[2]);
                          break;

                case "-": if     (arguments[2] is DateTime) options.Template(output, (DateTime)arguments[0] - (DateTime)arguments[2]);
                          else if(arguments[0] is TimeSpan) options.Template(output, (TimeSpan)arguments[0] - (TimeSpan)arguments[2]);
                          else                              options.Template(output, (DateTime)arguments[0] - (TimeSpan)arguments[2]);
                          break;
            }
        }

        private static void CalcWithDouble(object op, TextWriter output, HelperOptions options, object[] arguments)
        {
            double.TryParse(arguments[0]?.ToString(), out var left);
            double.TryParse(arguments[2]?.ToString(), out var right);

            switch (op)
            {
                case "+": options.Template(output, left + right); break;
                case "-": options.Template(output, left - right); break;
                case "*": options.Template(output, left * right); break;
                case "/": options.Template(output, left / right); break;
                case "%": options.Template(output, left % right); break;
            }
        }

        [HandlebarTag("calc")]
        public static void CalcHelper(TextWriter output, object root, dynamic context, object[] arguments)
        {
            if (arguments.Length != 3) throw new HandlebarsException("{{calc}} helper must have exactly three argument: (lvalue) op (rvalue)");

            try
            {
                double.TryParse(arguments[0]?.ToString(), out var left);
                var op = HandlebarsUtils.IsUndefinedBindingResult(arguments[1])
                                ? arguments[1].GetType().GetField("Value").GetValue(arguments[1])
                                : arguments[1]?.ToString();
                double.TryParse(arguments[2]?.ToString(), out var right);

                switch (op)
                {
                    case "+": output.Write(left + right); break;
                    case "-": output.Write(left - right); break;
                    case "*": output.Write(left * right); break;
                    case "/": output.Write(left / right); break;
                    case "%": output.Write(left % right); break;
                }
            }
            catch (Exception error)
            {
                throw new HandlebarsException($"{{calc}} [{arguments?.Aggregate(string.Empty, (s, a) => s + $"{a}")}]:{EmpyrionScripting.ErrorFilter(error)}");
            }
        }

        [HandlebarTag("distance")]
        public static void DistanceHelper(TextWriter output, object root, dynamic context, object[] arguments)
        {
            if (arguments.Length != 2) throw new HandlebarsException("{{distance}} helper must have exactly two argument: (lVector) (rVector)");

            try
            {
                output.Write(Vector3.Distance((Vector3)arguments[0], (Vector3)arguments[1])); 
            }
            catch (Exception error)
            {
                throw new HandlebarsException($"{{distance}} error: {EmpyrionScripting.ErrorFilter(error)}");
            }
        }

    }
}
