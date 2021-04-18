using System;
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
        Func<bool> ScriptLoopTimeLimitReached { get; set; }

        IPlayfieldScriptData GetPlayfieldScriptData();
    }
}