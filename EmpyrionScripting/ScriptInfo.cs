using EmpyrionScripting.Interface;
using System;

namespace EmpyrionScripting
{

    public class ScriptInfo : IScriptInfo
    {
        public int Count { get; set; }
        public DateTime LastStart { get; set; }
        public TimeSpan ExecTime { get; set; }
        public string ScriptId { get; set; }
        public int EntityId { get; set; }
        public int ScriptPriority { get; set; }
        public bool NeedsMainThread { get; set; }
        public bool IsElevatedScript { get; set; }

        public int RunningInstances => _RunningInstances;
        public int _RunningInstances;

        public int TimeLimitReached => _TimeLimitReached;

        public int _TimeLimitReached;
    }
}