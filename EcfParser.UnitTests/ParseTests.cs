using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Linq;

namespace EcfParser.UnitTests
{
    [TestClass()]
    public class ParseTests
    {
        [TestMethod()]
        public void ReadVersionTest()
        {
            var line = @"VERSION: 9";
            var result = EcfParser.Parse.Deserialize(line);
            Assert.AreEqual(9, result.Version);
        }


        [TestMethod()]
        public void ReadAttributeStdLineTest()
        {
            var line = @"Volume: 62.5, type: float, display: true, formatter: Liter";
            var result = EcfParser.Parse.ReadAttribute(line);
            Assert.AreEqual("Volume", result.Name);
            Assert.AreEqual(62.5, (double)result.Value);

            var payload = result.AdditionalPayload;
            Assert.AreEqual("float", payload["type"]);
            Assert.AreEqual(true, (bool)payload["display"]);
            Assert.AreEqual("Liter", payload["formatter"]);
        }

        [TestMethod()]
        public void ReadAttributeTextTest()
        {
            var line = @"Volume: ""110,110,110""";
            var result = EcfParser.Parse.ReadAttribute(line);
            Assert.AreEqual("Volume", result.Name);
            Assert.AreEqual("110,110,110", result.Value);
        }

        [TestMethod()]
        public void ReadAttributeTextAttrTest()
        {
            var line = @"Volume: ""110,110,110"", abc: 42";
            var result = EcfParser.Parse.ReadAttribute(line);
            Assert.AreEqual("Volume", result.Name);
            Assert.AreEqual("110,110,110", result.Value);

            var payload = result.AdditionalPayload;
            Assert.AreEqual(42, (int)payload["abc"]);
        }

        [TestMethod()]
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
            Assert.AreEqual("Id", result.Blocks[0].Attributes.First().Name);
            Assert.AreEqual(2373, (int)result.Blocks[0].Attributes.First().Value);
            Assert.AreEqual(7, result.Blocks[0].Attributes.Count);
        }

        [TestMethod()]
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
            Assert.AreEqual("Id", block.Attributes.First().Name);
            Assert.AreEqual(2373, (int)block.Attributes.First().Value);
            Assert.AreEqual(7, block.Attributes.Count);

            Assert.AreEqual(1, block.Childs.Count);
            var child = block.Childs.Values.First();
            Assert.AreEqual(5, child.Attributes.Count);

        }

        [TestMethod()]
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
            Assert.AreEqual("Id", block.Attributes.First().Name);
            Assert.AreEqual(2373, (int)block.Attributes.First().Value);
            Assert.AreEqual(7, block.Attributes.Count);

            Assert.AreEqual(3, block.Childs.Count);
            Assert.AreEqual(5, block.Childs.Values.First().Attributes.Count);
            Assert.IsNull(block.Childs.Values.Last().Attributes);

        }

        [TestMethod()]
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
            Assert.AreEqual("Name", block.Attributes.First().Name);
            Assert.AreEqual("CPUExtenderCVT4", block.Attributes.First().Value);
            Assert.AreEqual(3, block.Attributes.Count);

            Assert.AreEqual(1, block.Childs.Count);
            var child = block.Childs.Values.First();
            Assert.AreEqual("Child Inputs", child.Name);
            Assert.AreEqual(8, child.Attributes.Count);
        }



        [TestMethod()]
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
            Assert.AreEqual("Id", block.Attributes.First().Name);
            Assert.AreEqual(1, (int)block.Attributes.First().Value);
            Assert.AreEqual(3, block.Attributes.Count);

            Assert.AreEqual(2, block.Childs.Count);
            var child = block.Childs.Values.First();
            Assert.AreEqual("Child 0", child.Name);
            Assert.AreEqual(2, child.Attributes.Count);
        }



        [TestMethod()]
        public void TestReadAllEcfFiles()
        {
            var configDir = @"C:\SteamGames\steamapps\common\Empyrion - Galactic Survival\Content\Configuration";

            if (!Directory.Exists(configDir)) return;

            Directory.GetFiles(configDir, "*.ecf")
                .ToList()
                .ForEach(F =>
                {
                    try
                    {
                        EcfParser.Parse.Deserialize(File.ReadAllLines(F));
                    }
                    catch (Exception error)
                    {
                        throw new Exception(F, error);
                    }
                });
        }
    }
}