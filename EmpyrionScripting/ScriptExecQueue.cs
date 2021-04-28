using EmpyrionNetAPIDefinitions;
using EmpyrionScripting.DataWrapper;
using EmpyrionScripting.Internal.Interface;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace EmpyrionScripting
{
    public partial class ScriptExecQueue
    {
        public ConcurrentDictionary<string, ScriptInfo> ScriptRunInfo { get; set; } = new ConcurrentDictionary<string, ScriptInfo>();

        private Action<IScriptRootData> ProcessScript { get; }

        public int MainCount => _MainCount;
        private int _MainCount;

        public DateTime LastIterationUpdate { get; set; }

        public ConcurrentDictionary<string, IScriptRootData> WaitForExec { get; } = new ConcurrentDictionary<string, IScriptRootData>();
        public ConcurrentQueue<IScriptRootData>              ExecQueue { get; private set; } = new ConcurrentQueue<IScriptRootData>();

        static public Action<string, LogLevel> Log { get; set; }
        public int ScriptsCount { get; set; }
        public int QueueCount => ExecQueue.Count;

        private BackgroundWorker Worker { get; }
        public ConcurrentQueue<IScriptRootData> BackgroundExecQueue { get; private set; } = new ConcurrentQueue<IScriptRootData>();

        public ConcurrentDictionary<string, bool> ScriptNeedsMainThread { get; set; } = new ConcurrentDictionary<string, bool>();
        public int GameUpdateScriptLoopTimeLimitReached { get; private set; }

        public ScriptExecQueue(Action<IScriptRootData> processScript)
        {
            ProcessScript           = processScript;
            Worker                  = new BackgroundWorker();
            Worker.DoWork           += (s, e) => {
                var toDo = new List<IScriptRootData>();
                while(BackgroundExecQueue.TryDequeue(out var data)) toDo.Add(data);
                Parallel.ForEach(toDo, ExecScriptWithMainThreadCheck);
            };
        }

        public void Add(IScriptRootData data)
        {
            if (ScriptNeedsMainThread.TryGetValue(data.ScriptId, out var needMainThread)) data.ScriptNeedsMainThread = needMainThread;
            else ScriptNeedsMainThread.TryAdd(data.ScriptId, false);

            lock (ExecQueue)
            {
                if (WaitForExec.TryAdd(data.ScriptId, data)) ExecQueue.Enqueue(data);
                else WaitForExec.AddOrUpdate(data.ScriptId, data, (scriptId, oldData) => data);
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

        public void ExecNext(int maxCount, int scriptsSyncExecution, ref int syncExecCount, Stopwatch scriptLoopTimeLimiter)
        {
            var scriptLoopTimeLimiterStopwatch = scriptLoopTimeLimiter;
            Func<bool> timeLimitBackgroundReached = () => scriptLoopTimeLimiterStopwatch.ElapsedMilliseconds > EmpyrionScripting.Configuration.Current.ScriptLoopBackgroundTimeLimiterMS;
            Func<bool> timeLimitSyncReached       = () => scriptLoopTimeLimiterStopwatch.ElapsedMilliseconds > EmpyrionScripting.Configuration.Current.ScriptLoopSyncTimeLimiterMS;

            for (int i = maxCount - 1; i >= 0; i--)
            {
                if (ExecQueue.TryPeek(out var data))
                {
                    if (data.ScriptNeedsMainThread)
                    {
                        Interlocked.Increment(ref syncExecCount);

                        if (syncExecCount > scriptsSyncExecution) lock (ExecQueue) { if (ExecQueue.TryDequeue(out var reinsert)) ExecQueue.Enqueue(reinsert); }
                        else
                        {
                            ((ScriptRootData)data).ScriptLoopTimeLimitReached = timeLimitSyncReached;
                            ExecNext();
                        }
                    }
                    else
                    {
                        ((ScriptRootData)data).ScriptLoopTimeLimitReached = timeLimitBackgroundReached;
                        ExecNext();
                    }
                }
                else return;

                if (timeLimitSyncReached())
                {
                    GameUpdateScriptLoopTimeLimitReached++;
                    return;
                }
            }
        }

        private bool ExecNext()
        {
            var found = false;
            IScriptRootData data = null;
            lock (ExecQueue) found = ExecQueue.TryDequeue(out data);
            if (!found) return false;

            ((PlayfieldScriptData)data.GetPlayfieldScriptData()).IncrementCycleCounter(data.ScriptId);

            return EmpyrionScripting.Configuration.Current.ExecMethod switch
            {
                ExecMethod.ThreadPool       => data.ScriptNeedsMainThread ? DirectExecNext(data) : ThreadPoolExecNext(data),
                ExecMethod.BackgroundWorker => data.ScriptNeedsMainThread ? DirectExecNext(data) : BackgroundWorkerExecNext(data),
                ExecMethod.Direct           => DirectExecNext(data),
                _                           => false,
            };
        }

        public bool BackgroundWorkerExecNext(IScriptRootData data)
        {
            BackgroundExecQueue.Enqueue(data);
            if (!Worker.IsBusy) Worker.RunWorkerAsync();
            return true;
        }

        public bool ThreadPoolExecNext(IScriptRootData data)
        {
            if (!ThreadPool.QueueUserWorkItem(ExecScriptWithMainThreadCheck, data))
            {
                Log($"EmpyrionScripting Mod: ExecNext NorThreadPoolFree {data.ScriptId}", LogLevel.Debug);
                return false;
            }
            return true;
        }

        public bool DirectExecNext(IScriptRootData data)
        {
            data.ScriptWithinMainThread = true;

            try                     { ExecScript(data); }
            catch (Exception error) { Log($"EmpyrionScripting Mod: DirectExecNext {data.ScriptId}:{error}", LogLevel.Debug); }

            return true;
        }

        private void ExecScriptWithMainThreadCheck(object state)
        {
            if (!(state is IScriptRootData data)) return;
            ExecScript(data);
            if (data.ScriptNeedsMainThread) Add(data);
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

                if (!ScriptRunInfo.TryGetValue(data.ScriptId, out var info)) info = new ScriptInfo() {
                    ScriptPriority = data.ScriptPriority,
                    ScriptId = data.ScriptId, 
                    EntityId = data.E.Id 
                };

                info.Count++;
                if (data.ScriptPriority <= 1 || info.Count % data.ScriptPriority == 0)
                {
                    info.IsElevatedScript       = data.IsElevatedScript;
                    info.LastStart              = DateTime.Now;
                    data.ScriptDiagnosticInfo   = info;

                    Interlocked.Increment(ref info._RunningInstances);
                    data.Running = true;
                    ProcessScript(data);
                    Interlocked.Decrement(ref info._RunningInstances);

                    info.NeedsMainThread = data.ScriptNeedsMainThread;
                    ScriptNeedsMainThread.TryUpdate(data.ScriptId, data.ScriptNeedsMainThread, false);

                    info.ExecTime += DateTime.Now - info.LastStart;
                }

                lock (ExecQueue) WaitForExec.TryRemove(data.ScriptId, out _);

                ScriptRunInfo.AddOrUpdate(data.ScriptId, info, (id, i) => info);

                Interlocked.Increment(ref _MainCount);
                if (MainCount > ScriptsCount)
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
            try                     { ScriptRunInfo = new ConcurrentDictionary<string, ScriptInfo>(ScriptRunInfo.Where(S => S.Value._RunningInstances > 0).ToArray()); }
            catch (Exception error) { ScriptRunInfo.Clear(); Log($"EmpyrionScripting Mod: Clear => {error}", LogLevel.Error); }

            lock (ExecQueue)
            {
                WaitForExec.Clear();
                while (ExecQueue.TryDequeue(out _)) ;
            }
        }

        public static void Exec(ICollection<PlayfieldScriptData> values, int scriptsParallelExecution, int scriptsSyncExecution)
        {
            var syncExecCount         = 0;
            var scriptLoopTimeLimiter = Stopwatch.StartNew();

            try { values.ForEach(PF => PF.ScriptExecQueue.ExecNext(scriptsParallelExecution, scriptsSyncExecution, ref syncExecCount, scriptLoopTimeLimiter)); }
            catch (Exception error) { Log($"Game_Update: ScriptExecQueue.ExecNext: {error}", LogLevel.Error); }
        }
    }
}