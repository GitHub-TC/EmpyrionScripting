using HandlebarsDotNet;
using System;
using System.IO;

namespace EmpyrionScripting.CustomHelpers
{
    public class SaveGameScriptHelpers
    {
        [HandlebarTag("readfile")]
        public static void ReadFileHelper(TextWriter output, HelperOptions options, dynamic context, object[] arguments)
        {
            if (arguments.Length != 3) throw new HandlebarsException("{{readfile @root dir filename}} helper must have two argument: @root (dir) (filename)");

            if (!(arguments[0] is ScriptSaveGameRootData root)) throw new HandlebarsException("{{readfile @root dir filename}} only allowed in SaveGame scripts");

            try
            {
                var filename = Path.Combine(arguments[1].ToString(), arguments[2].ToString());

                if (File.Exists(filename)) options.Template(output, File.ReadAllLines(filename));
                else                       options.Inverse(output, context as object);
            }
            catch (Exception error)
            {
                output.Write("{{readfile}} error " + error.Message);
            }
        }

        [HandlebarTag("writefile")]
        public static void WriteFileHelper(TextWriter output, HelperOptions options, dynamic context, object[] arguments)
        {
            if (arguments.Length < 3) throw new HandlebarsException("{{writefile @root dir filename [append]}} helper must have at least three argument: @root (dir) (filename)");

            if (!(arguments[0] is ScriptSaveGameRootData root)) throw new HandlebarsException("{{writefile @root dir filename}} only allowed in SaveGame scripts");

            try
            {
                var filename = Path.Combine(arguments[1].ToString(), arguments[2].ToString());
                bool.TryParse(arguments.Length > 3 ? arguments[3].ToString() : null, out var append);

                using (var file = append ? File.AppendText(filename) : File.CreateText(filename))
                {
                    options.Template(file, context as object);
                }
            }
            catch (Exception error)
            {
                output.Write("{{writefile}} error " + error.Message);
            }
        }
    }
}
