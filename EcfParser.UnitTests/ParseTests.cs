using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace EcfParser.UnitTests
{
    [TestClass()]
    public class ParseTests
    {
        [TestMethod]
        public void ReadBlockMapping()
        {
            var result = EcfParser.Parse.ReadBlockMapping(Path.Combine(Directory.GetCurrentDirectory(), @"..\..\Data\blocksmap.dat"));
            Assert.AreEqual(161, result.Count);
            Assert.AreEqual(2048, result["ThrusterGVSuperRound2x4x2"]);
            Assert.AreEqual(2052, result["WarpDriveT2"]);
            Assert.AreEqual(2208, result["Eden_TestModelBlock2"]);
        }

        [TestMethod]
        public void ReadAttributeStdLineTest()
        {
            var line = @"Volume: 62.5, type: float, display: true, formatter: Liter";
            var result = EcfParser.Parse.ReadAttribute(line);
            Assert.AreEqual("Volume", result.Name);
            Assert.AreEqual(62.5, (double)result.Value);

            var payload = result.AddOns;
            Assert.AreEqual("float", payload["type"]);
            Assert.AreEqual(true, (bool)payload["display"]);
            Assert.AreEqual("Liter", payload["formatter"]);
        }

        [TestMethod]
        public void ReadAttributeTextTest()
        {
            var line = @"Volume: ""110,110,110""";
            var result = EcfParser.Parse.ReadAttribute(line);
            Assert.AreEqual("Volume", result.Name);
            Assert.AreEqual("110,110,110", result.Value);
        }

        [TestMethod]
        public void ReadAttributeTextAttrTest()
        {
            var line = @"Volume: ""110,110,110"", abc: 42";
            var result = EcfParser.Parse.ReadAttribute(line);
            Assert.AreEqual("Volume", result.Name);
            Assert.AreEqual("110,110,110", result.Value);

            var payload = result.AddOns;
            Assert.AreEqual(42, (int)payload["abc"]);
        }

        [TestMethod]
        public void ReadAttributeBlockWithAliasAttr()
        {
            var line = @"
{ Item Name: FusionCell, Ref: ComponentsTemplate
  Mass: 75, type: float, display: true, formatter: Kilogram
  Volume: 10.5, type: float, display: true, formatter: Liter
  UnlockCost: 25, display: true
  UnlockLevel: 20, display: true
  TechTreeParent: EnergyCellLarge
  TechTreeNames: Misc
}
";

            var alias = new Dictionary<string, int> { { "FusionCell", 2373 } };

            var result = EcfParser.Parse.Deserialize(line.Split('\n'));
            Assert.AreEqual(1, result.Blocks.Count);
            Assert.AreEqual("Item", result.Blocks[0].Name);
            Assert.AreEqual("Name", result.Blocks[0].Attr.First().Name);
            Assert.AreEqual("FusionCell", result.Blocks[0].Attr.FirstOrDefault(a => a.Name == "Name").Value);
            Assert.AreEqual(7, result.Blocks[0].Attr.Count);
        }


        [TestMethod]
        public void ReadAttributeBlockAttr()
        {
            var line = @"
{ Item Id: 2373, Name: FusionCell, Ref: ComponentsTemplate
  Mass: 75, type: float, display: true, formatter: Kilogram
  Volume: 10.5, type: float, display: true, formatter: Liter
  UnlockCost: 25, display: true
  UnlockLevel: 20, display: true
  TechTreeParent: EnergyCellLarge
  TechTreeNames: Misc
}
";
            var result = EcfParser.Parse.Deserialize(line.Split('\n'));
            Assert.AreEqual(1, result.Blocks.Count);
            Assert.AreEqual("Item", result.Blocks[0].Name);
            Assert.AreEqual("Id", result.Blocks[0].Attr.First().Name);
            Assert.AreEqual(2373, (int)result.Blocks[0].Attr.First().Value);
            Assert.AreEqual(7, result.Blocks[0].Attr.Count);
        }

        [TestMethod]
        public void ReadAttributeBlockUnnamedChildAttr()
        {
            var line = @"
{ Item Id: 2373, Name: FusionCell, Ref: ComponentsTemplate
  Mass: 75, type: float, display: true, formatter: Kilogram
  Volume: 10.5, type: float, display: true, formatter: Liter
  {
    ROF: 1, type: float
    AddHealth: 5, display: HealthValue
    AddStamina: 10, display: StaminaValue
    AddFood: 30, display: FoodValue
    AddOxygen: 0
  }
  UnlockCost: 25, display: true
  UnlockLevel: 20, display: true
  TechTreeParent: EnergyCellLarge
  TechTreeNames: Misc
}
";
            var result = EcfParser.Parse.Deserialize(line.Split('\n'));
            Assert.AreEqual(1, result.Blocks.Count);

            var block = result.Blocks[0];

            Assert.AreEqual("Item", block.Name);
            Assert.AreEqual("Id", block.Attr.First().Name);
            Assert.AreEqual(2373, (int)block.Attr.First().Value);
            Assert.AreEqual(7, block.Attr.Count);

            Assert.AreEqual(1, block.Childs.Count);
            var child = block.Childs.Values.First();
            Assert.AreEqual(5, child.Attr.Count);

        }

        [TestMethod]
        public void ReadAttributeBlock3UnnamedChildAttr()
        {
            var line = @"
{ Item Id: 2373, Name: FusionCell, Ref: ComponentsTemplate
  Mass: 75, type: float, display: true, formatter: Kilogram
  Volume: 10.5, type: float, display: true, formatter: Liter
  {
    ROF: 1, type: float
    AddHealth: 5, display: HealthValue
    AddStamina: 10, display: StaminaValue
    AddFood: 30, display: FoodValue
    AddOxygen: 0
  }
  {
    ROF: 2, type: float
    AddHealth: 5, display: HealthValue
    AddStamina: 10, display: StaminaValue
    AddFood: 30, display: FoodValue
    AddOxygen: 0
  }
  {
  }
  UnlockCost: 25, display: true
  UnlockLevel: 20, display: true
  TechTreeParent: EnergyCellLarge
  TechTreeNames: Misc
}
";
            var result = EcfParser.Parse.Deserialize(line.Split('\n'));
            Assert.AreEqual(1, result.Blocks.Count);

            var block = result.Blocks[0];

            Assert.AreEqual("Item", block.Name);
            Assert.AreEqual("Id", block.Attr.First().Name);
            Assert.AreEqual(2373, (int)block.Attr.First().Value);
            Assert.AreEqual(7, block.Attr.Count);

            Assert.AreEqual(3, block.Childs.Count);
            Assert.AreEqual(5, block.Childs.Values.First().Attr.Count);
            Assert.IsNull(block.Childs.Values.Last().Attr);

        }

        [TestMethod]
        public void ReadAttributeBlockNamedChildAttr()
        {
            var line = @"
{ Template Name: CPUExtenderCVT4
  CraftTime: 800
  Target: AdvC
  { Child Inputs
    SteelPlate: 24
    Electronics: 16
    Computer: 20
    OpticalFiber: 12
    FluxCoil: 14
    PowerCoil: 6
    LargeOptronicBridge: 2
    LargeOptronicMatrix: 1
  }
}
";
            var result = EcfParser.Parse.Deserialize(line.Split('\n'));
            Assert.AreEqual(1, result.Blocks.Count);

            var block = result.Blocks[0];

            Assert.AreEqual("Template", block.Name);
            Assert.AreEqual("Name", block.Attr.First().Name);
            Assert.AreEqual("CPUExtenderCVT4", block.Attr.First().Value);
            Assert.AreEqual(3, block.Attr.Count);

            Assert.AreEqual(1, block.Childs.Count);
            var child = block.Childs.Values.First();
            Assert.AreEqual("Child Inputs", child.Name);
            Assert.AreEqual(8, child.Attr.Count);
        }

        [TestMethod]
        public void ReadAttributeBlockShapesAttr()
        {
            var line = @"
/* Two shape families for Plastic Large Block */
{ +Block Id: 1482, Name: PlasticFullLarge
  DropMeshfile: Entities/Misc/BagSmallPrefab
  Material: plastic
  Texture: 77
  BlockColor: ""10,10,10""
  Shape: New
  Place: Free
  Model: Cube
  TemplateRoot: PlasticLargeBlocks
  AllowPlacingAt: ""Base,MS"", display: true
  HitPoints: 200, type: int, display: true
  Mass: 100, type: float, display: true, formatter: Kilogram
  BlockSizeScale: 8  
  ChildShapes: ""Cube, CutCornerE, CutCornerB, SlicedCornerA1, CornerHalfB, CornerSmallC, CornerC, CornerHalfA3, RampCMedium, RampA, RampC, CornerRoundB, CornerRoundADouble, RoundCornerA, CubeRoundConnectorA, EdgeRound, Cylinder, RampRoundFTriple, RampRoundF, SmallCornerRoundB, SmallCornerRoundA, SphereHalf, Cone, ConeB, CutCornerC, Cylinder6Way, CornerRoundATriple, CornerA, CornerHalfA1, CornerDoubleA3, CornerSmallB, PyramidA""
  UpgradeTo: HullFullLarge, display: true
}
";
            var result = EcfParser.Parse.Deserialize(line.Split('\n'));
            Assert.AreEqual(1, result.Blocks.Count);

            var block = result.Blocks[0];

            Assert.AreEqual("Block", block.Name);
            Assert.AreEqual("Id", block.Attr.First().Name);
            Assert.AreEqual(413, block.Attr.First(n => n.Name == "ChildShapes").Value.ToString().Length);
            Assert.AreEqual(15, block.Attr.Count);
        }

        [TestMethod]
        public void ReadAttributeBlockSprout()
        {
            var line = @"
# Sprout
{ Block Id: 591, Name: WheatStage1
  Class: PlantGrowing
  MarketPrice: 78, display: true
  AllowedInBlueprint: false, display: true 
  IndexName: Plant
  Material: plants
  Shape: ModelEntity
  Model: @models2/Entities/Farming/SpeedTrees/WheatStage1Prefab
  DropMeshfile: Entities/Misc/BagSmallPrefab
  { Child PlantGrowing
    Next: WheatStage2
    GrowthRate: 20
    FertileLevel: 3
    OnDeath: PlantDead
  }
  IsAccessible: false, type: bool
  Collide: ""bullet,rocket,melee,sight""
  Place: Free
# ModelOffset: ""0,0.5,0""
  AllowPlacingAt: ""Base,MS"", display: true
  SizeInBlocks: ""1,1,1"", display: true
  SizeInBlocksLocked: ""Base,MS""
  ShowBlockName: true
  CropType: Grain, display: true
  CropYield: 6, display: true
  GrowthTimeInfo: 40, type: int, display: true, formatter: Minutes
  Mass: 1, type: float, display: true, formatter: Kilogram
  Info: bkiPlantSprout, display: true
  Category: Farming
  XpFactor: 1
  PickupTarget: WheatStage1  # disassemble 
  TemplateRoot: WheatStage2  # deconstruct - to avoid exploit 
}
";
            var result = EcfParser.Parse.Deserialize(line.Split('\n'));
            Assert.AreEqual(1, result.Blocks.Count);

            var block = result.Blocks[0];

            Assert.AreEqual("Block", block.Name);
            Assert.AreEqual("Id", block.Attr.First().Name);
            Assert.AreEqual(25, block.Attr.Count);
        }

        [TestMethod]
        public void ReadAttributeBlockChildNumAttr()
        {
            var line = @"
{ TabGroup Id: 1
  Name: ""All Shapes""
  Icon: ""AllShapes""

  {  Child 0
     ParentBlocks: ""396,399,402,405,408,411,1322,1395,1481""
     Shapes: ""Cube, CubeSliced, CubeLShape, CubeStepped, CubeSteppedEdge, CubeHalfCubeConnector, CubeHalfRamp, CubeHalf, CubeQuarter, CubeQuarterEdge, CubeEighth, RampA, RampB, RampC, RampCMedium, RampBMedium, RampCLow, RampD, RampDLow, RampE, RampEHalf, RampADouble, RampBDouble, RampADoubleHalf, RampBDoubleHalf, RampAHalfleft, RampAHalfright, RampCHalf, RampCMediumHalfright, RampCMediumHalfleft, RampCMediumQuarter, CutCornerA, CutCornerB, CutCornerC, CutCornerD, CutCornerE, CutCornerDMedium, CutCornerEMedium, CutCornerDLow, SlicedCornerA1, SlicedCornerA2, SlicedCornerA1Medium, SlicedCornerA1Low, SlicedCornerB1, SlicedCornerB2, SlicedCornerB1Medium, SlicedCornerB2Medium, NotchedA, NotchedB, NotchedC, SlicedCornerD, NotchedBMedium, NotchedCMedium, SlicedCornerDMedium, NotchedCLow, PyramidA, EdgeRound, EdgeRoundHalf, EdgeRoundThin, EdgeRoundMedium, EdgeRoundLow, EdgeRoundMediumHalfDouble, EdgeRoundMediumHalf, EdgeRoundMediumQuarter, EdgeRoundLowHalf, EdgeRoundLowQuarter, EdgeRoundLowEighth, RoundCornerA, CornerRoundAMedium, CornerRoundALow, CornerRoundAMediumQuarter, CornerRoundALowQuarter, CornerRoundB, CornerRoundBMedium, CornerRoundBLow, CornerRoundBMediumQuarter, CornerRoundBLowQuarter, EdgeRoundDoubleA, EdgeRoundDoubleAHalf, RampRoundADouble, RampRoundADoubleHalf, RampRoundBDouble, RampRoundC, RampRoundDDouble, RampRoundE, RampRoundF, CornerRoundADouble, CornerRoundATriple, RampRoundFTriple, SmallCornerRoundA, SmallCornerRoundB, SmallCornerRoundB2, SmallCornerRoundC, Cylinder, Cylinder6Way, CylinderL, CylinderThin, CylinderThinTJoint, CylinderThin3Way, CylinderThinXJoint, CylinderThin4Way, CylinderThin5Way, CylinderThin6Way, PipesFence, PipesFenceDiagonal, PipesFenceKinked, PipesL, PipesT, PipesX, FenceTop, FenceTopDiagonal, ConeB, SphereHalf, Cone, CornerDoubleA1, CornerDoubleA2, CornerDoubleA3, CornerDoubleB1, CornerDoubleB2, CornerDoubleB3, CornerA, CornerB, CornerC, CornerCMedium, CornerCLow, CornerHalfA1, CornerHalfA2, CornerHalfA3, CornerHalfA3Medium, CornerHalfA3Low, CornerHalfB, CornerHalfC, CornerSmallA, CornerSmallB, CornerSmallBMedium, CornerSmallBMediumLow, CornerSmallBLow, CornerSmallC, CornerSmallCMedium, CornerSmallCMediumLow, CornerSmallCLow, RampConnectorAleft, RampConnectorAright, RampConnectorBleft, RampConnectorBright, RampConnectorCleft, RampConnectorCright, RampConnectorDleft, RampConnectorDright, RampConnectorEleft, RampConnectorFright, CylinderRoundTransition, CubeRoundConnectorA, CubeRoundConnectorAMedium, CubeRoundConnectorALow, CubeRoundConnectorBleft, CubeRoundConnectorBright, RampRoundConnectorAleft, RampRoundConnectorAright, RampRoundConnectorBleft, RampRoundConnectorBright, CubeRoundTransitionleft, CubeRoundTransitionright, CylinderCubeConnector, CylinderCubeHalfConnector, CylinderWallConnector, Wall, WallLow, Beam, BeamQuarter, WallSlopedAright, WallSlopedAleft, WallSlopedC, WallSlopedCMediumright, WallSlopedCMediumleft, CylinderFramed, CubeFramed, WallUShape, WallCorner, WallLShape, WallLShapeMedium, WallLShapeLow, WallDouble, WallMediumDouble, WallLowDouble, WallSlopedADouble, WallSlopedBDouble, WallSlopedCDouble, WallSlopedBDoubleMedium, WallSlopedCDoubleMedium, WallSlopedCDoubleLow, WallEdge, WallCornerSloped, WallSloped, WallSloped3Corner, WallSloped3CornerLow, WallEdgeRound, WallSlopedRound, WallEdgeRound3Way, WallCornerRoundA, WallCornerRoundB, WallCornerRoundC, WallSlopedBold, CorridorWallA, CorridorWallB, CorridorEdgeA, CorridorEdgeB, CorridorEdgeC, CorridorPillarA, CorridorPillarB, CorridorPillarC, CorridorPillarD, CorridorRoof, CorridorRoofCorner, CorridorRoofCornerInverted, CorridorRoofCornerRound, CorridorBulkyWallA, CorridorBulkyWallAWindowed, CorridorBulkyWallB, CorridorBulkyWallBWindowed, CorridorRampA, CorridorRampB, DoorframeA, DoorframeB, DoorframeC ""
  }

    { Child 1
      ParentBlocks: ""380,393,1478,1594""
      Shapes: ""Cube, CubeSliced, CubeLShape, CubeStepped, CubeSteppedEdge, CubeHalfCubeConnector, CubeHalfRamp, CubeHalf, CubeQuarter, CubeQuarterEdge, CubeEighth, RampA, RampB, RampC, RampCMedium, RampBMedium, RampCLow, RampD, RampDLow, RampE, RampEHalf, RampADouble, RampBDouble, RampADoubleHalf, RampBDoubleHalf, RampAHalfleft, RampAHalfright, RampCHalf, RampCMediumHalfright, RampCMediumHalfleft, RampCMediumQuarter, CutCornerA, CutCornerB, CutCornerC, CutCornerD, CutCornerE, CutCornerDMedium, CutCornerEMedium, CutCornerDLow, SlicedCornerA1, SlicedCornerA2, SlicedCornerA1Medium, SlicedCornerA1Low, SlicedCornerB1, SlicedCornerB2, SlicedCornerB1Medium, SlicedCornerB2Medium, NotchedA, NotchedB, NotchedC, SlicedCornerD, NotchedBMedium, NotchedCMedium, SlicedCornerDMedium, NotchedCLow, PyramidA, EdgeRound, EdgeRoundHalf, EdgeRoundThin, EdgeRoundMedium, EdgeRoundLow, EdgeRoundMediumHalfDouble, EdgeRoundMediumHalf, EdgeRoundMediumQuarter, EdgeRoundLowHalf, EdgeRoundLowQuarter, EdgeRoundLowEighth, RoundCornerA, CornerRoundAMedium, CornerRoundALow, CornerRoundAMediumQuarter, CornerRoundALowQuarter, CornerRoundB, CornerRoundBMedium, CornerRoundBLow, CornerRoundBMediumQuarter, CornerRoundBLowQuarter, EdgeRoundDoubleA, EdgeRoundDoubleAHalf, RampRoundADouble, RampRoundADoubleHalf, RampRoundBDouble, RampRoundC, RampRoundDDouble, RampRoundE, RampRoundF, CornerRoundADouble, CornerRoundATriple, RampRoundFTriple, SmallCornerRoundA, SmallCornerRoundB, SmallCornerRoundB2, SmallCornerRoundC, Cylinder, Cylinder6Way, CylinderL, CylinderThin, CylinderThinTJoint, CylinderThin3Way, CylinderThinXJoint, CylinderThin4Way, CylinderThin5Way, CylinderThin6Way, PipesFence, PipesFenceDiagonal, PipesFenceKinked, PipesL, PipesT, PipesX, FenceTop, FenceTopDiagonal, ConeB, SphereHalf, Cone, CornerDoubleA1, CornerDoubleA2, CornerDoubleA3, CornerDoubleB1, CornerDoubleB2, CornerDoubleB3, CornerA, CornerB, CornerC, CornerCMedium, CornerCLow, CornerHalfA1, CornerHalfA2, CornerHalfA3, CornerHalfA3Medium, CornerHalfA3Low, CornerHalfB, CornerHalfC, CornerSmallA, CornerSmallB, CornerSmallBMedium, CornerSmallBMediumLow, CornerSmallBLow, CornerSmallC, CornerSmallCMedium, CornerSmallCMediumLow, CornerSmallCLow, RampConnectorAleft, RampConnectorAright, RampConnectorBleft, RampConnectorBright, RampConnectorCleft, RampConnectorCright, RampConnectorDleft, RampConnectorDright, RampConnectorEleft, RampConnectorFright, CylinderRoundTransition, CubeRoundConnectorA, CubeRoundConnectorAMedium, CubeRoundConnectorALow, CubeRoundConnectorBleft, CubeRoundConnectorBright, RampRoundConnectorAleft, RampRoundConnectorAright, RampRoundConnectorBleft, RampRoundConnectorBright, CubeRoundTransitionleft, CubeRoundTransitionright, CylinderCubeConnector, CylinderCubeHalfConnector, CylinderWallConnector, Wall, WallLow, Beam, BeamQuarter, WallSlopedAright, WallSlopedAleft, WallSlopedC, WallSlopedCMediumright, WallSlopedCMediumleft, CylinderFramed, CubeFramed, WallUShape, WallCorner, WallLShape, WallLShapeMedium, WallLShapeLow, WallDouble, WallMediumDouble, WallLowDouble, WallSlopedADouble, WallSlopedBDouble, WallSlopedCDouble, WallSlopedBDoubleMedium, WallSlopedCDoubleMedium, WallSlopedCDoubleLow, WallEdge, WallCornerSloped, WallSloped, WallSloped3Corner, WallSloped3CornerLow, WallEdgeRound, WallSlopedRound, WallEdgeRound3Way, WallCornerRoundA, WallCornerRoundB, WallCornerRoundC, WallSlopedBold""
    }
}
";
            var result = EcfParser.Parse.Deserialize(line.Split('\n'));
            Assert.AreEqual(1, result.Blocks.Count);

            var block = result.Blocks[0];

            Assert.AreEqual("TabGroup", block.Name);
            Assert.AreEqual("Id", block.Attr.First().Name);
            Assert.AreEqual(1, (int)block.Attr.First().Value);
            Assert.AreEqual(3, block.Attr.Count);

            Assert.AreEqual(2, block.Childs.Count);
            var child = block.Childs.Values.First();
            Assert.AreEqual("Child 0", child.Name);
            Assert.AreEqual(2, child.Attr.Count);
        }



        [TestMethod]
        public void TestReadAllEcfFiles()
        {
            var configDir = @"C:\SteamGames\steamapps\common\Empyrion - Galactic Survival\Content\Configuration";

            if (!Directory.Exists(configDir)) return;

            EcfFile mergeAll = null;

            Directory.GetFiles(configDir, "*.ecf")
                .ToList()
                .ForEach(F =>
                {
                    try
                    {
                        var ecf = EcfParser.Parse.Deserialize(File.ReadAllLines(F));
                        if (mergeAll == null) mergeAll = ecf;
                        else                  mergeAll.MergeWith(ecf);
                    }
                    catch (Exception error)
                    {
                        throw new Exception(F, error);
                    }
                });

            var blockById = mergeAll.Blocks.
                EcfBlocksToDictionary(
                    B => (B.Name == "Block" || B.Name == "Item") && B.Attr.Any(A => A.Name == "Id"), 
                    B => B.Attr.FirstOrDefault(a => a.Name == "Id")?.Value);

            var b = blockById[1367];
        }

        [TestMethod]
        public void ReadHarvestPlant()
        {
            var line = @"
{ Block Id: 1329, Name: TomatoStage4
  Class: CropsGrown
  AllowedInBlueprint: false, display: true 
  IndexName: Plant
  CropType: Vegetables, display: true
  { Child DropOnHarvest
    Item: Vegetables
    Count: 4
  }
  { Child CropsGrown   
    OnHarvest: TomatoStage4NoFruit
    OnDeath: PlantDead2
  }
  Material: plants
  Shape: ModelEntity
  Model: @models2/Entities/Farming/SpeedTrees/TomatoPlantStage4Prefab
  PickupTarget: TomatoStage1
  IsAccessible: false, type: bool
  Collide: ""bullet,rocket,melee,sight""
  AllowPlacingAt: ""Base,MS"", display: true
  SizeInBlocks: ""1,1,1"", display: true
  SizeInBlocksLocked: ""Base,MS""
  ShowBlockName: true
  XpFactor: 2.0
}
";

            var alias = new Dictionary<string, int> { { "TomatoStage4", 1329 } };

            var result = EcfParser.Parse.Deserialize(line.Split('\n'));

            Assert.AreEqual(1, result.Blocks.Count);
            var harvestData =
                result.Blocks
                    .Where(b => b.Attr.FirstOrDefault(a => a.Name == "Class")?.Value?.ToString() == "CropsGrown")
                    .Select(b =>
                    {
                        var childDropOnHarvest = b.Childs.FirstOrDefault(c => c.Key == "Child DropOnHarvest");
                        var dropOnHarvestItem  = childDropOnHarvest.Value.Attr.FirstOrDefault(a => a.Name == "Item").Value;
                        var dropOnHarvestCount = int.TryParse(childDropOnHarvest.Value.Attr.FirstOrDefault(a => a.Name == "Count")?.Value?.ToString(), out var count) ? count : 0;

                        var childOnHarvest     = b.Childs.FirstOrDefault(c => c.Key == "Child CropsGrown").Value.Attr.FirstOrDefault(a => a.Name == "OnHarvest").Value;

                        return new
                        {
                            Id                  = b.Name,
                            DropOnHarvestItem   = dropOnHarvestItem,
                            DropOnHarvestCount  = dropOnHarvestCount,
                            ChildOnHarvest      = childOnHarvest
                        };
                    })
                    .ToDictionary(b => b.Id, b => b);

        }

    }
}