using System.Collections.Generic;
using System.IO;
using Eleon.Modding;
using EmpyrionScripting.Interface;

namespace EmpyrionScripting.Internal.Interface
{

    public interface IScriptRootData : IScriptModData
    {
        ScriptLanguage ScriptLanguage { get; set; }
        string Script { get; set; }
        TextWriter ScriptOutput { get; set; }

        IPlayfieldScriptData GetPlayfieldScriptData();
        IEnumerable<IEntity> AllEntities { get; }
        IEnumerable<IEntity> Entites { get; }

        IPlayfield GetCurrentPlayfield();
    }
}