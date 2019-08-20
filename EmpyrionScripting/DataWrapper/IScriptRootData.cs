using System.Collections.Concurrent;
using System.Collections.Generic;
using Eleon.Modding;
using UnityEngine;

namespace EmpyrionScripting.DataWrapper
{
    public interface IScriptRootData
    {
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
        string Script { get; set; }
        string ScriptId { get; set; }

        IEntity[] GetCurrentEntites();
    }
}