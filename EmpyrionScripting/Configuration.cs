using Eleon.Modding;
using EmpyrionNetAPIDefinitions;
using EmpyrionScripting.Interface;
using Humanizer;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Collections.Generic;
using YamlDotNet.Core.Tokens;
using static Humanizer.In;

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
        public ExecMethod ExecMethod { get; set; } = ExecMethod.ThreadPool;
        public int ScriptsSyncExecution { get; set; } = 2;
        public int ScriptsParallelExecution { get; set; } = 4;
        public long ScriptLoopSyncTimeLimiterMS { get; set; } = 200;
        public long ScriptLoopBackgroundTimeLimiterMS { get; set; } = 2000;
        public bool ScriptTrackingError { get; set; }
        public bool DetailedScriptsInfoData { get; set; }
        public int MaxStoredEventsPerSignal { get; set; } = 10;
        public int ProcessMaxBlocksPerCycle { get; set; } = 200;
        public string OverrideScenarioPath { get; set; }
        public string[] AddOnAssemblies { get; set; } = new string[] { };

        public AllowedItem GardenerSalary { get; set; } = new AllowedItem(4344, 10); // Money Card

        public Dictionary<int, int> DeconstructBlockSubstitution { get; set; } = new Dictionary<int, int>() { [331] = 0, [541] = 0, [542] = 0, [543] = 0, [544] = 0 };
        public Dictionary<string, string> Ids { get; set; } = new Dictionary<string, string>()
        {
            ["Ore"         ] = ",AluminiumOre,CobaltOre,CopperOre,ErestrumOre,GoldOre,IronOre,MagnesiumOre,NeodymiumOre,PentaxidOre,PromethiumOre,SiliconOre,TitanOre,ZascosiumOre,",
            ["Ingot"       ] = ",CobaltIngot,CopperIngot,CrushedStone,ErestrumIngot,GoldIngot,IronIngot,MagnesiumPowder,NeodymiumIngot,PentaxidCrystal,PlatinBar,PromethiumPellets,RockDust,SathiumIngot,SiliconIngot,ZascosiumIngot,",
            ["BlockL"      ] = ",AlienBlocks,AlienLargeBlocks,ConcreteArmoredBlocks,ConcreteBlocks,ConcreteDestroyedBlocks,GrowingPot,GrowingPotConcrete,GrowingPotWood,HeavyWindowBlocks,HullArmoredLargeBlocks,HullCombatFullLarge,HullCombatLargeBlocks,HullFullLarge,HullLargeBlocks,HullLargeDestroyedBlocks,HullThinLarge,LadderBlocks,PlasticLargeBlocks,StairsBlocks,StairsBlocksConcrete,StairsBlocksWood,TrussLargeBlocks,WindowArmoredLargeBlocks,WindowLargeBlocks,WindowShutterLargeBlocks,WoodBlocks,",
            ["BlockS"      ] = ",ArtMassBlocks,HullArmoredSmallBlocks,HullSmallBlocks,HullSmallDestroyedBlocks,ModularWingBlocks,PlasticSmallBlocks,TrussSmallBlocks,VentilatorCubeQuarter,WindowArmoredSmallBlocks,WindowShutterSmallBlocks,WindowSmallBlocks,WingBlocks,",
            ["Medic"       ] = ",AlienParts03,AntibioticInjection,AntibioticPills,Medikit01,Medikit02,Medikit03,Medikit04,RadiationImmunityShot,RadiationPills,StomachPills,",
            ["Food"        ] = ",AkuaWine,AnniversaryCake,Beer,BerryJuice,Bread,Cheese,EmergencyRations,FruitJuice,FruitPie,HotBeverage,MeatBurger,Milk,Pizza,Sandwich,Steak,Stew,VegetableJuice,VeggieBurger,",
            ["Ingredient"  ] = ",AlienParts01,AlienParts03,AlienTooth,AloeVera,ConfettiMossScrapings,FireMossScrapings,Fruit,HerbalLeaves,Meat,PlantProtein,RottenFood,Spice,TrumpetGreens,Vegetables,",
            ["Sprout"      ] = ",AlienPalmTreeStage1,AlienPlantTube2Stage1,AlienplantWormStage1,BigFlowerStage1,BulbShroomYoungStage1,CobraLeavesPlantStage1,CoffeePlantStage1,CornStage1,DesertPlant20Stage1,DurianRoot,ElderberryStage1,InsanityPepperStage1,MushroomBellBrown01Stage1,PearthingStage1,PumpkinStage1,SnakeweedStage1,TomatoStage1,WheatStage1,",
            ["Tools"       ] = ",Chainsaw,ColorTool,ConcreteBlocks,ConstructorSurvival,DrillT2,Explosives,Flashlight,LightWork,LightWork02,MobileAirCon,MultiTool,MultiToolT2,OreScanner,OxygenGeneratorSmall,PlayerBike,RadarSuitT1,TextureTool,WaterGenerator,",
            ["ArmorMod"    ] = ",ArmorHeavyEpic,",
            ["DeviceL"     ] = ",AlienNPCBlocks,ArmorLocker,ATM,BlastDoorLargeBlocks,BoardingRampBlocks,CloneChamber,CockpitBlocksCV,ConstructorT0,ConstructorT1V2,ConstructorT2,ContainerAmmoControllerLarge,ContainerAmmoLarge,ContainerControllerLarge,ContainerExtensionLarge,ContainerLargeBlocks,ContainerPersonal,Core,CoreNoCPU,CPUExtenderBAT2,CPUExtenderBAT3,CPUExtenderBAT4,Deconstructor,DetectorCV,DoorArmoredBlocks,DoorBlocks,ElevatorMS,ExplosiveBlocks,ExplosiveBlocks2,Flare,FoodProcessorV2,ForcefieldEmitterBlocks,FridgeBlocks,FuelTankMSLarge,FuelTankMSLargeT2,FuelTankMSSmall,Furnace,GeneratorBA,GeneratorMS,GeneratorMST2,GravityGeneratorMS,HangarDoorBlocks,HumanNPCBlocks,LandClaimDevice,LandinggearBlocksCV,LCDScreenBlocks,LightLargeBlocks,LightPlant01,MedicalStationBlocks,OfflineProtector,OxygenStation,OxygenTankMS,OxygenTankSmallMS,PentaxidTank,Portal,RampLargeBlocks,RCSBlockMS,RCSBlockMS_T2,RemoteConnection,RepairBayBA,RepairBayBAT2,RepairBayConsole,RepairBayCVT2,RepairStation,SensorTriggerBlocks,ShieldGeneratorBA,ShieldGeneratorBAT2,ShieldGeneratorPOI,ShutterDoorLargeBlocks,SolarGenerator,SolarPanelBlocks,SolarPanelSmallBlocks,SpotlightBlocks,TeleporterBA,ThrusterMSDirectional,ThrusterMSRound2x2Blocks,ThrusterMSRound3x3Blocks,ThrusterMSRoundBlocks,VentilatorBlocks,",
            ["DeviceS"     ] = ",ArmorLockerSV,CloneChamberHV,ConstructorHV,ConstructorSV,Core,CPUExtenderHVT2,CPUExtenderHVT3,CPUExtenderHVT4,DetectorHVT1,DoorBlocksSV,Flare,ForcefieldEmitterBlocks,FridgeSV,FuelTankSV,FuelTankSVSmall,GeneratorSV,GeneratorSVSmall,HoverBooster,HoverEngineLarge,HoverEngineSmall,HoverEngineThruster,LightSS01,MedicStationHV,OxygenTankSV,PentaxidTankSV,RCSBlockGV,RCSBlockSV,RemoteConnection,ShieldGeneratorHV,ThrusterGVJetRound1x3x1,ThrusterGVRoundBlocks,ThrusterGVRoundLarge,ThrusterGVRoundLargeT2,ThrusterGVRoundNormalT2,ThrusterJetRound1x3x1,ThrusterJetRound2x5x2,ThrusterJetRound2x5x2V2,ThrusterJetRound3x10x3,ThrusterJetRound3x10x3V2,ThrusterJetRound3x13x3,ThrusterJetRound3x13x3V2,ThrusterJetRound3x7x3,ThrusterSVDirectional,ThrusterSVRoundBlocks,ThrusterSVRoundLarge,ThrusterSVRoundLargeT2,ThrusterSVRoundNormalT2,VentilatorBlocks,WarpDriveSV,",
            ["WeaponPlayer"] = ",AssaultRifle,AssaultRifleEpic,AssaultRifleT2,Chainsaw,ColorTool,DrillT2,Explosives,LaserPistol,LaserPistolT2,LaserRifle,LaserRifleEpic,Minigun,MinigunEpic,MultiTool,Pistol,PistolEpic,PistolT2,PulseRifle,RocketLauncher,RocketLauncherEpic,RocketLauncherT2,ScifiCannon,ScifiCannonEpic,Shotgun,Shotgun2,Shotgun2Epic,Sniper,Sniper2,Sniper2Epic,TextureTool,",
            ["WeaponHV"    ] = ",DrillAttachment,DrillAttachmentLarge,DrillAttachmentT2,SawAttachment,TurretGVArtilleryBlocks,TurretGVMinigunBlocks,TurretGVPlasmaBlocks,TurretGVRocketBlocks,TurretGVToolBlocks,WeaponSV02,",
            ["WeaponSV"    ] = ",WeaponSV01,WeaponSV02,WeaponSV03,WeaponSV04,WeaponSV05,WeaponSV05Homing,",
            ["WeaponCV"    ] = ",DrillAttachmentCV,SentryGunBlocks,TurretMSArtilleryBlocks,TurretMSLaserBlocks,TurretMSProjectileBlocks,TurretMSRocketBlocks,TurretMSToolBlocks,TurretZiraxMSLaser,TurretZiraxMSPlasma,TurretZiraxMSRocket,WeaponMS01,WeaponMS02,",
            ["WeaponBA"    ] = ",SentryGunBlocks,TurretBaseArtilleryBlocks,TurretBaseLaserBlocks,TurretBaseProjectileBlocks,TurretBaseRocketBlocks,",
            ["AmmoPlayer"  ] = ",12.7mmBullet,5.8mmBullet,50Caliber,8.3mmBullet,DrillCharge,MultiCharge,PulseLaserChargePistol,PulseLaserChargeRifle,SciFiCannonPlasmaCharge,ShotgunShells,SlowRocket,SlowRocketHoming,",
            ["AmmoHV"      ] = ",15mmBullet,ArtilleryRocket,FastRocket,TurretGVPlasmaCharge,",
            ["AmmoSV"      ] = ",15mmBullet,FastRocket,FastRocketHoming,PlasmaCannonChargeSS,PulseLaserChargeSS,RailgunBullet,",
            ["AmmoCV"      ] = ",15mmBullet,30mmBullet,5.8mmBullet,FastRocketMS,FlakRocketMS,LargeRocketMS,PulseLaserChargeMS,PulseLaserChargeMSWeapon,TurretMSPlasmaCharge,",
            ["AmmoBA"      ] = ",15mmBullet,30mmBullet,5.8mmBullet,FastRocketBA,FlakRocket,LargeRocket,PulseLaserChargeBA,TurretBAPlasmaCharge,",
            ["Gardeners"   ] = ",ConsoleSmallHuman,"
        };

        [JsonIgnore]
        public Dictionary<string, string> MappedIds { get; set; }

        [JsonIgnore]
        public Dictionary<string, string> NamedIds { get; set; }

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

        public AllDBQueries DBQueries { get; set; } = new AllDBQueries {
            Elevated = new Dictionary<string, DBQuery> {
                {"Entities", new DBQuery {CacheQueryForSeconds = 300, Description = "Provides the data of the structures", Query =
@"
SELECT * FROM Structures 
JOIN Entities ON Structures.entityid = Entities.entityid
JOIN Playfields ON Entities.pfid = Playfields.pfid
JOIN SolarSystems ON SolarSystems.ssid = Playfields.ssid
WHERE isremoved = 0 {additionalWhereAnd}
"
                    }
                },
            },

            Player = new Dictionary<string, DBQuery> {
                {"Entities", new DBQuery {CacheQueryForSeconds = 300, Description = "Provides the data of the structures of the player and the faction", Query =
@"
SELECT * FROM Structures 
JOIN Entities ON Structures.entityid = Entities.entityid
JOIN Playfields ON Entities.pfid = Playfields.pfid
JOIN SolarSystems ON SolarSystems.ssid = Playfields.ssid
WHERE (isremoved = 0 AND ((facgroup = 1 AND facid = @FactionId) OR (facgroup = 1 AND facid = @PlayerId) OR (facgroup = 0 AND facid = @FactionId))) {additionalWhereAnd}
"
                    }
                },

                {"DiscoveredPOIs", new DBQuery {CacheQueryForSeconds = 600, Description = "Provides the data of the DiscoveredPOIs of the player and the faction", Query =
@"
SELECT * FROM DiscoveredPOIs
JOIN Entities ON DiscoveredPOIs.poiid = Entities.entityid
JOIN Playfields ON Entities.pfid = Playfields.pfid
JOIN SolarSystems ON SolarSystems.ssid = Playfields.ssid
WHERE (Entities.isremoved = 0 AND ((DiscoveredPOIs.facgroup = 1 AND DiscoveredPOIs.facid = 100) OR (DiscoveredPOIs.facgroup = 0 AND DiscoveredPOIs.facid = 100))) {additionalWhereAnd}
"
                    }
                },

                {"TerrainPlaceables", new DBQuery {CacheQueryForSeconds = 600, Description = "Provides the data of the TerrainPlaceables of the player and the faction", Query =
@"
SELECT * FROM TerrainPlaceables 
JOIN Entities ON TerrainPlaceables.entityid = Entities.entityid
JOIN Playfields ON TerrainPlaceables.pfid = Playfields.pfid
JOIN SolarSystems ON SolarSystems.ssid = Playfields.ssid
WHERE (isremoved = 0 AND ((facgroup = 1 AND facid = @FactionId) OR (facgroup = 1 AND facid = @PlayerId) OR (facgroup = 0 AND facid = @FactionId))) {additionalWhereAnd}
"
                    }
                },

                {"Playfields", new DBQuery {CacheQueryForSeconds = 300, Description = "Provides the data of the discovered Playfields of the player and the faction", Query =
@"
SELECT * FROM Playfields
LEFT JOIN DiscoveredPlayfields ON DiscoveredPlayfields.pfid = playfields.pfid
JOIN SolarSystems ON SolarSystems.ssid = Playfields.ssid
WHERE playfields.ssid IN (
SELECT ssid FROM Playfields
LEFT JOIN DiscoveredPlayfields ON DiscoveredPlayfields.pfid = playfields.pfid
WHERE (DiscoveredPlayfields.facgroup = 0 AND DiscoveredPlayfields.facid = @FactionId) OR (DiscoveredPlayfields.facgroup = 1 AND DiscoveredPlayfields.facid = @PlayerId)
GROUP BY playfields.ssid
) {additionalWhereAnd}
"
                    }
                },

                {"PlayfieldResources", new DBQuery {CacheQueryForSeconds = 600, Description = "Provides the PlayfieldResources of the discovered Playfields of the player and the faction", Query =
@"
SELECT * FROM Playfields
LEFT JOIN DiscoveredPlayfields ON DiscoveredPlayfields.pfid = playfields.pfid
JOIN SolarSystems ON SolarSystems.ssid = Playfields.ssid
JOIN PlayfieldResources ON PlayfieldResources.pfid = Playfields.pfid
WHERE playfields.ssid IN (
SELECT ssid FROM Playfields
LEFT JOIN DiscoveredPlayfields ON DiscoveredPlayfields.pfid = playfields.pfid
WHERE (DiscoveredPlayfields.facgroup = 0 AND DiscoveredPlayfields.facid = @FactionId) OR (DiscoveredPlayfields.facgroup = 1 AND DiscoveredPlayfields.facid = @PlayerId)
GROUP BY playfields.ssid
) {additionalWhereAnd}
"
                    }
                },

                {"PlayerData", new DBQuery {CacheQueryForSeconds = 30, Description = "Provides the PlayerData of the player and the faction", Query =
@"
SELECT * FROM PlayerData 
JOIN Entities ON Entities.entityid = PlayerData.entityid
JOIN Playfields ON Playfields.pfid = PlayerData.pfid
JOIN SolarSystems ON SolarSystems.ssid = Playfields.ssid
WHERE ((Entities.facgroup = 1 AND Entities.facid = @FactionId) OR (Entities.facgroup = 1 AND Entities.facid = @PlayerId) OR (Entities.facgroup = 0 AND Entities.facid = @FactionId)) {additionalWhereAnd}
"
                    }
                },

            }
        };
    }

    public class AllDBQueries
    {
        public Dictionary<string, DBQuery> Elevated { get; set; }
        public Dictionary<string, DBQuery> Player { get; set; }
    }

    public class DBQuery
    {
        public string Description { get; set; }
        public int CacheQueryForSeconds { get; set; }
        public string Query { get; set; }
    }
}
