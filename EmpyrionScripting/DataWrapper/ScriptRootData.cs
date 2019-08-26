using Eleon.Modding;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using UnityEngine;

namespace EmpyrionScripting.DataWrapper
{
    public class ScriptRootData : IScriptRootData
    {
        public bool DeviceLockAllowed { get; }

        private ConcurrentDictionary<string, object> _PersistendData;
        private IEntity[] currentEntites;
        private IPlayfield playfield;
        private IEntity entity;

        public ScriptRootData()
        {
            _e = new Lazy<IEntityData>(() => new EntityData(entity));
        }

        public ScriptRootData(IEntity[] currentEntities, IPlayfield playfield, IEntity entity, bool deviceLockAllowed, ConcurrentDictionary<string, object> persistendData) : this()
        {
            DeviceLockAllowed = deviceLockAllowed;
            _PersistendData = persistendData;
            this.currentEntites = currentEntities;
            this.playfield = playfield;
            this.entity = entity;
        }

        public ScriptRootData(ScriptRootData data) : this(data.currentEntites, data.playfield, data.entity, data.DeviceLockAllowed, data._PersistendData)
        {
            _p = data._p;
            _e = data._e;
            DisplayType = data.DisplayType;
        }

        public ConcurrentDictionary<string, object> GetPersistendData() => _PersistendData;
        public IEntity[] GetCurrentEntites() => currentEntites;

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
    }
}
