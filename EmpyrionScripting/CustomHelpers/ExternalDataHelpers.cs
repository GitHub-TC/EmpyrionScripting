using EmpyrionScripting.DataWrapper;
using HandlebarsDotNet;
using System;
using System.IO;
using System.Linq;
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
                output.Write("{{datetime}} error " + EmpyrionScripting.ErrorFilter(error));
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
                output.Write("{{random}} error " + EmpyrionScripting.ErrorFilter(error));
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
                output.Write("{{use}} error " + EmpyrionScripting.ErrorFilter(error));
            }
        }

        [HandlebarTag("set")]
        public static void SetHelper(TextWriter output, dynamic context, object[] arguments)
        {
            if (arguments.Length != 3) throw new HandlebarsException("{{set @root key data}} helper must have three argument: @root (key) (data)");

            try
            {
                var root = arguments[0] as ScriptRootData;
                root.Data.AddOrUpdate(arguments[1]?.ToString(), arguments[2], (S, O) => arguments[2]);
            }
            catch (Exception error)
            {
                output.Write("{{set}} error " + EmpyrionScripting.ErrorFilter(error));
            }
        }

        [HandlebarTag("setblock")]
        public static void SetBlockHelper(TextWriter output, HelperOptions options, dynamic context, object[] arguments)
        {
            if (arguments.Length != 2) throw new HandlebarsException("{{setblock @root key}} helper must have three argument: @root (key)");

            try
            {
                var root = arguments[0] as ScriptRootData;

                var data = new StringWriter();
                options.Template(data, context as object);

                root.Data.AddOrUpdate(arguments[1]?.ToString(), data.ToString(), (S, O) => data.ToString());
            }
            catch (Exception error)
            {
                output.Write("{{setblock}} error " + EmpyrionScripting.ErrorFilter(error));
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
                output.Write("{{split}} error " + EmpyrionScripting.ErrorFilter(error));
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
                output.Write("{{concat}} error " + EmpyrionScripting.ErrorFilter(error));
            }
        }

        [HandlebarTag("substring")]
        public static void SubstringHelper(TextWriter output, dynamic context, object[] arguments)
        {
            if (arguments.Length < 2) throw new HandlebarsException("{{substring text startindex [length]}} helper must have at least two arguments: (text) (startindex)");

            var text = arguments[0]?.ToString();
            int startIndex = 0, 
                length     = 0;
            try
            {
                int.TryParse(arguments[1]?.ToString(), out startIndex);

                if(arguments.Length == 2) output.Write(text?.Substring(startIndex));
                else
                {
                    int.TryParse(arguments[2]?.ToString(), out length);
                    output.Write(text?.Substring(startIndex, Math.Min(length, text.Length - startIndex)));
                }
            }
            catch (Exception error)
            {
                if (arguments.Length == 2) output.Write("{{substring}} error (startindex=" + startIndex + ") text='" + text + "' " + EmpyrionScripting.ErrorFilter(error));
                else                       output.Write("{{substring}} error (startindex=" + startIndex + ", length=" + length + ") text='" + text + "' " + EmpyrionScripting.ErrorFilter(error));
            }
        }

        [HandlebarTag("chararray")]
        public static void CharArrayHelper(TextWriter output, HelperOptions options, dynamic context, object[] arguments)
        {
            if (arguments.Length != 1) throw new HandlebarsException("{{chararray text}} helper must have only one argument: (text)");

            try
            {
                options.Template(output, arguments[0]?.ToString().ToCharArray().Select(C => C.ToString()).ToArray());
            }
            catch (Exception error)
            {
                output.Write("{{chararray}} error " + EmpyrionScripting.ErrorFilter(error));
            }
        }

    }
}
