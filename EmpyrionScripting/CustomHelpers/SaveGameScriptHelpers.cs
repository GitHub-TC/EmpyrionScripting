using EmpyrionScripting.DataWrapper;
using HandlebarsDotNet;
using System;
using System.IO;

namespace EmpyrionScripting.CustomHelpers
{
    [HandlebarHelpers]
    public class SaveGameScriptHelpers
    {
        [HandlebarTag("fileexists")]
        public static void FileExistsHelper(TextWriter output, HelperOptions options, dynamic context, object[] arguments)
        {
            if (arguments.Length != 3) throw new HandlebarsException("{{fileexists @root dir filename}} helper must have two argument: @root (dir) (filename)");

            if (!(arguments[0] is ScriptSaveGameRootData root)) throw new HandlebarsException("{{readfile @root dir filename}} only allowed in SaveGame scripts");

            try
            {
                if (File.Exists(Path.Combine(arguments[1].ToString(), arguments[2].ToString()))) options.Template(output, context as object);
                else                                                                             options.Inverse (output, context as object);
            }
            catch (Exception error)
            {
                output.Write("{{fileexists}} error " + EmpyrionScripting.ErrorFilter(error));
            }
        }

        [HandlebarTag("filelist")]
        public static void FileListHelper(TextWriter output, HelperOptions options, dynamic context, object[] arguments)
        {
            if (arguments.Length != 3 && arguments.Length != 4) throw new HandlebarsException("{{filelist @root dir filename [recursive]}} helper must have at least three arguments: @root (dir) (filename) [recursive]");

            if (!(arguments[0] is ScriptSaveGameRootData root)) throw new HandlebarsException("{{filelist}} only allowed in SaveGame scripts");

            try
            {
                bool.TryParse(arguments.Length == 4 ? arguments[3].ToString() : "false", out var recursive);
                options.Template(output, Directory.EnumerateFiles(arguments[1].ToString(), arguments[2].ToString(), recursive ? SearchOption.TopDirectoryOnly : SearchOption.AllDirectories));
            }
            catch (Exception error)
            {
                output.Write("{{filelist}} error " + EmpyrionScripting.ErrorFilter(error));
            }
        }

        [HandlebarTag("readfile")]
        public static void ReadFileHelper(TextWriter output, HelperOptions options, dynamic context, object[] arguments)
        {
            if (arguments.Length != 3) throw new HandlebarsException("{{readfile @root dir filename}} helper must have two argument: @root (dir) (filename)");

            if (!(arguments[0] is ScriptSaveGameRootData root)) throw new HandlebarsException("{{readfile @root dir filename}} only allowed in SaveGame scripts");

            try
            {
                var filename = Path.Combine(arguments[1].ToString(), arguments[2].ToString());

                var fileContent = HelpersTools.GetFileContent(filename)?.Lines;

                if (fileContent == null) options.Inverse(output, context as object);
                else                     options.Template(output, fileContent);
            }
            catch (Exception error)
            {
                output.Write("{{readfile}} error " + EmpyrionScripting.ErrorFilter(error));
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

                using (var text = new StringWriter()) {
                    options.Template(text, context as object);
                    var content = text.ToString();

                    if (!append && HelpersTools.GetFileContent(filename)?.Text == content) return;

                    Directory.CreateDirectory(Path.GetDirectoryName(filename));

                    if (append) File.AppendAllText(filename, content);
                    else        File.WriteAllText (filename, content);
                }
            }
            catch (Exception error)
            {
                output.Write("{{writefile}} error " + EmpyrionScripting.ErrorFilter(error));
            }
        }

        [HandlebarTag("sendmessagetoplayer")]
        public static void SendMessageToPlayerHelper(TextWriter output, HelperOptions options, dynamic context, object[] arguments)
        {
            if (arguments.Length < 2) throw new HandlebarsException("{{sendmessagetoplayer @root playerid}} helper must have at least two argument: @root (playerid)");

            if (!(arguments[0] is ScriptSaveGameRootData root)) throw new HandlebarsException("{{sendmessagetoplayer @root playerid}} only allowed in SaveGame scripts");

            try
            {
                int.TryParse(arguments[1].ToString(), out var playerId);

                using (var text = new StringWriter())
                {
                    options.Template(text, context as object);

                    root.ModApi.Application.SendChatMessage(new Eleon.MessageData()
                    {
                        SenderType          = Eleon.SenderType.ServerInfo,
                        Channel             = Eleon.MsgChannel.Server,
                        RecipientEntityId   = playerId,
                        Text                = text.ToString()
                    });
                }
            }
            catch (Exception error)
            {
                output.Write("{{sendmessagetoplayer}} error " + EmpyrionScripting.ErrorFilter(error));
            }
        }

        [HandlebarTag("setdamage")]
        public static void BlockSetDamageHelper(TextWriter output, dynamic context, object[] arguments)
        {
            if (arguments.Length < 3) throw new HandlebarsException("{{setdamage @root block damage}} helper must have three argument: @root block damage");
            if (!(arguments[0] is ScriptSaveGameRootData root)) throw new HandlebarsException("{{setdamage @root block damage}} only allowed in SaveGame scripts");

            var block = arguments[1] as BlockData;
            int.TryParse(arguments[2].ToString(), out var damage);

            try
            {
                block.GetBlock().SetDamage(damage);
            }
            catch (Exception error)
            {
                output.Write("{{setdamage}} error " + EmpyrionScripting.ErrorFilter(error));
            }
        }

        [HandlebarTag("settype")]
        public static void BlockSetHelper(TextWriter output, dynamic context, object[] arguments)
        {
            if (arguments.Length < 3) throw new HandlebarsException("{{settype @root block typeid}} helper must have three argument: @root block typeid");
            if (!(arguments[0] is ScriptSaveGameRootData root)) throw new HandlebarsException("{{settype @root block typeid}} only allowed in SaveGame scripts");

            var block = arguments[1] as BlockData;
            int.TryParse(arguments[2].ToString(), out var type);

            try
            {
                block.GetBlock().Set(type);
            }
            catch (Exception error)
            {
                output.Write("{{settype}} error " + EmpyrionScripting.ErrorFilter(error));
            }
        }



    }
}
