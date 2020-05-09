using EmpyrionNetAPIDefinitions;
using EmpyrionScripting.Internal.Interface;
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
            lock (ExecQueue)
            {
                if (WaitForExec.TryAdd(data.ScriptId, data)) ExecQueue.Enqueue(data);
                else WaitForExec.AddOrUpdate(data.ScriptId, data, (i, d) => data);
            }
        }

        public void CheckForEmergencyRestart()
        {
            lock (ExecQueue)
            {
                if (ExecQueue.IsEmpty && WaitForExec.Count > 0)
                {
                    Log($"EmpyrionScripting Mod: ExecQueue restart... #{WaitForExec.Count}", LogLevel.Message);
                    ThreadPool.GetAvailableThreads(out var availableWorkerThreads, out var availableCompletionPortThreads);
                    Log($"EmpyrionScripting Mod: ThreadPool available: WorkerThreads:{availableWorkerThreads} CompletionPortThreads:{availableCompletionPortThreads}", LogLevel.Message);
                    ThreadPool.GetMaxThreads(out var maxWorkerThreads, out var maxCompletionPortThreads);
                    Log($"EmpyrionScripting Mod: ThreadPool max: WorkerThreads:{maxWorkerThreads} CompletionPortThreads:{maxCompletionPortThreads}", LogLevel.Message);
                    ThreadPool.GetMinThreads(out var minWorkerThreads, out var minCompletionPortThreads);
                    Log($"EmpyrionScripting Mod: ThreadPool min: WorkerThreads:{minWorkerThreads} CompletionPortThreads:{minCompletionPortThreads}", LogLevel.Message);
                    WaitForExec.Clear(); // robust error restart with fresh data
                }
            }
        }

        static public Action<string, LogLevel> Log { get; set; }
        public int ScriptsCount { get; set; }
        public int QueueCount => ExecQueue.Count;

        public bool ExecNext()
        {
            switch (EmpyrionScripting.Configuration.Current.ExecMethod)
            {
                case ExecMethod.ThreadPool: return ThreadPoolExecNext();
                case ExecMethod.Direct:     return DirectExecNext();
                default:                    return false;  
            }
        }

        public bool ThreadPoolExecNext()
        {
            var              found = false;
            IScriptRootData  data  = null;
            lock (ExecQueue) found = ExecQueue.TryDequeue(out data);
            if (!found) return false;

            ((PlayfieldScriptData)data.GetPlayfieldScriptData()).IncrementCycleCounter(data.ScriptId);

            if (!ThreadPool.QueueUserWorkItem(ExecScript, data))
            {
                Log($"EmpyrionScripting Mod: ExecNext NorThreadPoolFree {data.ScriptId}", LogLevel.Debug);
                return false;
            }
            return true;
        }

        public bool DirectExecNext()
        {
            var found = false;
            IScriptRootData data = null;
            lock (ExecQueue) found = ExecQueue.TryDequeue(out data);
            if (!found) return false;

            ((PlayfieldScriptData)data.GetPlayfieldScriptData()).IncrementCycleCounter(data.ScriptId);

            try
            {
                ExecScript(data);
            }
            catch (Exception error)
            {
                Log($"EmpyrionScripting Mod: DirectExecNext {data.ScriptId}:{error}", LogLevel.Debug);
            }

            return true;
        }

        private void ExecScript(object state)
        {
            if (!(state is IScriptRootData data)) return;

            try
            {
                if (data.E.EntityType == EntityType.Proxy || data.E.EntityType == EntityType.Unknown)
                {
                    lock (ExecQueue) WaitForExec.TryRemove(data.ScriptId, out _);
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

            lock (ExecQueue)
            {
                WaitForExec.Clear();
                while (ExecQueue.TryDequeue(out _)) ;
            }
        }
    }
}