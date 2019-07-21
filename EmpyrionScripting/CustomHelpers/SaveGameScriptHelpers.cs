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

    }
}
