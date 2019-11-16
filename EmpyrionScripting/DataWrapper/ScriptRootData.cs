using Eleon.Modding;
using EmpyrionNetAPIAccess;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace EmpyrionScripting.DataWrapper
{
    public class ScriptRootData : IScriptRootData
    {
        private static readonly Assembly CurrentAssembly = Assembly.GetAssembly(typeof(ScriptRootData));
        private readonly PlayfieldScriptData _PlayfieldScriptData;

        private ConcurrentDictionary<string, object> _PersistendData;
        private IEntity[] currentEntites;
        private IPlayfield playfield;
        private IEntity entity;

        public EventStore SignalEventStore { get; }

        public ScriptRootData()
        {
            _e = new Lazy<IEntityData>(() => new EntityData(playfield, entity));
        }

        public ScriptRootData(PlayfieldScriptData playfieldScriptData, IEntity[] currentEntities, IPlayfield playfield, IEntity entity, ConcurrentDictionary<string, object> persistendData, EventStore eventStore) : this()
        {
            _PlayfieldScriptData = playfieldScriptData;
            _PersistendData = persistendData;
            this.currentEntites = currentEntities;
            this.playfield = playfield;
            this.entity = entity;
            SignalEventStore = eventStore;
        }

        public ScriptRootData(ScriptRootData data) : this(data._PlayfieldScriptData, data.currentEntites, data.playfield, data.entity, data._PersistendData, data.SignalEventStore)
        {
            _p = data._p;
            _e = data._e;
            DisplayType = data.DisplayType;
        }

        public string Version { get; } = $"{CurrentAssembly.GetAttribute<AssemblyTitleAttribute>()?.Title } by {CurrentAssembly.GetAttribute<AssemblyCompanyAttribute>()?.Company} Version:{CurrentAssembly.GetAttribute<AssemblyFileVersionAttribute>()?.Version}";

        public PlayfieldScriptData GetPlayfieldScriptData() => _PlayfieldScriptData;
        public ConcurrentDictionary<string, object> GetPersistendData() => _PersistendData;
        public IEntity[] GetCurrentEntites() => currentEntites;

        public IPlayfield GetCurrentPlayfield() => playfield;

        public string OreIds => "2248,2249,2250,2251,2252,2253,2254,2269,2270,2284,2293,2297";
        public string IngotIds => "2271,2272,2273,2274,2275,2276,2277,2278,2279,2280,2281,2285,2294,2298";

        public PlayfieldData P { get => _p == null ? _p = new PlayfieldData(playfield) : _p; set => _p = value; }
        private PlayfieldData _p;

        public IEntityData E => _e.Value; 
        private readonly Lazy<IEntityData> _e;

        public List<string> LcdTargets { get; set; } = new List<string>();
        public bool FontSizeChanged { get; set; }
        public int FontSize { get; set; }
        public bool ColorChanged { get; set; }
        public Color Color { get; set; }
        public bool BackgroundColorChanged { get; set; }
        public Color BackgroundColor { get; set; }
        public ConcurrentDictionary<string, object> Data { get; set; } = new ConcurrentDictionary<string, object>();
        public string Script { get; set; }
        public DisplayOutputConfiguration DisplayType { get; set; }
        public string Error { get; set; }
        public string ScriptId { get; set; }

        public int CycleCounter => GetPlayfieldScriptData().CycleCounter(ScriptId);
        public virtual bool DeviceLockAllowed => GetPlayfieldScriptData().DeviceLockAllowed(ScriptId);
    }
}
