using EmpyrionNetAPIDefinitions;
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

    public enum CsModPermission
    {
        Player,
        Admin,
        SaveGame,
    }

    public enum ExecMethod
    {
        None,
        ThreadPool,
        Direct,
        BackgroundWorker
    }

    public class Configuration
    {
        [JsonConverter(typeof(StringEnumConverter))]
        public LogLevel LogLevel { get; set; } = LogLevel.Message;
        [JsonConverter(typeof(StringEnumConverter))]
        public CsModPermission CsScriptsAllowedFor { get; set; } = CsModPermission.Player;
        public int InGameScriptsIntervallMS { get; set; } = 1000;
        public int DeviceLockOnlyAllowedEveryXCycles { get; set; } = 10;
        public int SaveGameScriptsIntervallMS { get; set; } = 10000;
        public bool ScriptTracking { get; set; }
        public float EntityAccessMaxDistance { get; set; } = 500;
        public int DelayStartForNSecondsOnPlayfieldLoad { get; set; } = 30;
        [JsonConverter(typeof(StringEnumConverter))]
        public ExecMethod ExecMethod { get; set; } = ExecMethod.BackgroundWorker;
        public int ScriptsSyncExecution { get; set; } = 2;
        public int ScriptsParallelExecution { get; set; } = 10;
        public bool ScriptTrackingError { get; set; }
        public int MaxStoredEventsPerSignal { get; set; } = 10;
        public Dictionary<int, int> DeconstructBlockSubstitution { get; set; } = new Dictionary<int, int>() { [331] = 0, [541] = 0, [542] = 0, [543] = 0, [544] = 0 };
        public Dictionary<string, string> Ids { get; set; } = new Dictionary<string, string>()
        {
            ["Ore"          ]   = "2248,2249,2250,2251,2252,2253,2254,2269,2270,2284,2293,2297,2311,2314",
            ["Ingot"        ] = "2271,2272,2273,2274,2275,2276,2277,2278,2279,2280,2281,2285,2294,2298,2318",
            ["BlockL"       ] = "343,396,399,402,405,408,411,462,545,839,1075,1128,1129,1238,1239,1322,1386,1392,1395,1440,1443,1481,1549",
            ["BlockS"       ] = "380,393,493,836,837,974,976,1135,1389,1478,1626,1960",
            ["Medic"        ] = "2355,2356,2375,2377,2382,2385,2359,2393,2416,2426-2428,2430,2435-2441",
            ["Food"         ] = "2325,2349,2354,2361,2362,2372,2374,2376,2378,2394,2409,2410-2412,2415,2417,2419-2421,2423-2425,2429,2431,2433,2434,2442",
            ["Ingredient"   ] = "2350,2351,2357,2359,2363,3264,2368,2381,2383,2388,2391,2392,2395,2400,2403-2405,2407",
            ["Sprout"       ] = "490,591,594,597,600,607,639,641,644,1167,1171,1175,1179,1367,1527,1531,1597,1601",
            ["Tools"        ] = "399,569,588,652,711,964,1108-1110,1485,2050,2059,2067,2069,2070,2071,2079,2080,2087-2089,2301,2749,2750",
            ["ArmorMod"     ] = "2648-2650,2653,2669-2676",
        };
        public Dictionary<StructureTankType, AllowedItem[]> StructureTank { get; set; } = new Dictionary<StructureTankType, AllowedItem[]>()
        {
            [StructureTankType.Oxygen  ] = new[] { new AllowedItem(2128, 250) },
            [StructureTankType.Fuel    ] = new[] { new AllowedItem(2373, 300), new AllowedItem(2287, 150), new AllowedItem(2266, 30) },
            [StructureTankType.Pentaxid] = new[] { new AllowedItem(2294, 1) }
        };
        public string NumberSpaceReplace { get; set; } = " "; // eigentlich :-( funktioniert aber leider nicht mehr "\u2007\u2009";
        public string BarStandardValueSign { get; set; } = "\u2588";
        public string BarStandardSpaceSign { get; set; } = "\u2591";
    }

}
