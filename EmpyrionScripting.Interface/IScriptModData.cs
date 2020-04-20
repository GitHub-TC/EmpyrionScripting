using System.Collections.Concurrent;
using System.Collections.Generic;
using UnityEngine;

namespace EmpyrionScripting.Interface
{
    public interface IScriptModData
    {
        ICsScriptFunctions CsRoot { get; }
        IConsoleMock Console { get; }
        IEventStore SignalEventStore { get; }
        Color BackgroundColor { get; set; }
        bool BackgroundColorChanged { get; set; }
        Color Color { get; set; }
        bool ColorChanged { get; set; }
        ConcurrentDictionary<string, object> Data { get; set; }
        bool DeviceLockAllowed { get; }
        IDisplayOutputConfiguration DisplayType { get; set; }
        IEntityData E { get; }
        string Error { get; set; }
        int FontSize { get; set; }
        bool FontSizeChanged { get; set; }
        string IngotIds { get; }
        List<string> LcdTargets { get; set; }
        string OreIds { get; }
        IPlayfieldData P { get; set; }
        string ScriptId { get; set; }
        int CycleCounter { get; }
        bool IsElevatedScript { get; }
        ConcurrentDictionary<string, object> GetPersistendData();
    }
}