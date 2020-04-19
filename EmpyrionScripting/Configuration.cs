using EmpyrionNetAPIDefinitions;
using EmpyrionScripting.DataWrapper;
using EmpyrionScripting.Interface;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Collections.Generic;

namespace EmpyrionScripting
{
    public class AllowedItem
    {
        public AllowedItem(int itemId, int amount)
        {
            ItemId = itemId;
            Amount = amount;
        }

        public int ItemId { get; set; }
        public int Amount { get; set; }
    }

    public enum CsScriptsAllowed
    {
        SaveGameScripts = 0,
        AdminStructures = 1,
        Everywhere      = 2,
    }

    public class Configuration
    {
        [JsonConverter(typeof(StringEnumConverter))]
        public LogLevel LogLevel { get; set; } = LogLevel.Message;
        [JsonConverter(typeof(StringEnumConverter))]
        public CsScriptsAllowed CsScriptsAllowed { get; set; } = CsScriptsAllowed.AdminStructures;
        public int InGameScriptsIntervallMS { get; set; } = 1000;
        public int DeviceLockOnlyAllowedEveryXCycles { get; set; } = 10;
        public int SaveGameScriptsIntervallMS { get; set; } = 10000;
        public bool ScriptTracking { get; set; }
        public float EntityAccessMaxDistance { get; set; } = 500;
        public int DelayStartForNSecondsOnPlayfieldLoad { get; set; } = 30;
        public int ScriptsParallelExecution { get; set; } = 2;
        public bool ScriptTrackingError { get; set; }
        public int MaxStoredEventsPerSignal { get; set; } = 10;
        public Dictionary<int, int> DeconstructBlockSubstitution { get; set; } = new Dictionary<int, int>() { [331] = 0, [541] = 0, [542] = 0, [543] = 0, [544] = 0 };       
        public Dictionary<StructureTankType, AllowedItem[]> StructureTank { get; set; } = new Dictionary<StructureTankType, AllowedItem[]>()
        {
            [StructureTankType.Oxygen  ] = new[] { new AllowedItem(2128, 250) },
            [StructureTankType.Fuel    ] = new[] { new AllowedItem(2373, 300), new AllowedItem(2287, 150), new AllowedItem(2266, 30) },
            [StructureTankType.Pentaxid] = new[] { new AllowedItem(2294, 1), new AllowedItem(2293, 2) }
        };
        public string[] CsUsings { get; set; } = new[] { "System", "System.Text", "System.Linq" };
        public string[] CsAssemblyReferences { get; set; } = new[] { "System.Core", "System.Numerics" };
    }

}
