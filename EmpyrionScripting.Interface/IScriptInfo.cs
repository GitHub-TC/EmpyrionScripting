using System;

namespace EmpyrionScripting.Interface
{
    public interface IScriptInfo
    {
        int Count { get; }
        int EntityId { get; }
        TimeSpan ExecTime { get; }
        DateTime LastStart { get; }
        int ScriptPriority { get; }
        int RunningInstances { get; }
        string ScriptId { get; set; }
        int TimeLimitReached { get; }
        bool NeedsMainThread { get; }
        bool IsElevatedScript { get; }
        ScriptLanguage ScriptLanguage { get; }
        bool NeedsDeviceLock { get; }
        int TimeCriticalScriptExecutions { get; }
    }
}