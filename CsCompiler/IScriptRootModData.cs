using System.IO;
using EmpyrionScripting.Interface;

namespace EmpyrionScripting.Internal.Interface
{
    public interface IScriptRootModData : IScriptModData
    {
        ScriptLanguage ScriptLanguage { get; set; }
        string Script { get; set; }
        TextWriter ScriptOutput { get; set; }
        bool ColorChanged { get; set; }
        bool BackgroundColorChanged { get; set; }
        bool FontSizeChanged { get; set; }
        int ScriptPriority { get; set; }
        IScriptInfo ScriptDiagnosticInfo { get; set; }
        bool Running { get; set; }
    }
}