using System.IO;
using EmpyrionScripting.Interface;

namespace EmpyrionScripting.Internal.Interface
{

    public interface IScriptRootData : IScriptModData
    {
        ScriptLanguage ScriptLanguage { get; set; }
        string Script { get; set; }
        TextWriter ScriptOutput { get; set; }
        bool ColorChanged { get; set; }
        bool BackgroundColorChanged { get; set; }
        bool FontSizeChanged { get; set; }
        int ScriptPriority { get; set; }
        ScriptInfo ScriptDiagnosticInfo { get; set; }
        bool Running { get; set; }

        IPlayfieldScriptData GetPlayfieldScriptData();
    }
}