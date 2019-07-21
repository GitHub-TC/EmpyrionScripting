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
        public static void MathBlockHelper(TextWriter output, HelperOptions options, dynamic context, object[] arguments)
        {
            if (arguments.Length != 3) throw new HandlebarsException("{{math}} helper must have exactly three argument: (lvalue) op (rvalue)");

            try
            {
                double.TryParse(arguments[0]?.ToString(), out var left);
                var op = HandlebarsUtils.IsUndefinedBindingResult(arguments[1])
                                ? arguments[1].GetType().GetField("Value").GetValue(arguments[1])
                                : arguments[1]?.ToString();
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
            catch (Exception error)
            {
                throw new HandlebarsException($"{{math}} [{arguments?.Aggregate(string.Empty, (s, a) => s + $"{a}")}]:{EmpyrionScripting.ErrorFilter(error)}");
            }
        }

        [HandlebarTag("calc")]
        public static void CalcHelper(TextWriter output, dynamic context, object[] arguments)
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
        public static void DistanceHelper(TextWriter output, dynamic context, object[] arguments)
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
