using EmpyrionNetAPIDefinitions;
using EmpyrionScripting.DataWrapper;
using System;
using System.Collections.Concurrent;
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
        }
        public ConcurrentDictionary<string, ScriptInfo> ScriptRunInfo { get; } = new ConcurrentDictionary<string, ScriptInfo>();

        private Action<ScriptRootData> processScript;

        public ConcurrentDictionary<string, ScriptRootData> WaitForExec { get; } = new ConcurrentDictionary<string, ScriptRootData>();
        public ConcurrentQueue<ScriptRootData>              ExecQueue { get; private set; } = new ConcurrentQueue<ScriptRootData>();

        public ScriptExecQueue(Action<ScriptRootData> processScript)
        {
            this.processScript = processScript;
        }

        public void Add(ScriptRootData data)
        {
            if (WaitForExec.TryAdd(data.ScriptId, data)) ExecQueue.Enqueue(data);
            else lock (ExecQueue) WaitForExec.AddOrUpdate(data.ScriptId, data, (i, d) => { data.E.DeviceLockAllowed = data.E.DeviceLockAllowed || data.E.DeviceLockAllowed; return data; });
        }

        static public Action<string, LogLevel> Log { get; set; }

        public void ExecNext()
        {
            var             found = false;
            ScriptRootData  data  = null;
            lock (ExecQueue) found = ExecQueue.TryDequeue(out data);
            if (!found) return;

            if(!ThreadPool.QueueUserWorkItem(ExecScript, data)) Log($"EmpyrionScripting Mod: ExecNext NorThreadPoolFree {data.ScriptId}", LogLevel.Debug);
        }

        private void ExecScript(object state)
        {
            if (!(state is ScriptRootData data)) return;

            try
            {
                if (data.E.EntityType == EntityType.Proxy) return;

                if (!ScriptRunInfo.TryGetValue(data.ScriptId, out var info)) info = new ScriptInfo();

                info.LastStart = DateTime.Now;
                info.Count++;

                processScript(data);
                lock (ExecQueue) WaitForExec.TryRemove(data.ScriptId, out _);

                info.ExecTime += DateTime.Now - info.LastStart;

                ScriptRunInfo.AddOrUpdate(data.ScriptId, info, (id, i) => info);
            }
            catch (Exception error)
            {
                Log($"EmpyrionScripting Mod: ExecNext {data.ScriptId} => {error}", LogLevel.Debug);
            }
        }

        public void Clear()
        {
            ScriptRunInfo.Clear();
            WaitForExec.Clear();
            ExecQueue = new ConcurrentQueue<ScriptRootData>();
        }
    }
}