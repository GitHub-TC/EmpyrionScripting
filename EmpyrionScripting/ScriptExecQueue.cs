using EmpyrionNetAPIDefinitions;
using EmpyrionScripting.DataWrapper;
using EmpyrionScripting.Interface;
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
        public ConcurrentQueue<string>                       ExecQueue { get; private set; } = new ConcurrentQueue<string>();

        static public Action<string, LogLevel> Log { get; set; }
        public int ScriptsCount { get; set; }
        public int QueueCount => ExecQueue.Count;

        private BackgroundWorker Worker { get; }
        public ConcurrentQueue<string> BackgroundExecQueue { get; private set; } = new ConcurrentQueue<string>();

        public ConcurrentDictionary<string, bool> ScriptNeedsMainThread { get; set; } = new ConcurrentDictionary<string, bool>();
        public int GameUpdateScriptLoopTimeLimitReached { get; private set; }
        public static Stopwatch ScriptLoopTimeLimiter { get; } = new Stopwatch();
        public ConcurrentBag<string> BackgroundWorkerToDo { get; set; } = new ConcurrentBag<string>();
        public Func<bool> TimeLimitSyncReached { get; private set; }

        public ScriptExecQueue(Action<IScriptRootData> processScript)
        {
            ProcessScript           = processScript;
            Worker                  = new BackgroundWorker();
            Worker.DoWork           += (s, e) => {
                while (BackgroundExecQueue.TryDequeue(out var data)) BackgroundWorkerToDo.Add(data);
                Parallel.ForEach(BackgroundWorkerToDo, ExecScriptWithMainThreadCheck);
                BackgroundWorkerToDo = new ConcurrentBag<string>();
            };
        }

        public void Add(IScriptRootData data)
        {
            if (ScriptNeedsMainThread.TryGetValue(data.ScriptId, out var needMainThread)) data.ScriptNeedsMainThread = needMainThread;
            else                                                                          ScriptNeedsMainThread.TryAdd(data.ScriptId, false);

            if (WaitForExec.TryAdd(data.ScriptId, data)) ExecQueue.Enqueue(data.ScriptId);
            else                                         WaitForExec.AddOrUpdate(data.ScriptId, data, (scriptId, oldData) => data);
        }

        public void CheckForEmergencyRestart(PlayfieldScriptData playfield)
        {
            lock (ExecQueue)
            {
                if (ExecQueue.IsEmpty && WaitForExec.Count > 0 && BackgroundWorkerToDo.Count == 0)
                {
                    Log($"ExecQueue.IsEmpty restart [{playfield.PlayfieldName}]... #{WaitForExec.Count} => {WaitForExec.Aggregate("Pendingscripts:", (E, L) => E + "\n" + L.Key)}", LogLevel.Message);
                    ThreadPool.GetAvailableThreads(out var availableWorkerThreads, out var availableCompletionPortThreads);
                    Log($"ThreadPool available: WorkerThreads:{availableWorkerThreads} CompletionPortThreads:{availableCompletionPortThreads}", LogLevel.Message);
                    ThreadPool.GetMaxThreads(out var maxWorkerThreads, out var maxCompletionPortThreads);
                    Log($"ThreadPool max: WorkerThreads:{maxWorkerThreads} CompletionPortThreads:{maxCompletionPortThreads}", LogLevel.Message);
                    ThreadPool.GetMinThreads(out var minWorkerThreads, out var minCompletionPortThreads);
                    Log($"ThreadPool min: WorkerThreads:{minWorkerThreads} CompletionPortThreads:{minCompletionPortThreads}", LogLevel.Message);
                    WaitForExec.Clear(); // robust error restart with fresh data
                }
            }
        }

        public void ExecNext(int maxCount, int scriptsSyncExecution, ref int syncExecCount, Stopwatch scriptLoopTimeLimiter)
        {
            if (BackgroundWorkerToDo.Count > maxCount / 2) return;

            var scriptLoopTimeLimiterStopwatch = scriptLoopTimeLimiter;
            var timeLimitBackgroundReached = new Func<bool>(() => scriptLoopTimeLimiterStopwatch.ElapsedMilliseconds > EmpyrionScripting.Configuration.Current.ScriptLoopBackgroundTimeLimiterMS);
            TimeLimitSyncReached           = new Func<bool>(() => scriptLoopTimeLimiterStopwatch.ElapsedMilliseconds > EmpyrionScripting.Configuration.Current.ScriptLoopSyncTimeLimiterMS);

            Log($"ExecNext: {WaitForExec.Count} -> {ExecQueue.Count}", LogLevel.Debug);

            for (int i = maxCount - 1; i >= 0; i--)
            {
                if (ExecQueue.TryDequeue(out var scriptId) && WaitForExec.TryGetValue(scriptId, out var data))
                {
                    if (data.ScriptNeedsMainThread)
                    {
                        Interlocked.Increment(ref syncExecCount);

                        if (syncExecCount > scriptsSyncExecution) ExecQueue.Enqueue(scriptId);
                        else
                        {
                            ((ScriptRootData)data).ScriptLoopTimeLimitReached = TimeLimitSyncReached;
                            ExecNext(data);
                        }
                    }
                    else
                    {
                        ((ScriptRootData)data).ScriptLoopTimeLimitReached = timeLimitBackgroundReached;
                        ExecNext(data);
                    }
                }
                else
                {
                    Log($"ExecQueue.TryPeek: {WaitForExec.Count} -> {ExecQueue.Count} => {i}", LogLevel.Debug);
                    return;
                }

                if (TimeLimitSyncReached())
                {
                    GameUpdateScriptLoopTimeLimitReached++;
                    Log($"ScriptLoopTimeLimitReached: {scriptLoopTimeLimiter.ElapsedMilliseconds}", LogLevel.Debug);
                    return;
                }
            }
        }

        private void ExecNext(IScriptRootData data)
        {
            ((PlayfieldScriptData)data.GetPlayfieldScriptData()).IncrementCycleCounter(data.ScriptId);

            _ = EmpyrionScripting.Configuration.Current.ExecMethod switch
            {
                ExecMethod.ThreadPool       => data.ScriptNeedsMainThread ? DirectExecNext(data) : ThreadPoolExecNext      (data.ScriptId),
                ExecMethod.BackgroundWorker => data.ScriptNeedsMainThread ? DirectExecNext(data) : BackgroundWorkerExecNext(data.ScriptId),
                ExecMethod.Direct           => DirectExecNext(data),
                _                           => false,
            };
        }

        public bool BackgroundWorkerExecNext(string scriptId)
        {
            BackgroundExecQueue.Enqueue(scriptId);
            if (!Worker.IsBusy) Worker.RunWorkerAsync();
            return true;
        }

        public bool ThreadPoolExecNext(string scriptId)
        {
            if (!ThreadPool.QueueUserWorkItem(ExecScriptWithMainThreadCheck, scriptId))
            {
                Log($"EmpyrionScripting Mod: ExecNext NorThreadPoolFree {scriptId}", LogLevel.Debug);
                return false;
            }
            return true;
        }

        public bool DirectExecNext(IScriptRootData data)
        {
            data.ScriptWithinMainThread = true;
            ExecScriptAndRemoveFromList(data);
            return true;
        }

        private void ExecScriptWithMainThreadCheck(object state)
        {
            if (!WaitForExec.TryGetValue(state?.ToString(), out var data)) return;
            ExecScriptAndRemoveFromList(data);
            if (data.ScriptNeedsMainThread) Add(data);
        }

        private void ExecScriptAndRemoveFromList(IScriptRootData data)
        {
            try{ ExecScript(data); }
            catch (Exception error) { Log($"ExecScriptAndRemoveFromList: {data.ScriptId}:{error}", LogLevel.Debug); }
            finally { WaitForExec.TryRemove(data.ScriptId, out _); }
        }

        private void ExecScript(IScriptRootData data)
        {
            if (data.E.EntityType == EntityType.Proxy || data.E.EntityType == EntityType.Unknown) return;

            if (!ScriptRunInfo.TryGetValue(data.ScriptId, out var info)) info = new ScriptInfo() {
                ScriptLanguage = data.ScriptLanguage,
                ScriptPriority = data.ScriptPriority,
                ScriptId       = data.ScriptId, 
                EntityId       = data.E.Id 
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

        public static void Exec(IEnumerable<PlayfieldScriptData> values, int scriptsParallelExecution, int scriptsSyncExecution)
        {
            var syncExecCount         = 0;
            ScriptLoopTimeLimiter.Restart();

            try { values.ForEach(PF => PF.ScriptExecQueue.ExecNext(scriptsParallelExecution, scriptsSyncExecution, ref syncExecCount, ScriptLoopTimeLimiter)); }
            catch (Exception error) { Log($"Game_Update: ScriptExecQueue.ExecNext: {error}", LogLevel.Error); }
        }
    }
}