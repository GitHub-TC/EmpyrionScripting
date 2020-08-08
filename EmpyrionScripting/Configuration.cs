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
        public bool DetailedScriptsInfoData { get; set; }
        public int MaxStoredEventsPerSignal { get; set; } = 10;
        public Dictionary<int, int> DeconstructBlockSubstitution { get; set; } = new Dictionary<int, int>() { [331] = 0, [541] = 0, [542] = 0, [543] = 0, [544] = 0 };
        public Dictionary<string, string> Ids { get; set; } = new Dictionary<string, string>()
        {
            ["Ore"          ] = ",2248,2249,2250,2251,2252,2253,2254,2269,2270,2284,2293,2297,2311,2314,",
            ["Ingot"        ] = ",2271,2272,2273,2274,2275,2276,2277,2278,2279,2280,2281,2285,2294,2298,2318,",
            ["BlockL"       ] = ",343,396,399,402,403,404,405,408,411,412,462,545,839,1075,1128,1129,1238,1239,1322,1386,1392,1395,1440,1443,1481,1549,",
            ["BlockS"       ] = ",380,393,493,836,837,974,976,1135,1389,1478,1626,1960,",
            ["Medic"        ] = ",2355,2356,2375,2377,2382,2385,2359,2393,2416,2426-2428,2430,2435-2441,",
            ["Food"         ] = ",2325,2349,2354,2361,2362,2372,2374,2376,2378,2394,2409,2410-2412,2415,2417,2419-2421,2423-2425,2429,2431,2433,2434,2442,",
            ["Ingredient"   ] = ",2350,2351,2357,2359,2363,3264,2368,2381,2383,2388,2391,2392,2395,2400,2403-2405,2407,",
            ["Sprout"       ] = ",490,591,594,597,600,607,639,641,644,1167,1171,1175,1179,1367,1527,1531,1597,1601,",
            ["Tools"        ] = ",399,569,588,652,711,964,1108-1110,1485,2050,2059,2067,2069,2070,2071,2079,2080,2087-2089,2301,2749,2750,",
            ["ArmorMod"     ] = ",2648-2650,2653,2669-2676,",
            ["DeviceL"      ] = ",259,260,263,278,291,335,336,468,469,498,558,564,653,686,714,717,724,781,960,962,1002,1008,1016,1034,1035,1095,1111,1112,1132,1134,1136,1231,1257,1263,1278,1318,1370,1371,1372,1373,1377,1402,1436,1465,1466,1490,1494,1495,1500,1513,1571,1588,1627,1628,1682,1683,1689,1692,1706,1711,1808,1812,1813,1956,2032,2033,2034,420,457,772,778,835,934,1120,1253,1304,1321,1486,",
            ["DeviceS"      ] = ",272,279,418,419,422,423,456,470,471,536,546,547,548,558,653,694,695,696,697,698,1375,1380,1435,1437,1446,1500,1585,1591,1592,603,604,1107,1127,1130,1417,1447,1484,1575,1583,1584,1627,1774,1775,1776,1888,1956,2023,2024,2025,",
            ["WeaponPlayer" ] = ",2051,2052,2053,2054,2055,2057,2058,2059,2062,2063,2064,2065,2066,2067,2068,2069,2070,2071,2072,2073,2074,2075,2076,2077,2078,2080,2081,2082,2083,2084,",
            ["WeaponHV"     ] = ",429,669,683,1236,1659,1660,1661,1662,1663,1876,",
            ["WeaponSV"     ] = ",428,429,430,431,432,489,",
            ["WeaponCV"     ] = ",550,551,552,646,647,1582,1637,1638,1639,1640,1641,1673,",
            ["WeaponBA"     ] = ",1648,1649,1650,1651,1673,",
            ["AmmoPlayer"   ] = ",2099,2100,2101,2102,2103,2105,2106,2108,2109,2110,2112,2113,",
            ["AmmoHV"       ] = ",2104,2200,2208,2211,",
            ["AmmoSV"       ] = ",2104,2198,2199,2200,2202,2213,",
            ["AmmoCV"       ] = ",2102,2104,2201,2205,2206,2210,2214,2215,2219",
            ["AmmoBA"       ] = ",2102,2104,2201,2204,2209,2216,2217,2218,",
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
