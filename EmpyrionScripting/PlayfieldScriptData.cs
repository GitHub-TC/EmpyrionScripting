using Eleon.Modding;
using EmpyrionScripting.DataWrapper;
using System;
using System.Collections.Concurrent;
using System.Threading;

namespace EmpyrionScripting
{
    public class PlayfieldScriptData
    {
        public string PlayfieldName { get; set; }
        public IPlayfield Playfield { get; set; }
        public IEntity[] CurrentEntities { get; set; }
        public bool PauseScripts { get; set; } = true;
        public ConcurrentDictionary<int, EventStore> EventStore { get; set; } = new ConcurrentDictionary<int, EventStore>();
        public ConcurrentDictionary<string, Func<object, string>> LcdCompileCache = new ConcurrentDictionary<string, Func<object, string>>();

        public ConcurrentDictionary<string, object> PersistendData { get; set; } = new ConcurrentDictionary<string, object>();
        public ScriptExecQueue ScriptExecQueue { get; set; }

        public int CycleCounter => _CycleCounter;
        int _CycleCounter;
        public void IncrementCycleCounter(){ Interlocked.Increment(ref _CycleCounter); }
        public bool DeviceLockAllowed => (CycleCounter % EmpyrionScripting.Configuration.Current.DeviceLockOnlyAllowedEveryXCycles) == 0;

        public int Iteration => _Iteration;
        private int _Iteration;
        public void IncrementIteration(){ Interlocked.Increment(ref _Iteration); }

        public EntityDelegate Playfield_OnEntityLoaded { get; } 
        public EntityDelegate Playfield_OnEntityUnloaded { get; }

        public PlayfieldScriptData(EmpyrionScripting parent)
        {
            Playfield_OnEntityLoaded   = (IEntity entity) => EventStore.AddOrUpdate(entity.Id, id => new EventStore(entity), (id, store) => store);
            Playfield_OnEntityUnloaded = (IEntity entity) => { if (EventStore.TryRemove(entity.Id, out var store)) store.Dispose(); };

            ScriptExecQueue            = new ScriptExecQueue(this, D => parent.ProcessScript(this, D));
        }
    }

}
