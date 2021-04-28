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
        public long ScriptLoopSyncTimeLimiterMS { get; set; } = 200;
        public long ScriptLoopBackgroundTimeLimiterMS { get; set; } = 2000;
        public bool ScriptTrackingError { get; set; }
        public bool DetailedScriptsInfoData { get; set; }
        public int MaxStoredEventsPerSignal { get; set; } = 10;
        public Dictionary<int, int> DeconstructBlockSubstitution { get; set; } = new Dictionary<int, int>() { [331] = 0, [541] = 0, [542] = 0, [543] = 0, [544] = 0 };
        public Dictionary<string, string> Ids { get; set; } = new Dictionary<string, string>()
        {
            ["Ore"          ] = ",4296,4297,4298,4299,4300,4301,4302,4317,4318,4341,4345,4345,4359,4362,",
            ["Ingot"        ] = ",4319,4320,4321,4322,4323,4324,4325,4326,4327,4328,4329,4333,4342,4346,4366,",
            ["BlockL"       ] = ",343,396,399,402,403,404,405,408,411,412,462,545,839,1075,1128,1129,1238,1239,1322,1386,1392,1395,1440,1443,1481,1549,",
            ["BlockS"       ] = ",380,393,493,836,837,974,976,1135,1389,1478,1626,1960,",
            ["Medic"        ] = ",4403,4404,4423,4425,4430,4433,4407,4441,4464,4478,4483-4489,",
            ["Food"         ] = ",4373,4397,4402,4409,4410,4420,4422,4424,4426,4442,4457,4463,4465,4477,4479,4481,4482,4490,4458-4460,4467-4469,",
            ["Ingredient"   ] = ",4398,4399,4405,4407,4411,5312,4416,4429,4431,4436,4439,4440,4443,4448,4451-4453,4455,",
            ["Sprout"       ] = ",490,591,594,597,600,607,639,641,644,1167,1171,1175,1179,1367,1527,1531,1597,1601,",
            ["Tools"        ] = ",399,569,588,652,711,964,1485,4098,4107,4115,4117,4118,4119,4127,4128,4349,4797,4798,1108-1110,4135-4137,",
            ["ArmorMod"     ] = ",4696-4698,4701,4717-4724,",
            ["DeviceL"      ] = ",259,260,263,278,291,335,336,468,469,498,558,564,653,686,714,717,724,781,960,962,1002,1008,1016,1034,1035,1095,1111,1112,1132,1134,1136,1231,1257,1263,1278,1318,1370,1371,1372,1373,1377,1402,1436,1465,1466,1490,1494,1495,1500,1513,1571,1588,1627,1628,1682,1683,1689,1692,1706,1711,1808,1812,1813,1956,2032,2033,2034,420,457,772,778,835,934,1120,1253,1304,1321,1486,",
            ["DeviceS"      ] = ",272,279,418,419,422,423,456,470,471,536,546,547,548,558,653,694,695,696,697,698,1375,1380,1435,1437,1446,1500,1585,1591,1592,603,604,1107,1127,1130,1417,1447,1484,1575,1583,1584,1627,1774,1775,1776,1888,1956,2023,2024,2025,",
            ["WeaponPlayer" ] = ",4099,4100,4101,4102,4103,4105,4106,4107,4110,4111,4112,4113,4114,4115,4116,4117,4118,4119,4120,4121,4122,4123,4124,4125,4126,4128,4129,4130,4131,4132,",
            ["WeaponHV"     ] = ",429,669,683,1236,1659,1660,1661,1662,1663,1876,",
            ["WeaponSV"     ] = ",428,429,430,431,432,489,",
            ["WeaponCV"     ] = ",550,551,552,646,647,1582,1637,1638,1639,1640,1641,1673,",
            ["WeaponBA"     ] = ",1648,1649,1650,1651,1673,",
            ["AmmoPlayer"   ] = ",4147,4148,4149,4150,4151,4153,4154,4156,4157,4158,4160,4161,",
            ["AmmoHV"       ] = ",4152,4248,4256,4259,",
            ["AmmoSV"       ] = ",4152,4246,4247,4248,4250,4261,",
            ["AmmoCV"       ] = ",4150,4152,4249,4253,4254,4258,4262,4263,4267",
            ["AmmoBA"       ] = ",4150,4152,4249,4252,4257,4264,4265,4266,",
        };
        public Dictionary<StructureTankType, AllowedItem[]> StructureTank { get; set; } = new Dictionary<StructureTankType, AllowedItem[]>()
        {
            [StructureTankType.Oxygen  ] = new[] { new AllowedItem(4176, 250) },
            [StructureTankType.Fuel    ] = new[] { new AllowedItem(4421, 300), new AllowedItem(4335, 150), new AllowedItem(4314, 30) },
            [StructureTankType.Pentaxid] = new[] { new AllowedItem(4342, 1) }
        };
        public string NumberSpaceReplace { get; set; } = " "; // eigentlich :-( funktioniert aber leider nicht mehr "\u2007\u2009";
        public string BarStandardValueSign { get; set; } = "\u2588";
        public string BarStandardSpaceSign { get; set; } = "\u2591";
        public string[] ElevatedGroups { get; set; } = new[] {
                "Admin",
                "Zirax",
                "Predator",
                "Prey",
                "Talon",
                "Polaris",
                "Alien",
                "Pirates",
                "Kriel",
                "UCH",
                "Trader",
                "Civilian",
        };
    }

}
