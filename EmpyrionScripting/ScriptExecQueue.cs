using EmpyrionNetAPIDefinitions;
using EmpyrionScripting.DataWrapper;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;

namespace EmpyrionScripting
{
    public class ScriptExecQueue
    {
        public class ScriptInfo
        {
            public int Count { get; set; }
            public DateTime LastStart { get; set; }
            public TimeSpan ExecTime { get; set; }
            public int RunningInstances;
        }
        public ConcurrentDictionary<string, ScriptInfo> ScriptRunInfo { get; set; } = new ConcurrentDictionary<string, ScriptInfo>();

        private PlayfieldScriptData playfieldScriptData;
        private Action<IScriptRootData> processScript;

        public int MainCount => _MainCount;
        private int _MainCount;

        public DateTime LastIterationUpdate { get; set; }

        public ConcurrentDictionary<string, IScriptRootData> WaitForExec { get; } = new ConcurrentDictionary<string, IScriptRootData>();
        public ConcurrentQueue<IScriptRootData>              ExecQueue { get; private set; } = new ConcurrentQueue<IScriptRootData>();

        public ScriptExecQueue(PlayfieldScriptData playfieldScriptData, Action<IScriptRootData> processScript)
        {
            this.playfieldScriptData = playfieldScriptData;
            this.processScript       = processScript;
        }

        public void Add(IScriptRootData data)
        {
            if (WaitForExec.TryAdd(data.ScriptId, data)) ExecQueue.Enqueue(data);
            else lock (ExecQueue) WaitForExec.AddOrUpdate(data.ScriptId, data, (i, d) => data);
        }

        static public Action<string, LogLevel> Log { get; set; }
        public int ScriptsCount { get; set; }
        public int QueueCount => ExecQueue.Count;

        public bool ExecNext()
        {
            var              found = false;
            IScriptRootData  data  = null;
            lock (ExecQueue) found = ExecQueue.TryDequeue(out data);
            if (!found) return false;

            data.GetPlayfieldScriptData().IncrementCycleCounter(data.ScriptId);

            if (!ThreadPool.QueueUserWorkItem(ExecScript, data))
            {
                Log($"EmpyrionScripting Mod: ExecNext NorThreadPoolFree {data.ScriptId}", LogLevel.Debug);
                return false;
            }
            return true;
        }

        private void ExecScript(object state)
        {
            if (!(state is IScriptRootData data)) return;

            try
            {
                if (data.E.EntityType == EntityType.Proxy)
                {
                    WaitForExec.TryRemove(data.ScriptId, out _);
                    return;
                }

                if (!ScriptRunInfo.TryGetValue(data.ScriptId, out var info)) info = new ScriptInfo();

                info.LastStart = DateTime.Now;
                info.Count++;

                Interlocked.Increment(ref info.RunningInstances);
                processScript(data);
                Interlocked.Decrement(ref info.RunningInstances);

                lock (ExecQueue) WaitForExec.TryRemove(data.ScriptId, out _);

                info.ExecTime += DateTime.Now - info.LastStart;

                ScriptRunInfo.AddOrUpdate(data.ScriptId, info, (id, i) => info);

                Interlocked.Increment(ref _MainCount);
                if(MainCount > ScriptsCount)
                {
                    if (Interlocked.Exchange(ref _MainCount, 0) > 0 && (DateTime.Now - LastIterationUpdate).TotalSeconds >= 1)
                    {
                        LastIterationUpdate = DateTime.Now;
                    }
                }
            }
            catch (Exception error)
            {
                Log($"EmpyrionScripting Mod: ExecNext {data.ScriptId} => {error}", LogLevel.Debug);
            }
        }

        public void Clear()
        {
            try                     { ScriptRunInfo = new ConcurrentDictionary<string, ScriptInfo>(ScriptRunInfo.Where(S => S.Value.RunningInstances > 0).ToArray()); }
            catch (Exception error) { ScriptRunInfo.Clear(); Log($"EmpyrionScripting Mod: Clear => {error}", LogLevel.Error); }

            WaitForExec.Clear();
            ExecQueue = new ConcurrentQueue<IScriptRootData>();
        }
    }
}