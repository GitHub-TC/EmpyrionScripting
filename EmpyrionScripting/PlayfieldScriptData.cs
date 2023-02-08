using Eleon.Modding;
using EmpyrionScripting.DataWrapper;
using EmpyrionScripting.Interface;
using EmpyrionScripting.Internal.Interface;
using System;
using System.Collections.Concurrent;

namespace EmpyrionScripting
{
    public class ExclusiveAccess : IExclusiveAccess
    {
        public string CommandId { get; set; }
        public int EntityId { get; set; }
        public string EntityName { get; set; }
    }

    public class PlayfieldScriptData : IPlayfieldScriptData
    {
        public string PlayfieldName { get; set; }
        public IPlayfield Playfield { get; set; }
        public IEntity[] CurrentEntities { get; set; }
        public IEntity[] AllEntities { get; set; }
        public bool PauseScripts { get; set; } = true;
        public ConcurrentDictionary<int, EntityCultureInfo> EntityCultureInfo { get; set; } = new ConcurrentDictionary<int, EntityCultureInfo>();
        public ConcurrentDictionary<int, IEventStore> EventStore { get; set; } = new ConcurrentDictionary<int, IEventStore>();
        public ConcurrentDictionary<string, Func<object, string>> LcdCompileCache = new ConcurrentDictionary<string, Func<object, string>>();
        public ConcurrentQueue<IItemMoveInfo> MoveLostItems { get; } = new ConcurrentQueue<IItemMoveInfo>();

        public ConcurrentDictionary<string, object> PersistendData { get; set; } = new ConcurrentDictionary<string, object>();
        public ConcurrentDictionary<int, IExclusiveAccess> EntityExclusiveAccess { get; set; } = new ConcurrentDictionary<int, IExclusiveAccess>();
        public ScriptExecQueue ScriptExecQueue { get; set; }

        private ConcurrentDictionary<string, int> CycleCounterStore { get; set; } = new ConcurrentDictionary<string, int>();
        public int CycleCounter(string scriptId) => CycleCounterStore.GetOrAdd(scriptId, I => 0);
        public void IncrementCycleCounter(string scriptId) { CycleCounterStore.AddOrUpdate(scriptId, 0, (I, C) => C + 1); }
        public bool DeviceLockAllowed(string scriptId) => (CycleCounter(scriptId) % EmpyrionScripting.Configuration.Current.DeviceLockOnlyAllowedEveryXCycles) == 0;

        public EntityDelegate Playfield_OnEntityLoaded { get; }
        public EntityDelegate Playfield_OnEntityUnloaded { get; }
        public ConcurrentDictionary<int, ulong> LastPOIVisited { get; } = new ConcurrentDictionary<int, ulong>();

        public PlayfieldScriptData(EmpyrionScripting parent)
        {
            Playfield_OnEntityLoaded   = (IEntity entity) => AddEntity(entity);
            Playfield_OnEntityUnloaded = (IEntity entity) => RemoveEntity(entity);

            ScriptExecQueue = new ScriptExecQueue(D => parent.ProcessScript(this, D));
        }

        public void RemoveEntity(IEntity entity)
        {
            if (EventStore.TryRemove(entity.Id, out var store)) ((EventStore)store).Dispose();
        }

        public IEventStore AddEntity(IEntity entity) => entity == null ? null : EventStore.AddOrUpdate(entity.Id, id => new EventStore(entity), (id, store) => store);
    }

}
