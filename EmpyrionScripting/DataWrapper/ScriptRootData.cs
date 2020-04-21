using Eleon.Modding;
using EmpyrionNetAPIAccess;
using EmpyrionScripting.CsHelper;
using EmpyrionScripting.Interface;
using EmpyrionScripting.Internal.Interface;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace EmpyrionScripting.DataWrapper
{

    public class ScriptRootData : IScriptRootData
    {
        private static readonly Assembly CurrentAssembly = Assembly.GetAssembly(typeof(ScriptRootData));
        private readonly PlayfieldScriptData _PlayfieldScriptData;

        private ConcurrentDictionary<string, object> _PersistendData;
        private IEntity[] currentEntities;
        private IEntity[] allEntities;
        private IPlayfield playfield;
        private IEntity entity;

        public IEventStore SignalEventStore { get; }

        public ScriptRootData()
        {
            Console = new ConsoleMock(this);
            CsRoot  = new CsScriptFunctions(this);

            _e = new Lazy<IEntityData>(() => new EntityData(playfield, entity));
        }

        public ScriptRootData(
            PlayfieldScriptData playfieldScriptData,
            IEntity[] allEntities,
            IEntity[] currentEntities, 
            IPlayfield playfield, 
            IEntity entity, 
            ConcurrentDictionary<string, object> persistendData, 
            EventStore eventStore) : this()
        {
            _PlayfieldScriptData        = playfieldScriptData;
            _PersistendData             = persistendData;
            this.currentEntities        = currentEntities;
            this.allEntities            = allEntities;
            this.playfield              = playfield;
            this.entity                 = entity;
            SignalEventStore            = eventStore;
            IsElevatedScript = this is ScriptSaveGameRootData || entity?.Faction.Group == FactionGroup.Admin;
        }

        public ScriptRootData(ScriptRootData data) : this(data._PlayfieldScriptData, data.allEntities, data.currentEntities, data.playfield, data.entity, data._PersistendData, (EventStore)data.SignalEventStore)
        {
            _p = data._p;
            _e = data._e;
            DisplayType = data.DisplayType;
        }

        public string Version { get; } = $"{CurrentAssembly.GetAttribute<AssemblyTitleAttribute>()?.Title } by {CurrentAssembly.GetAttribute<AssemblyCompanyAttribute>()?.Company} Version:{CurrentAssembly.GetAttribute<AssemblyFileVersionAttribute>()?.Version}";

        public bool IsElevatedScript { get; }
        public ICsScriptFunctions CsRoot { get; }
        public IConsoleMock Console { get; }

        public IPlayfieldScriptData GetPlayfieldScriptData() => _PlayfieldScriptData;
        public ConcurrentDictionary<string, object> GetPersistendData() => _PersistendData;
        public IEnumerable<IEntity> AllEntities => IsElevatedScript ? allEntities : Enumerable.Empty<IEntity>();
        public IEnumerable<IEntity> Entites => currentEntities
                    .Where(SafeIsNoProxyCheck)
                    .Where(e => e.Faction.Id == E.GetCurrent().Faction.Id)
                    .Where(e => IsElevatedScript || Vector3.Distance(e.Position, E.Pos) <= EmpyrionScripting.Configuration.Current.EntityAccessMaxDistance);

        public IPlayfield GetCurrentPlayfield() => playfield;

        public string OreIds => "2248,2249,2250,2251,2252,2253,2254,2269,2270,2284,2293,2297";
        public string IngotIds => "2271,2272,2273,2274,2275,2276,2277,2278,2279,2280,2281,2285,2294,2298";

        public IPlayfieldData P { get => _p == null ? _p = new PlayfieldData(playfield) : _p; set => _p = value; }
        private IPlayfieldData _p;

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
        public ScriptLanguage ScriptLanguage { get; set; }
        public string Script { get; set; }
        public TextWriter ScriptOutput { get; set; }
        public IDisplayOutputConfiguration DisplayType { get; set; }
        public string Error { get; set; }
        public string ScriptId { get; set; }

        public int CycleCounter => ((PlayfieldScriptData)GetPlayfieldScriptData()).CycleCounter(ScriptId);
        public virtual bool DeviceLockAllowed => ((PlayfieldScriptData)GetPlayfieldScriptData()).DeviceLockAllowed(ScriptId);

        private static bool SafeIsNoProxyCheck(IEntity entity)
        {
            try { return entity != null && entity.Type != EntityType.Proxy; }
            catch { return false; }
        }

    }
}
