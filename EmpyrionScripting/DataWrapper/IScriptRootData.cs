using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using Eleon.Modding;
using EmpyrionScripting.CsHelper;
using UnityEngine;

namespace EmpyrionScripting.DataWrapper
{
    public interface IScriptRootData
    {
        CsScriptFunctions CsRoot { get; }
        EventStore SignalEventStore { get; }
        Color BackgroundColor { get; set; }
        bool BackgroundColorChanged { get; set; }
        Color Color { get; set; }
        bool ColorChanged { get; set; }
        ConcurrentDictionary<string, object> Data { get; set; }
        bool DeviceLockAllowed { get; }
        DisplayOutputConfiguration DisplayType { get; set; }
        IEntityData E { get; }
        string Error { get; set; }
        int FontSize { get; set; }
        bool FontSizeChanged { get; set; }
        string IngotIds { get; }
        List<string> LcdTargets { get; set; }
        string OreIds { get; }
        PlayfieldData P { get; set; }
        ScriptLanguage ScriptLanguage { get; set; }
        string Script { get; set; }
        TextWriter ScriptOutput { get; set; }
        string ScriptId { get; set; }
        int CycleCounter { get; }

        PlayfieldScriptData GetPlayfieldScriptData();
        IEntity[] GetAllEntites();
        IEntity[] GetCurrentEntites();
        IPlayfield GetCurrentPlayfield();
        ConcurrentDictionary<string, object> GetPersistendData();
    }
}