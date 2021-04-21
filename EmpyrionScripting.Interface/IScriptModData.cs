using Eleon.Modding;
using System;
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
        Dictionary<string, string> Ids { get; }
        string IngotIds { get; }
        string OreIds { get; }
        List<string> LcdTargets { get; set; }
        IPlayfieldData P { get; set; }
        string ScriptId { get; set; }
        int CycleCounter { get; }
        bool IsElevatedScript { get; }
        string Version { get; }
        IConfigEcfAccess ConfigEcfAccess { get; }
        bool ScriptWithinMainThread { get; set; }
        bool ScriptNeedsMainThread { get; set; }
        ConcurrentDictionary<string, object> CacheData { get; }
        string ScriptingModInfoData { get; }
        ConcurrentDictionary<string, string> ScriptingModScriptsInfoData { get; }
        DateTime DateTimeNow { get; }

        IEnumerable<IEntity> GetAllEntities();
        IPlayfield GetCurrentPlayfield();
        IEnumerable<IEntity> GetEntities();
        ConcurrentDictionary<string, object> GetPersistendData();
        Func<bool> ScriptLoopTimeLimitReached { get; }
    }
}