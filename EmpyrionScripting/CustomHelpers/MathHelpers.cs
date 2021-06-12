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
            if (arguments.Length != 3 && arguments.Length != 4) throw new HandlebarsException("{{math}} helper must have exactly at least three arguments: (lvalue) op (rvalue) [digits]");

            try
            {
                var op = HandlebarsUtils.IsUndefinedBindingResult(arguments[1])
                                ? arguments[1].GetType().GetField("Value").GetValue(arguments[1])
                                : arguments[1]?.ToString();

                if (     arguments[0] is DateTime ||
                         arguments[0] is TimeSpan)  CalcWithTime  (op, output, options, arguments);
                else if (arguments[0] is Vector3)   CalcWithVector(op, output, options, arguments);
                else                                CalcWithDouble(op, output, options, arguments);
            }
            catch (Exception error)
            {
                throw new HandlebarsException($"{{math}} [{arguments?.Aggregate(string.Empty, (s, a) => s + $"{a}")}]:{EmpyrionScripting.ErrorFilter(error)}");
            }
        }

        private static void CalcWithVector(object op, TextWriter output, HelperOptions options, object[] arguments)
        {
            float.TryParse(arguments[2]?.ToString(), out var scalar);

            switch (op)
            {
                case "+": options.Template(output, (Vector3)arguments[0] + (Vector3)arguments[2]); break;
                case "-": options.Template(output, (Vector3)arguments[0] - (Vector3)arguments[2]); break;
                case "*": options.Template(output, (Vector3)arguments[0] * scalar); break;
                case "/": options.Template(output, (Vector3)arguments[0] / scalar); break;
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
            var withRound = int.TryParse(arguments.Get(3)?.ToString(), out var digits);

            switch (op)
            {
                case "+": options.Template(output, withRound ? Math.Round(left + right, digits) : left + right); break;
                case "-": options.Template(output, withRound ? Math.Round(left - right, digits) : left - right); break;
                case "*": options.Template(output, withRound ? Math.Round(left * right, digits) : left * right); break;
                case "/": options.Template(output, withRound ? Math.Round(left / right, digits) : left / right); break;
                case "%": options.Template(output, withRound ? Math.Round(left % right, digits) : left % right); break;
            }
        }

        [HandlebarTag("calc")]
        public static void CalcHelper(TextWriter output, object root, dynamic context, object[] arguments)
        {
            if (arguments.Length != 3 && arguments.Length != 4) throw new HandlebarsException("{{calc}} helper must have at least three arguments: (lvalue) op (rvalue) [digits]");

            try
            {
                double.TryParse(arguments[0]?.ToString(), out var left);
                var op = HandlebarsUtils.IsUndefinedBindingResult(arguments[1])
                                ? arguments[1].GetType().GetField("Value").GetValue(arguments[1])
                                : arguments[1]?.ToString();
                double.TryParse(arguments[2]?.ToString(), out var right);
                var withRound = int.TryParse(arguments.Get(3)?.ToString(), out var digits);

                switch (op)
                {
                    case "+": output.Write(withRound ? Math.Round(left + right, digits) : left + right); break;
                    case "-": output.Write(withRound ? Math.Round(left - right, digits) : left - right); break;
                    case "*": output.Write(withRound ? Math.Round(left * right, digits) : left * right); break;
                    case "/": output.Write(withRound ? Math.Round(left / right, digits) : left / right); break;
                    case "%": output.Write(withRound ? Math.Round(left % right, digits) : left % right); break;
                }
            }
            catch (Exception error)
            {
                throw new HandlebarsException($"{{calc}} [{arguments?.Aggregate(string.Empty, (s, a) => s + $"{a}")}]:{EmpyrionScripting.ErrorFilter(error)}");
            }
        }

        [HandlebarTag("min")]
        public static void MinHelper(TextWriter output, object root, dynamic context, object[] arguments)
        {
            if (arguments.Length != 2) throw new HandlebarsException("{{min}} helper must have two arguments: (lvalue) (rvalue)");

            try
            {
                double.TryParse(arguments[0]?.ToString(), out var left);
                double.TryParse(arguments[1]?.ToString(), out var right);

                output.Write(Math.Min(left, right));
            }
            catch (Exception error)
            {
                throw new HandlebarsException($"{{min}} [l:{arguments[0]} r:{arguments[1]} error: {EmpyrionScripting.ErrorFilter(error)}");
            }
        }

        [HandlebarTag("max")]
        public static void MaxHelper(TextWriter output, object root, dynamic context, object[] arguments)
        {
            if (arguments.Length != 2) throw new HandlebarsException("{{max}} helper must have two arguments: (lvalue) (rvalue)");

            try
            {
                double.TryParse(arguments[0]?.ToString(), out var left);
                double.TryParse(arguments[1]?.ToString(), out var right);

                output.Write(Math.Max(left, right));
            }
            catch (Exception error)
            {
                throw new HandlebarsException($"{{max}} [l:{arguments[0]} r:{arguments[1]} error: {EmpyrionScripting.ErrorFilter(error)}");
            }
        }

        [HandlebarTag("int")]
        public static void IntHelper(TextWriter output, object root, dynamic context, object[] arguments)
        {
            if (arguments.Length != 1) throw new HandlebarsException("{{int}} helper must have one argument: (value)");

            try
            {
                double.TryParse(arguments[0]?.ToString(), out var left);
                output.Write((int)left);
            }
            catch (Exception error)
            {
                throw new HandlebarsException($"{{int}} {arguments[0]} error: {EmpyrionScripting.ErrorFilter(error)}");
            }
        }

        [HandlebarTag("abs")]
        public static void AbsHelper(TextWriter output, object root, dynamic context, object[] arguments)
        {
            if (arguments.Length != 1) throw new HandlebarsException("{{abs}} helper must have one argument: (value)");

            try
            {
                double.TryParse(arguments[0]?.ToString(), out var left);
                output.Write(Math.Abs(left));
            }
            catch (Exception error)
            {
                throw new HandlebarsException($"{{abs}} {arguments[0]} error: {EmpyrionScripting.ErrorFilter(error)}");
            }
        }

        [HandlebarTag("distance")]
        public static void DistanceHelper(TextWriter output, object root, dynamic context, object[] arguments)
        {
            if (arguments.Length != 2 && arguments.Length != 3) throw new HandlebarsException("{{distance}} helper must have at least two argument: (lVector) (rVector) [format]");

            try
            {
                var format = arguments.Get(2)?.ToString();

                output.Write(string.IsNullOrEmpty(format) 
                    ? Vector3.Distance((Vector3)arguments[0], (Vector3)arguments[1]).ToString()
                    : string.Format(format, Vector3.Distance((Vector3)arguments[0], (Vector3)arguments[1]))
                    ); 
            }
            catch (Exception error)
            {
                throw new HandlebarsException($"{{distance}} error: {EmpyrionScripting.ErrorFilter(error)}");
            }
        }

    }
}
