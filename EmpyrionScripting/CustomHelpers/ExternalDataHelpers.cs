using HandlebarsDotNet;
using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace EmpyrionScripting.CustomHelpers
{
    [HandlebarHelpers]
    public static class ExternalDataHelpers
    {
        [HandlebarTag("datetime")]
        public static void DateTimeHelper(TextWriter output, dynamic context, object[] arguments)
        {
            var format = arguments.Length > 0 ? arguments[0]?.ToString() : null;
            var add    = arguments.Length > 1 ? arguments[1]?.ToString() : null;

            try
            {
                var current = string.IsNullOrEmpty(add)
                        ? DateTime.Now
                        : DateTime.UtcNow.AddHours(int.Parse(add));

                if (string.IsNullOrEmpty(format)) output.Write(current);
                else                              output.Write(current.ToString(format));
            }
            catch (Exception error)
            {
                output.Write("{{datetime}} error " + error.Message);
            }
        }

        [HandlebarTag("random")]
        public static void RandomHelper(TextWriter output, HelperOptions options, dynamic context, object[] arguments)
        {
            if (arguments.Length != 2) throw new HandlebarsException("{{random start end}} helper must have two argument: (start) (end)");

            var format = arguments.Length > 0 ? arguments[0]?.ToString() : null;
            var add    = arguments.Length > 1 ? arguments[1]?.ToString() : null;

            try
            {
                int.TryParse(arguments[0]?.ToString(), out var start);
                int.TryParse(arguments[1]?.ToString(), out var end);

                options.Template(output, new Random().Next(start, end));
            }
            catch (Exception error)
            {
                output.Write("{{random}} error " + error.Message);
            }
        }

        [HandlebarTag("use")]
        public static void UseHelper(TextWriter output, HelperOptions options, dynamic context, object[] arguments)
        {
            if (arguments.Length == 0) throw new HandlebarsException("{{use data}} helper must have one argument: (data)");

            try
            {
                if (arguments[0] == null) options.Inverse (output, context as object);
                else                      options.Template(output, arguments[0]);
            }
            catch (Exception error)
            {
                output.Write("{{use}} error " + error.Message);
            }
        }

        [HandlebarTag("split")]
        public static void SplitHelper(TextWriter output, HelperOptions options, dynamic context, object[] arguments)
        {
            if (arguments.Length < 2) throw new HandlebarsException("{{split string separator [removeemptyentries]}} helper must have at least two argument: (string) (separator) [true|false]");

            var data      =                arguments[0].ToString();
            var separator = Regex.Unescape(arguments[1].ToString());

            try
            {
                bool.TryParse(arguments.Length > 2 ? arguments[2]?.ToString() : null, out var removeemptyentries);

                options.Template(output, data.Split(new[] { separator },
                    removeemptyentries ? StringSplitOptions.RemoveEmptyEntries : StringSplitOptions.None));
            }
            catch (Exception error)
            {
                output.Write("{{split}} error " + error.Message);
            }
        }

        [HandlebarTag("concat")]
        public static void ConcatHelper(TextWriter output, dynamic context, object[] arguments)
        {
            if (arguments.Length < 2) throw new HandlebarsException("{{concat a1 a2 a3 ...}} helper must have at least two arguments: (a1) (a2)");

            try
            {
                var text = new StringBuilder();
                for (int i = 0; i < arguments.Length; i++)
                {
                    if      (arguments[i] is string   s) text.Append(s);
                    else if (arguments[i] is string[] a) text.Append(string.Join(Regex.Unescape(arguments[++i]?.ToString()), a));
                    else                                 text.Append(arguments[i]?.ToString());
                }

                output.Write(text.ToString());
            }
            catch (Exception error)
            {
                output.Write("{{concat}} error " + error.Message);
            }
        }

    }
}
