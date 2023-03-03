using Eleon.Modding;
using EmpyrionNetAPIDefinitions;
using EmpyrionScripting.CustomHelpers;
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
        public Dictionary<int, int> DeconstructBlockSubstitution { get; set; } = new Dictionary<int, int>() { [331] = 0, [541] = 0, [542] = 0, [543] = 0, [544] = 0, [1254] = 0 };

        const string RemoveBlocksId = "RemoveBlocks";

        public Dictionary<string, string> Ids { get; set; } = new Dictionary<string, string>()
        {
            ["Ore"]                  = ",AluminiumOre,CobaltOre,CopperOre,ErestrumOre,GoldOre,IronOre,MagnesiumOre,NeodymiumOre,PentaxidOre,PromethiumOre,SiliconOre,TitanOre,ZascosiumOre,",
            ["Ingot"]                = ",CobaltIngot,CopperIngot,CrushedStone,ErestrumIngot,GoldIngot,IronIngot,MagnesiumPowder,NeodymiumIngot,PentaxidCrystal,PlatinBar,PromethiumPellets,RockDust,SathiumIngot,SiliconIngot,ZascosiumIngot,",
            ["BlockL"]               = ",AlienBlocks,AlienLargeBlocks,ConcreteArmoredBlocks,ConcreteBlocks,ConcreteDestroyedBlocks,GrowingPot,GrowingPotConcrete,GrowingPotWood,HeavyWindowBlocks,HullArmoredLargeBlocks,HullCombatFullLarge,HullCombatLargeBlocks,HullFullLarge,HullLargeBlocks,HullLargeDestroyedBlocks,HullThinLarge,LadderBlocks,PlasticLargeBlocks,StairsBlocks,StairsBlocksConcrete,StairsBlocksWood,TrussLargeBlocks,WindowArmoredLargeBlocks,WindowLargeBlocks,WindowShutterLargeBlocks,WoodBlocks,HeavyWindowDetailedBlocks,SteelRampBlocksL,HardenedRampBlocksL,CombatRampBlocksL,PassengerSeatMS,WalkwayLargeBlocks,",
            ["BlockS"]               = ",ArtMassBlocks,HullArmoredSmallBlocks,HullSmallBlocks,HullSmallDestroyedBlocks,ModularWingBlocks,PlasticSmallBlocks,TrussSmallBlocks,VentilatorCubeQuarter,WindowArmoredSmallBlocks,WindowShutterSmallBlocks,WindowSmallBlocks,WingBlocks,HullCombatSmallBlocks,WalkwaySmallBlocks,HeavyWindowBlocksSmall,SteelRampBlocksS,HardenedRampBlocksS,CombatRampBlocksS,",
            ["Medic"]                = ",AlienParts03,AntibioticInjection,AntibioticPills,Medikit01,Medikit02,Medikit03,Medikit04,RadiationImmunityShot,RadiationPills,StomachPills,Bandages,EnergyPills,AntibioticOintment,AdrenalineShot,AntiRadiationOintment,AntiToxicOintment,AntiToxicPills,AntiParasitePills,AntiToxicInjection,AntiParasiteInjection,AntiRadiationInjection,EnergyDrink,AblativeSpray,BugSpray,Medikit05,Eden_EmergencyLifeSupport,Eden_RegenKit,Eden_StaminaRegenKit,Eden_ImmunityShield,Eden_RegenKitT2,Eden_StaminaRegenKitT2,Eden_RadiationRegenKit,Eden_Implant1,Eden_Implant2,Eden_Implant3,Eden_Implant4,Eden_Implant5,Eden_Implant6,Eden_BandagesT2,",
            ["Food"]                 = ",AkuaWine,AnniversaryCake,Beer,BerryJuice,Bread,Cheese,EmergencyRations,FruitJuice,FruitPie,HotBeverage,MeatBurger,Milk,Pizza,Sandwich,Steak,Stew,VegetableJuice,VeggieBurger,",
            ["Ingredient"]           = ",5312,AlienParts01,AlienParts02,AlienParts03,AlienThorn,AlienTooth,AloeVera,BerryJuice,Cheese,ConfettiMossScrapings,Eden_SilverIngot,Egg,ErestrumGel,Fiber,FireMossScrapings,FishMeat,Flour,Fruit,FruitJuice,Ham,HerbalLeaves,HWSFish,Meat,Milk,NCPowder,NutrientSolution,PentaxidElement,PlantProtein,PlasticMaterial,PlatinOunce,PromethiumPellets,Ratatouille,RockDust,RottenFood,Salami,Spice,TrumpetGreens,VegetableJuice,Vegetables,WaterBottle,XenoSubstrate,",
            ["Sprout"]               = ",AlienPalmTreeStage1,AlienPlantTube2Stage1,AlienplantWormStage1,BigFlowerStage1,BulbShroomYoungStage1,CobraLeavesPlantStage1,CoffeePlantStage1,CornStage1,DesertPlant20Stage1,DurianRoot,ElderberryStage1,InsanityPepperStage1,MushroomBellBrown01Stage1,PearthingStage1,PumpkinStage1,SnakeweedStage1,TomatoStage1,WheatStage1,",
            ["Tools"]                = ",Chainsaw,ColorTool,ConcreteBlocks,ConstructorSurvival,DrillT2,Explosives,Flashlight,LightWork,LightWork02,MobileAirCon,MultiTool,MultiToolT2,OreScanner,OxygenGeneratorSmall,PlayerBike,RadarSuitT1,TextureTool,WaterGenerator,AutoMiningDeviceT1,AutoMiningDeviceT2,AutoMiningDeviceT3,Eden_AutoMiningDeviceT4,DrillEpic,TextureColorTool,NightVision,SurvivalTent,OxygenGenerator,OxygenHydrogenGenerator,Drill,SurvivalTool,DrillEpic,MedicGun,Eden_DrillVoidium,Eden_VoidiumScanner,Eden_VoidiumScannerT2,",
            ["ArmorMod"]             = ",ArmorBoost,ArmorBoostEpic,Eden_ArmorBoostAbyss,Eden_ArmorBoostAugmented,Eden_ColdBoostAbyss,Eden_ColdBoostAugmented,Eden_HeatBoostAbyss,Eden_HeatBoostAugmented,Eden_JetpackBoostAbyss,Eden_JetpackBoostAugmented,Eden_RadiationBoostAbyss,Eden_RadiationBoostAugmented,Eden_TransportationBoostAugmented,EVABoost,InsulationBoost,InsulationBoostEpic,JetpackBoost,JetpackBoostEpic,MobilityBoost,MobilityBoostEpic,MultiBoost,MultiBoostEpic,OxygenBoost,RadiationBoost,RadiationBoostEpic,TransportationBoost,",
            ["DeviceL"]              = ",AlienNPCBlocks,ArmorLocker,ATM,BlastDoorLargeBlocks,BoardingRampBlocks,CloneChamber,CockpitBlocksCV,ConstructorT0,ConstructorT1V2,ConstructorT2,ContainerAmmoControllerLarge,ContainerAmmoLarge,ContainerControllerLarge,ContainerExtensionLarge,ContainerLargeBlocks,ContainerPersonal,Core,CoreNoCPU,CPUExtenderBAT2,CPUExtenderBAT3,CPUExtenderBAT4,Deconstructor,DetectorCV,DoorArmoredBlocks,DoorBlocks,ElevatorMS,ExplosiveBlocks,ExplosiveBlocks2,Flare,FoodProcessorV2,ForcefieldEmitterBlocks,FridgeBlocks,FuelTankMSLarge,FuelTankMSLargeT2,FuelTankMSSmall,Furnace,GeneratorBA,GeneratorMS,GeneratorMST2,GravityGeneratorMS,HangarDoorBlocks,HumanNPCBlocks,LandClaimDevice,LandinggearBlocksCV,LCDScreenBlocks,LightLargeBlocks,LightPlant01,MedicalStationBlocks,OfflineProtector,OxygenStation,OxygenTankMS,OxygenTankSmallMS,PentaxidTank,Portal,RampLargeBlocks,RCSBlockMS,RCSBlockMS_T2,RemoteConnection,RepairBayBA,RepairBayBAT2,RepairBayConsole,RepairBayCVT2,RepairStation,SensorTriggerBlocks,ShieldGeneratorBA,ShieldGeneratorBAT2,ShieldGeneratorPOI,ShutterDoorLargeBlocks,SolarGenerator,SolarPanelBlocks,SolarPanelSmallBlocks,SpotlightBlocks,TeleporterBA,ThrusterMSDirectional,ThrusterMSRound2x2Blocks,ThrusterMSRound3x3Blocks,ThrusterMSRoundBlocks,VentilatorBlocks,PassengerSeatMS,CPUExtenderLargeT5,WarpDrive,RepairBayCV,TeleporterCV,ContainerHarvestControllerLarge,ShieldGeneratorCV,ShieldGeneratorCVT2,CPUExtenderCVT2,CPUExtenderCVT3,CPUExtenderCVT4,WarpDriveT2,DetectorCVT2,ShieldGeneratorT0,ShieldChargerLarge,FusionReactorLarge,ShieldCapacitorT2Large,ShieldCapacitorT3Large,ShieldChargerT2Large,ShieldChargerT3Large,Eden_LiftLargeBlocks,Eden_AuxillaryDummy,Eden_ShieldGeneratorAugmentedCV,Eden_AntimatterTank,Eden_WarpDriveAntimatter,Eden_ShieldGeneratorRegenerateCV,Eden_HydroponicsGrain,Eden_HydroponicsFruit,Eden_HydroponicsVegetables,Eden_HydroponicsNaturalStimulant,Eden_HydroponicsHerbalLeaves,Eden_HydroponicsPlantProtein,Eden_HydroponicsNaturalSweetener,Eden_HydroponicsFiber,Eden_HydroponicsMushroomBrown,Eden_HydroponicsSpice,Eden_HydroponicsBuds,Eden_HydroponicsBerries,Eden_HydroponicsPentaxid,Eden_ExplorationScannerCV,CVSmallSolarPanelBlocks,CVLargeSolarPanelBlocks,ShieldCapacitorLarge,Eden_ScienceStation,AsgardPassGen,AsgardThrusterCV,HWSLiftBlocks,AsgardExtensionLarge,AsgardExplosiveBlock,ThrusterMSRoundLarge,",
            ["DeviceS"]              = ",ArmorLockerSV,CloneChamberHV,ConstructorHV,ConstructorSV,Core,CPUExtenderHVT2,CPUExtenderHVT3,CPUExtenderHVT4,DetectorHVT1,DoorBlocksSV,Flare,ForcefieldEmitterBlocks,FridgeSV,FuelTankSV,FuelTankSVSmall,GeneratorSV,GeneratorSVSmall,HoverBooster,HoverEngineLarge,HoverEngineSmall,HoverEngineThruster,LightSS01,MedicStationHV,OxygenTankSV,PentaxidTankSV,RCSBlockGV,RCSBlockSV,RemoteConnection,ShieldGeneratorHV,ThrusterGVJetRound1x3x1,ThrusterGVRoundBlocks,ThrusterGVRoundLarge,ThrusterGVRoundLargeT2,ThrusterGVRoundNormalT2,ThrusterJetRound1x3x1,ThrusterJetRound2x5x2,ThrusterJetRound2x5x2V2,ThrusterJetRound3x10x3,ThrusterJetRound3x10x3V2,ThrusterJetRound3x13x3,ThrusterJetRound3x13x3V2,ThrusterJetRound3x7x3,ThrusterSVDirectional,ThrusterSVRoundBlocks,ThrusterSVRoundLarge,ThrusterSVRoundLargeT2,ThrusterSVRoundNormalT2,VentilatorBlocks,WarpDriveSV,GeneratorSVT2,ThrusterSVRoundT2Blocks,DetectorSVT2,ShieldGeneratorSVT0,ShieldGeneratorSVT2,CPUExtenderSmallT5,LargeCargoContainer,LargeHarvestContainer,ShieldCapacitorSmall,ShieldChargerSmall,FoodProcessorSmall,Eden_AuxillaryCPUSV,Eden_ShieldGeneratorAugmentedSV,Eden_WarpDriveAntimatterSV,AsgardContainerExtensionHVSV,AsgardFuelTankSVHV,AsgardExplosiveBlock,AsgardWarpDriveSV,AsgardGeneratorSVHV,ThrusterGVSuperRound2x4x2,PassengerSeatSV,PassengerSeat2SV,OxygenStationSV,ShutterDoorSmallBlocks,CockpitBlocksSV,LandinggearBlocksSV,LandinggearBlocksHeavySV,SensorTriggerBlocksSV,DetectorSVT1,ContainerControllerSmall,ContainerExtensionSmall,ContainerHarvestControllerSmall,ContainerAmmoControllerSmall,LandinggearBlocksHeavySV,RampSmallBlocks,ContainerSmallBlocks,CockpitBlocksSVT2,ShieldGeneratorSV,CPUExtenderSVT2,CPUExtenderSVT3,CPUExtenderSVT4,",
            ["WeaponPlayer"]         = ",AssaultRifle,AssaultRifleEpic,AssaultRifleT2,Chainsaw,ColorTool,DrillT2,Explosives,LaserPistol,LaserPistolT2,LaserRifle,LaserRifleEpic,Minigun,MinigunEpic,MultiTool,Pistol,PistolEpic,PistolT2,PulseRifle,RocketLauncher,RocketLauncherEpic,RocketLauncherT2,ScifiCannon,ScifiCannonEpic,Shotgun,Shotgun2,Shotgun2Epic,Sniper,Sniper2,Sniper2Epic,TextureTool,PulseRifleT2,SubmachineGunT1,SpecOpsRifle,SubmachineGunT2,GrenadeLauncher,TalonRepeatingCrossbow,",
            ["WeaponHV"]             = ",DrillAttachment,DrillAttachmentLarge,DrillAttachmentT2,SawAttachment,TurretGVArtilleryBlocks,TurretGVMinigunBlocks,TurretGVPlasmaBlocks,TurretGVRocketBlocks,TurretGVToolBlocks,WeaponSV02,TurretGVRocketBlocksT2,TurretGVArtilleryBlocksT2,WeaponSV09,WeaponSV11,",
            ["WeaponSV"]             = ",WeaponSV01,WeaponSV02,WeaponSV03,WeaponSV04,WeaponSV05,WeaponSV05Homing,TurretSVSmall,DrillAttachmentSVT2,TurretSVPulseLaserT2,TurretGVProjectileBlocksT2,TurretGVPlasmaBlocksT2,WeaponSV06,WeaponSV07,WeaponSV08,TurretGVBeamLaserBlocksT2, Eden_TurretVulcanSmall,Eden_ModularPulseLaserSVIR,Eden_ModularPulseLaserSVUV,Eden_ModularPulseLaserSVGamma,Eden_ShieldBoosterSV,AsgardDrillSV,",
            ["WeaponCV"]             = ",DrillAttachmentCV,SentryGunBlocks,TurretMSArtilleryBlocks,TurretMSLaserBlocks,TurretMSProjectileBlocks,TurretMSRocketBlocks,TurretMSToolBlocks,TurretZiraxMSLaser,TurretZiraxMSPlasma,TurretZiraxMSRocket,WeaponMS01,WeaponMS02,TurretAlien,TurretEnemyBallista,TurretMSProjectileBlocksT2,TurretMSRocketBlocksT2,TurretMSLaserBlocksT2,TurretMSArtilleryBlocksT2,WeaponMS03,TurretZiraxMSPlasmaArtillery,TurretZiraxMSLaserT2,TurretZiraxMSPlasmaT2,TurretZiraxMSRocketT2,Eden_TurretBolterCV,Eden_TurretMissileLight,Eden_TurretMissileLightT2,Eden_BlasterCV,Eden_RailgunCVSpinal_Kit,Eden_CVTorpedoRapid,Eden_TurretVulcanCV,Eden_DrillIceCV,Eden_DrillRichCV,Eden_DrillIceTurretCV,Eden_DrillTurretAutoCV,Eden_DrillTurretAutoCVT2,Eden_TurretLaserBeamCV,Eden_TurretLaserBeamCVT2,Eden_TurretLaserBeamCVT3,Eden_TurretBeamHeavyT1,Eden_TurretMissileCruiseCV,Eden_TurretMissileCruiseEMPCV,Eden_TurretMissileSwarmCV,Eden_TurretLaserT4,Eden_TurretAlienVulcan,Eden_TurretFlakClose,Eden_TurretRailgun,Eden_TurretRailgunHeavy,Eden_DrillIceTurretCVAlien,Eden_DrillTurretAutoCVAlien,AsgardDrillCV,",
            ["WeaponBA"]             = ",SentryGunBlocks,TurretBaseArtilleryBlocks,TurretBaseLaserBlocks,TurretBaseProjectileBlocks,TurretBaseRocketBlocks,TurretBaseProjectileBlocksT2,TurretBaseRocketBlocksT2,TurretBaseLaserBlocksT2,TurretBaseArtilleryBlocksT2,TurretBABeamLaserBlocksT2,",
            ["AmmoPlayer"]           = ",12.7mmBullet,5.8mmBullet,50Caliber,8.3mmBullet,DrillCharge,MultiCharge,PulseLaserChargePistol,PulseLaserChargeRifle,SciFiCannonPlasmaCharge,ShotgunShells,SlowRocket,SlowRocketHoming,",
            ["AmmoHV"]               = ",15mmBullet,ArtilleryRocket,FastRocket,TurretGVPlasmaCharge,",
            ["AmmoSV"]               = ",15mmBullet,FastRocket,FastRocketHoming,PlasmaCannonChargeSS,PulseLaserChargeSS,RailgunBullet,",
            ["AmmoCV"]               = ",15mmBullet,30mmBullet,5.8mmBullet,FastRocketMS,FlakRocketMS,LargeRocketMS,PulseLaserChargeMS,PulseLaserChargeMSWeapon,TurretMSPlasmaCharge,",
            ["AmmoBA"]               = ",15mmBullet,30mmBullet,5.8mmBullet,FastRocketBA,FlakRocket,LargeRocket,PulseLaserChargeBA,TurretBAPlasmaCharge,",
            ["Gardeners"]            = ",ConsoleSmallHuman,",
            ["Components"]           = ",AluminiumCoil,AluminiumOre,AluminiumPowder,AutoMinerCore,CapacitorComponent,Cement,CobaltAlloy,Computer,Electronics,EnergyCell,EnergyMatrix,ErestrumGel,Fiber,FluxCoil,GlassPlate,GoldIngot,HydrogenBottle,IceBlocks,LargeOptronicBridge,LargeOptronicMatrix,MagnesiumPowder,MechanicalComponents,Motor,Nanotubes,NCPowder,OpticalFiber,Oscillator,PentaxidCrystal,PentaxidElement,PentaxidOre,PlasticMaterial,PowerCoil,PromethiumOre,PromethiumPellets,RawDiamond,RockDust,SmallOptronicBridge,SmallOptronicMatrix,SteelPlate,SteelPlateArmored,WaterJug,WoodLogs,WoodPlanks,XenoSubstrate,ZascosiumAlloy,",
            ["EdenComponents"]       = ",AluminiumCoil,AluminiumOre,AluminiumPowder,Coolant,Eden_ComputerT2,Eden_DarkMatter,Eden_DarkMatterSmall,Eden_DiamondCut,Eden_DroneSalvageCore,Eden_DroneSalvageProcessor,Eden_Electromagnet,Eden_GaussRail,Eden_ModularPulseLaserLensLarge,Eden_ModularPulseLaserLensSmall,Eden_PlasmaCoil,Eden_PowerRegulator,Eden_ProgenitorArtifact,Eden_Semiconductor,Eden_Voidium,Fertilizer,HeatExchanger,HeliumBottle,NitrogenBottle,QuantumProcessor,RadiationShielding,ReactorCore,SolarCell,Superconductor,ThrusterComponents,XenonBottle,SmallUpgradeKit,LargeUpgradeKit,AdvancedUpgradeKit,Eden_MagmaciteIngot,Eden_MagmacitePlate,Eden_Deuterium,Eden_OreDenseT1Ingot,Eden_OreDenseT2Ingot,Eden_OreDenseT3Ingot,Eden_OreDenseT4Ingot,Eden_OreDenseT5Ingot,AncientRelics,LJArtifact1,LJArtifact2,LJArtifact3,LJArtifact4,NaqahdahOre,NaqahdahIngot,NaqahdahPlate,Naquadria,LJSandOre,LJEarthOre,Eden_MagmacitePlate,Eden_AugmentedMold,",
            ["Armor"]                = ",ArmorHeavy,ArmorHeavyEpic,ArmorLight,ArmorLightEpic,ArmorMedium,ArmorMediumEpic,Eden_ArmorAbyssLight,Eden_ArmorHeavyEpicReinforced,Eden_ArmorHeavyReinforced,Eden_ArmorLightAugmented,Eden_ArmorLightReinforced,Eden_ArmorMediumReinforced,AsgardArmor,AsgardArmorDonat",
            ["IngredientBasic"]      = ",Meat,Spice,AlienParts01,AlienParts02,AlienParts03,Bread,Fruit,Grain,Egg,NaturalStimulant,AlienTooth,Milk,Cheese,RottenFood,HerbalLeaves,ConfettiMossScrapings,FireMossScrapings,PlantProtein,MushroomBrown,AloeVera,AlienThorn,Vegetables,Flour,Ham,Berries,Ratatouille,NaturalSweetener,FruitJuice,Buds,",
            ["IngredientExtra"]      = ",PromethiumPellets,RockDust,PlasticMaterial,PentaxidElement,PlatinOunce,ErestrumGel,XenoSubstrate,NutrientSolution,WaterBottle,Eden_SilverIngot,",
            ["IngredientExtraMod"]   = ",Medikit04,RadiationImmunityShot,Bandages,AdrenalineShot,AntiRadiationInjection,EnergyDrink,",
            ["Deconstruct"]          = ",IronOre,CobaltOre,SiliconOre,NeodymiumOre,CopperOre,ErestrumOre,ZascosiumOre,SathiumOre,GoldOre,TitanOre,PlatinOre,Eden_MagmaciteOre,Eden_TungstenOre,",
            ["AmmoAllEnergy"]        = ",DrillCharge,PulseLaserChargePistol,PulseLaserChargeRifle,MultiCharge,SciFiCannonPlasmaCharge,PlasmaCannonAlienCharge,PlasmaCannonChargeSS,PulseLaserChargeSS,PulseLaserChargeMS,TurretGVPlasmaCharge,TurretMSPlasmaCharge,TurretEnemyLaserCharge,PulseLaserChargeBA,TurretBAPlasmaCharge,PulseLaserChargeMSWeapon,PlasmaCartridge,PulseLaserChargeMST2,TurretMSPlasmaChargeT2,PulseLaserChargeBAT2,TurretBAPlasmaChargeT2,TurretGVPlasmaChargeT2,PulseLaserChargeSST2,ZiraxMSPlasmaCharge,HeatSinkSmall,HeatSinkLarge,AsgardPlazmerAmmo,LJDrillChargeEpic,Eden_ModularPulseLaserSVIR_Ammo,Eden_ModularPulseLaserSVUV_Ammo,Eden_ModularPulseLaserSVGamma_Ammo,Eden_PlasmaChargeEntropic,Eden_PlasmaRifleXCorp_Ammo,Eden_BlasterCV_Ammo,Eden_ShieldBoosterSV_Ammo,Eden_PlasmaRifleRoyal_Ammo,",
            ["AmmoAllProjectile"]    = ",50Caliber,8.3mmBullet,5.8mmBullet,12.7mmBullet,15mmBullet,ShotgunShells,FlameThrowerCanister,RailgunBullet,30mmBullet,FlamethrowerTank,CrossbowBoltPlayer,40mmBullet,20mmBullet,Eden_TurretRailgun_Ammo,Eden_TurretRailgunHeavy_Ammo,Eden_VulcanAmmo,Eden_TurretBolterBA_Ammo,Eden_TurretBolterCV_Ammo,Eden_TurretVulcanCV_Ammo,",
            ["AmmoAllRocket"]        = ",SlowRocket,SlowRocketHoming,FastRocket,LargeRocket,FastRocketMS,FlakRocket,ArtilleryRocket,FastRocketHoming,FlakRocketMS,LargeRocketMS,FastRocketBA,TurretEnemyRocketAmmo,FastRocketGV,SVBomb,LightRocketCV,HeavyRocketMS,FlakRocketMST2,ArtilleryShellCVT2,FlakRocketBAT2,ArtilleryShellBAT2,SwarmRocketHV,ArtilleryShellHVT2,HeavyRocketBA,TorpedoSV,Eden_TurretFlakClose_Ammo,Eden_TurretRocketRapid_Ammo,Eden_TurretMissileLight_Ammo,Eden_TurretMissileLightT2_Ammo,Eden_TurretMissileCruiseCV_Ammo,Eden_TurretMissileCruiseEMPCV_Ammo,Eden_TurretMissileSwarmCV_Ammo,Eden_CVTorpedoRapid_Ammo,",
            ["WeaponPlayerUpgrades"] = ",PistolKit,RifleKit,SniperKit,ShotgunKit,HeavyWeaponKit,LaserKit,",
            ["WeaponPlayerEpic"]     = ",PulseRifleEpic,PlasmaCannonAlien,MinigunT2,FlameThrowerT2,AsgardPlazmer,Eden_PlasmaRifleEntropic,Eden_MinigunIncendiary,Eden_LaserRifleEntropic,Eden_ShotgunGauss,Eden_ShotgunDouble,Eden_ScoutRifle,Eden_Uzi,Eden_LightRailgunRifle,Eden_IonRifle,Eden_FarrPlasmaCrossbow,Eden_RifleLightning,Eden_PlasmaRifleXCorp,Eden_PlasmaRifleRoyal,AssaultRifleT3,TalonCrossbowPlayer,HeavyPistol,SubmachineGunT3,LaserPistolT3,ZiraxBeamRifle,AsgardPlazmer,",
            ["Deco"]                 = ",TurretRadar,AntennaBlocks,DecoBlocks,ConsoleBlocks,IndoorPlants,DecoBlocks2,DecoStoneBlocks,ChristmasTree,DecoVesselBlocks,DecoTribalBlocks,PosterARest,PosterBiker,PosterDontHide,PosterForeignWorld,PosterJump,PosterNewWorld,PosterSoleDesert,PosterStranger,PosterSurvivor,PosterTakingABreak,PosterTalon,PosterTrader,PosterZiraxAlienWorld,",
            ["DataPads"]             = ",Eden_UnlockPoint,Eden_WarpUpgrade,Eden_DataChipT1,Eden_DataChipT2,Eden_DataChipT3,",

            [RemoveBlocksId]         = ",ContainerUltraRare,AlienContainer,AlienContainerRare,AlienContainerVeryRare,AlienContainerUltraRare,AlienDeviceBlocks,Eden_AlienBlocksPOI,Eden_CoreNPCSpecial,Eden_CoreNPCFake,"
        };

        public bool WithinRemoveBlocks(int id) => MappedIds?.TryGetValue(RemoveBlocksId, out var removeBlocks) == true && LogicHelpers.In(id, removeBlocks);

        [JsonIgnore]
        public Dictionary<string, string> MappedIds { get; set; }

        [JsonIgnore]
        public Dictionary<string, string> NamedIds { get; set; }

        public Dictionary<StructureTankType, AllowedItem[]> StructureTank { get; set; } = new Dictionary<StructureTankType, AllowedItem[]>()
        {
            [StructureTankType.Oxygen] = new[] { new AllowedItem(4176, 250) },
            [StructureTankType.Fuel] = new[] { new AllowedItem(4421, 300), new AllowedItem(4335, 150), new AllowedItem(4314, 30) },
            [StructureTankType.Pentaxid] = new[] { new AllowedItem(4342, 1) }
        };
        public string NumberSpaceReplace { get; set; } = " "; // eigentlich :-( funktioniert aber leider nicht mehr "\u2007\u2009";
        public string BarStandardValueSign { get; set; } = "\u2588";
        public string BarStandardSpaceSign { get; set; } = "\u2591";
        public int[] HarvestCoreTypes { get; set; } = new[] { 558 };

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
