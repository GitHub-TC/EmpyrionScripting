using EcfParser;
using Eleon.Modding;
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
        Color Color { get; set; }
        ConcurrentDictionary<string, object> Data { get; set; }
        bool DeviceLockAllowed { get; }
        IDisplayOutputConfiguration DisplayType { get; set; }
        IEntityData E { get; }
        string Error { get; set; }
        int FontSize { get; set; }
        string IngotIds { get; }
        string OreIds { get; }
        List<string> LcdTargets { get; set; }
        IPlayfieldData P { get; set; }
        string ScriptId { get; set; }
        int CycleCounter { get; }
        bool IsElevatedScript { get; }
        string Version { get; }
        EcfFile Configuration_Ecf { get; }

        IEnumerable<IEntity> GetAllEntities();
        IPlayfield GetCurrentPlayfield();
        IEnumerable<IEntity> GetEntities();
        ConcurrentDictionary<string, object> GetPersistendData();
    }
}