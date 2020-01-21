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
        public static void FileExistsHelper(TextWriter output, object rootObject, HelperOptions options, dynamic context, object[] arguments)
        {
            if (arguments.Length != 2) throw new HandlebarsException("{{fileexists dir filename}} helper must have two argument: (dir) (filename)");

            if (!(rootObject is ScriptSaveGameRootData root)) throw new HandlebarsException("{{readfile dir filename}} only allowed in SaveGame scripts");

            try
            {
                if (File.Exists(Path.Combine(root.MainScriptPath, arguments[0].ToString(), arguments[1].ToString()))) options.Template(output, context as object);
                else                                                                                                  options.Inverse (output, context as object);
            }
            catch (Exception error)
            {
                output.Write("{{fileexists}} error " + EmpyrionScripting.ErrorFilter(error));
            }
        }

        [HandlebarTag("filelist")]
        public static void FileListHelper(TextWriter output, object rootObject, HelperOptions options, dynamic context, object[] arguments)
        {
            if (arguments.Length != 2 && arguments.Length != 3) throw new HandlebarsException("{{filelist dir filename [recursive]}} helper must have at least three arguments: @root (dir) (filename) [recursive]");

            if (!(rootObject is ScriptSaveGameRootData root)) throw new HandlebarsException("{{filelist}} only allowed in SaveGame scripts");

            try
            {
                bool.TryParse(arguments.Length == 3 ? arguments[2].ToString() : "false", out var recursive);
                options.Template(output, 
                    Directory.EnumerateFiles(
                        Path.Combine(root.MainScriptPath, arguments[0].ToString()), arguments[1].ToString(), 
                        recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly));
            }
            catch (Exception error)
            {
                output.Write("{{filelist}} error " + EmpyrionScripting.ErrorFilter(error));
            }
        }

        [HandlebarTag("readfile")]
        public static void ReadFileHelper(TextWriter output, object rootObject, HelperOptions options, dynamic context, object[] arguments)
        {
            if (arguments.Length != 2) throw new HandlebarsException("{{readfiledir dir filename}} helper must have two argument: (dir) (filename)");

            if (!(rootObject is ScriptSaveGameRootData root)) throw new HandlebarsException("{{readfile dir filename}} only allowed in SaveGame scripts");

            try
            {
                var filename = Path.Combine(root.MainScriptPath, arguments[0].ToString(), arguments[1].ToString());

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
        public static void WriteFileHelper(TextWriter output, object rootObject, HelperOptions options, dynamic context, object[] arguments)
        {
            if (arguments.Length < 2) throw new HandlebarsException("{{writefile dir filename [append]}} helper must have at least two argument: (dir) (filename)");

            if (!(rootObject is ScriptSaveGameRootData root)) throw new HandlebarsException("{{writefile dir filename}} only allowed in SaveGame scripts");

            try
            {
                var filename = Path.Combine(root.MainScriptPath, arguments[0].ToString(), arguments[1].ToString());
                bool.TryParse(arguments.Length > 2 ? arguments[2].ToString() : null, out var append);

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
        public static void SendMessageToPlayerHelper(TextWriter output, object rootObject, HelperOptions options, dynamic context, object[] arguments)
        {
            if (arguments.Length < 1) throw new HandlebarsException("{{sendmessagetoplayer playerid}} helper must have at least one argument: (playerid)");

            if (!(rootObject is ScriptSaveGameRootData root)) throw new HandlebarsException("{{sendmessagetoplayer playerid}} only allowed in SaveGame scripts");

            try
            {
                int.TryParse(arguments[0].ToString(), out var playerId);

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
        public static void BlockSetDamageHelper(TextWriter output, object root, dynamic context, object[] arguments)
        {
            if (arguments.Length < 2) throw new HandlebarsException("{{setdamage block damage}} helper must have two argument: block damage");
            if (!(root is ScriptSaveGameRootData)) throw new HandlebarsException("{{setdamage block damage}} only allowed in SaveGame scripts");

            var block = arguments[0] as BlockData;
            int.TryParse(arguments[1].ToString(), out var damage);

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
        public static void BlockSetHelper(TextWriter output, object root, dynamic context, object[] arguments)
        {
            if (arguments.Length < 2) throw new HandlebarsException("{{settype block typeid}} helper must have two argument: block typeid");
            if (!(root is ScriptSaveGameRootData)) throw new HandlebarsException("{{settype block typeid}} only allowed in SaveGame scripts");

            var block = arguments[0] as BlockData;
            int.TryParse(arguments[1].ToString(), out var type);

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
