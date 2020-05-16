using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

namespace EcfParser.Tests
{
    [TestClass()]
    public class MergeTests
    {
        [TestMethod()]
        public void MergeExistingAttribute()
        {
            var ecf1Lines = @"
{ Block Id: 267, Name: CockpitMS02
  Mass: 284, type: float, display: true, formatter: Kilogram
  BlastRadius: 2
  BlastDamage: 80
}
";

            var ecf2Lines = @"
{ Block Id: 267
  BlastRadius: 20
  IsLockable: false
}
";

            var ecf1 = EcfParser.Parse.Deserialize(ecf1Lines.Split('\n'));
            var ecf2 = EcfParser.Parse.Deserialize(ecf2Lines.Split('\n'));
            ecf1.MergeWith(ecf2);

            Assert.AreEqual(1, ecf1.Blocks.Count);
            var block = ecf1.Blocks.First();

            Assert.AreEqual(284,   (int) block.Attr.FirstOrDefault(a => a.Name == "Mass").Value);
            Assert.AreEqual(20,    (int) block.Attr.FirstOrDefault(a => a.Name == "BlastRadius").Value);
            Assert.AreEqual(false, (bool)block.Attr.FirstOrDefault(a => a.Name == "IsLockable").Value);
        }

        [TestMethod()]
        public void MergeNewAttribute()
        {
            var ecf1Lines = @"
{ Block Id: 267, Name: CockpitMS02
  Group: cpgCockpit
  Material: metal
  ShowBlockName: true
  BlockColor: ""110,110,110""
  Volume: 620, type: float, display: true, formatter: Liter
  TemplateRoot: CockpitBlocksCV
  IsLockable: true
  IsIgnoreLC: true
  Info: bkiCockpit, display: true
  StackSize: 1000
  Category: Devices
  Mass: 284, type: float, display: true, formatter: Kilogram
  HitPoints: 300, type: int, display: true
  EnergyIn: 20, type: int, display: true, formatter: Watt
  BlastDamage: 80
}
";

            var ecf2Lines = @"
{ Block Id: 267
  BlastRadius: 20
  IsLockable: false
}
";

            var ecf1 = EcfParser.Parse.Deserialize(ecf1Lines.Split('\n'));
            var ecf2 = EcfParser.Parse.Deserialize(ecf2Lines.Split('\n'));
            ecf1.MergeWith(ecf2);

            Assert.AreEqual(1, ecf1.Blocks.Count);
            var block = ecf1.Blocks.First();

            Assert.AreEqual(284,   (int) block.Attr.FirstOrDefault(a => a.Name == "Mass").Value);
            Assert.AreEqual(20,    (int) block.Attr.FirstOrDefault(a => a.Name == "BlastRadius").Value);
            Assert.AreEqual(false, (bool)block.Attr.FirstOrDefault(a => a.Name == "IsLockable").Value);
        }

        [TestMethod()]
        public void MergePayloadAttribute()
        {
            var ecf1Lines = @"
{ Block Id: 267, Name: CockpitMS02
  Mass: 284, type: float, display: true, formatter: Kilogram
  BlastRadius: 2
  BlastDamage: 80
}
";

            var ecf2Lines = @"
{ Block Id: 267
  Mass: 284, display: false
  BlastRadius: 20
  IsLockable: false
}
";

            var ecf1 = EcfParser.Parse.Deserialize(ecf1Lines.Split('\n'));
            var ecf2 = EcfParser.Parse.Deserialize(ecf2Lines.Split('\n'));
            ecf1.MergeWith(ecf2);

            Assert.AreEqual(1, ecf1.Blocks.Count);
            var block = ecf1.Blocks.First();

            Assert.AreEqual(284,   (int) block.Attr.FirstOrDefault(a => a.Name == "Mass").Value);
            Assert.AreEqual(false, (bool)block.Attr.FirstOrDefault(a => a.Name == "Mass").AddOns.First(p => p.Key == "display").Value);
            Assert.AreEqual(20,    (int) block.Attr.FirstOrDefault(a => a.Name == "BlastRadius").Value);
            Assert.AreEqual(false, (bool)block.Attr.FirstOrDefault(a => a.Name == "IsLockable").Value);
        }


        [TestMethod()]
        public void MergeAddPayloadAttribute()
        {
            var ecf1Lines = @"
{ Block Id: 267, Name: CockpitMS02
  Mass: 284, type: float, display: true, formatter: Kilogram
  BlastRadius: 2
  BlastDamage: 80
}
";

            var ecf2Lines = @"
{ Block Id: 267
  BlastRadius: 20, display: false
  IsLockable: false
}
";

            var ecf1 = EcfParser.Parse.Deserialize(ecf1Lines.Split('\n'));
            var ecf2 = EcfParser.Parse.Deserialize(ecf2Lines.Split('\n'));
            ecf1.MergeWith(ecf2);

            Assert.AreEqual(1, ecf1.Blocks.Count);
            var block = ecf1.Blocks.First();

            Assert.AreEqual(284,   (int) block.Attr.FirstOrDefault(a => a.Name == "Mass").Value);
            Assert.AreEqual(20,    (int) block.Attr.FirstOrDefault(a => a.Name == "BlastRadius").Value);
            Assert.AreEqual(false, (bool) block.Attr.FirstOrDefault(a => a.Name == "BlastRadius").AddOns.First(p => p.Key == "display").Value);
            Assert.AreEqual(false, (bool)block.Attr.FirstOrDefault(a => a.Name == "IsLockable").Value);
        }
    }
}