using System;
using System.IO;
using System.Linq;
using Eleon.Modding;
using EmpyrionScripting.CustomHelpers;
using EmpyrionScripting.DataWrapper;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace EmpyrionScripting.UnitTests
{
    [TestClass]
    public class UnitTest1
    {
        class Item
        {
            public int Id { get; set; }
            public int Count { get; set; }
            public string Name { get; set; }
        }

        [TestMethod]
        public void TestMethodTestEQ()
        {
            var lcdMod = new EmpyrionScripting();
            Assert.AreEqual(
                "Yes:aNo:bNo:c",
                lcdMod.ExecuteHandlebarScript(new { I = new[] {
                    new Item(){ Id = 1, Name = "a"},
                    new Item(){ Id = 2, Name = "b"},
                    new Item(){ Id = 3, Name = "c"},
                }
                }, "{{#each I}}{{test Id eq 1}}Yes:{{Name}}{{else}}No:{{Name}}{{/test}}{{/each}}")
            );
        }

        [TestMethod]
        public void TestMethodTestLEQ()
        {
            var lcdMod = new EmpyrionScripting();
            Assert.AreEqual(
                "Yes:aYes:bNo:c",
                lcdMod.ExecuteHandlebarScript(new
                {
                    S = new { 
                        I = new[] {
                        new ItemsData(){ Id = 1, Name = "a"},
                        new ItemsData(){ Id = 2, Name = "b"},
                        new ItemsData(){ Id = 3, Name = "c"},
                    }
                }
                }, "{{#each S.I}}{{test Id leq 2}}Yes:{{Name}}{{else}}No:{{Name}}{{/test}}{{/each}}")
            );
        }

        [TestMethod]
        public void TestMethodTestLEQReal()
        {
            var lcdData = Mock.Of<ScriptRootData>();
            lcdData.E   = Mock.Of<EntityData>();
            lcdData.E.S = Mock.Of<StructureData>();
            lcdData.E.S.Items = new[] {
                new ItemsData(){ Id = 1, Count = 10, Name = "a" },
                new ItemsData(){ Id = 2, Count = 20, Name = "b" },
                new ItemsData(){ Id = 3, Count = 30, Name = "c" },
            };
            var lcdMod = new EmpyrionScripting();
            Assert.AreEqual(
                "Yes:aYes:bNo:c",
                lcdMod.ExecuteHandlebarScript(lcdData, "{{#each E.S.Items}}{{test Id leq 2}}Yes:{{Name}}{{else}}No:{{Name}}{{/test}}{{/each}}")
            );
        }

        [TestMethod]
        public void TestMethodItems()
        {
            var lcdData = Mock.Of<ScriptRootData>();
            lcdData.E = Mock.Of<EntityData>();
            lcdData.E.S = Mock.Of<StructureData>();
            lcdData.E.S.AllCustomDeviceNames = new[] { "BoxA", "BoxB", "BoxC" };
            lcdData.E.S.Items = new[] {
                new ItemsData(){ Id = 1, Count = 10, Name = "a", Source = new []{ new ItemsSource() { CustomName = "BoxA", Id = 1, Count = 10 } }.ToList() },
                new ItemsData(){ Id = 2, Count = 20, Name = "b", Source = new []{ new ItemsSource() { CustomName = "BoxB", Id = 2, Count = 20 } }.ToList()  },
                new ItemsData(){ Id = 3, Count = 30, Name = "c", Source = new []{ new ItemsSource() { CustomName = "BoxC", Id = 3, Count = 30 } }.ToList()  },
            };
            var lcdMod = new EmpyrionScripting();
            Assert.AreEqual(
                "Yes:1 #10",
                lcdMod.ExecuteHandlebarScript(lcdData, "{{#items E.S 'BoxA'}}{{test Id leq 2}}Yes:{{Name}} #{{Count}}{{else}}No:{{Name}}{{/test}}{{/items}}")
            );
        }

        [TestMethod]
        public void TestMethodItemsPartial()
        {
            var lcdData = Mock.Of<ScriptRootData>();
            lcdData.E = Mock.Of<EntityData>();
            lcdData.E.S = Mock.Of<StructureData>();
            lcdData.E.S.AllCustomDeviceNames = new[] { "BoxA", "BoxB", "BoxC", "BoxX" };
            lcdData.E.S.Items = new[] {
                new ItemsData(){ Id = 1, Count = 11, Name = "a", Source = new []{ new ItemsSource() { CustomName = "BoxA", Id = 1, Count = 10 }, new ItemsSource() { CustomName = "BoxX", Id = 1, Count = 1 } }.ToList() },
                new ItemsData(){ Id = 2, Count = 20, Name = "b", Source = new []{ new ItemsSource() { CustomName = "BoxB", Id = 2, Count = 20 } }.ToList()  },
                new ItemsData(){ Id = 3, Count = 30, Name = "c", Source = new []{ new ItemsSource() { CustomName = "BoxC", Id = 3, Count = 30 } }.ToList()  },
            };
            var lcdMod = new EmpyrionScripting();
            Assert.AreEqual(
                "Yes:1 #10",
                lcdMod.ExecuteHandlebarScript(lcdData, "{{#items E.S 'BoxA'}}{{test Id leq 2}}Yes:{{Name}} #{{Count}}{{else}}No:{{Name}}{{/test}}{{/items}}")
            );
        }

        [TestMethod]
        public void TestMethodColor()
        {
            var lcdData = Mock.Of<ScriptRootData>();
            var lcdMod = new EmpyrionScripting();
            lcdMod.ExecuteHandlebarScript(lcdData, "{{color @root 'ff00ff'}}");
            Assert.AreEqual(new UnityEngine.Color(255,0,255), lcdData.Color);
        }

        [TestMethod]
        public void TestMethodTestINList()
        {
            var lcdMod = new EmpyrionScripting();
            Assert.AreEqual(
                "Yes:aYes:bNo:c",
                lcdMod.ExecuteHandlebarScript(new
                {
                    I = new[] {
                    new Item(){ Id = 1, Name = "a"},
                    new Item(){ Id = 2, Name = "b"},
                    new Item(){ Id = 3, Name = "c"},
                }
                }, "{{#each I}}{{test Id in '1,2'}}Yes:{{Name}}{{else}}No:{{Name}}{{/test}}{{/each}}")
            );
        }

        [TestMethod]
        public void TestMethodTestINRange()
        {
            var lcdMod = new EmpyrionScripting();
            Assert.AreEqual(
                "Yes:aYes:bNo:c",
                lcdMod.ExecuteHandlebarScript(new
                {
                    I = new[] {
                    new Item(){ Id = 1, Name = "a"},
                    new Item(){ Id = 2, Name = "b"},
                    new Item(){ Id = 3, Name = "c"},
                }
                }, "{{#each I}}{{test Id in '1-2'}}Yes:{{Name}}{{else}}No:{{Name}}{{/test}}{{/each}}")
            );
        }

        [TestMethod]
        public void TestMethodTestINRangeNeg1()
        {
            var lcdMod = new EmpyrionScripting();
            Assert.AreEqual(
                "Yes:aYes:bNo:c",
                lcdMod.ExecuteHandlebarScript(new
                {
                    I = new[] {
                    new Item(){ Id = 1, Name = "a"},
                    new Item(){ Id = 2, Name = "b"},
                    new Item(){ Id = 3, Name = "c"},
                }
                }, "{{#each I}}{{test Id in '-1-2'}}Yes:{{Name}}{{else}}No:{{Name}}{{/test}}{{/each}}")
            );
        }

        [TestMethod]
        public void TestMethodTestINRangeNeg2()
        {
            var lcdMod = new EmpyrionScripting();
            Assert.AreEqual(
                "No:aNo:bNo:c",
                lcdMod.ExecuteHandlebarScript(new
                {
                    I = new[] {
                    new Item(){ Id = 1, Name = "a"},
                    new Item(){ Id = 2, Name = "b"},
                    new Item(){ Id = 3, Name = "c"},
                }
                }, "{{#each I}}{{test Id in '-1--2'}}Yes:{{Name}}{{else}}No:{{Name}}{{/test}}{{/each}}")
            );
        }

        [TestMethod]
        public void TestMethodTestDateTime()
        {
            var lcdMod = new EmpyrionScripting();
            Assert.AreEqual(
                DateTime.Now.ToString(),
                lcdMod.ExecuteHandlebarScript("", "{{datetime}}")
            );
        }

        [TestMethod]
        public void TestMethodFormatPercentPos()
        {
            var lcdMod = new EmpyrionScripting();
            Assert.AreEqual(
                "50,0%",
                lcdMod.ExecuteHandlebarScript(0.5, "{{format . '{0:P1}'}}")
            );
        }

        [TestMethod]
        public void TestMethodFormatPercentNeg()
        {
            var lcdMod = new EmpyrionScripting();
            Assert.AreEqual(
                "-50,0%",
                lcdMod.ExecuteHandlebarScript(-0.5, "{{format . '{0:P1}'}}")
            );
        }

        [TestMethod]
        public void TestMethodTestDateTimeMESZ()
        {
            var lcdMod = new EmpyrionScripting();
            Assert.AreEqual(
                DateTime.Now.ToString(),
                lcdMod.ExecuteHandlebarScript("", "{{datetime MESZ}}")
            );
        }

        [TestMethod]
        public void TestMethodTestDateTimeMESZFormat()
        {
            var lcdMod = new EmpyrionScripting();
            Assert.AreEqual(
                DateTime.Now.ToString("dd MMM HH:mm:ss"),
                lcdMod.ExecuteHandlebarScript("", "{{datetime 'dd MMM HH:mm:ss'}}")
            );
        }

        [TestMethod]
        public void TestMethodUnicodeEscape()
        {
            var lcdMod = new EmpyrionScripting();
            Assert.AreEqual(
                "äöü äöü",
                lcdMod.ExecuteHandlebarScript("äöü", "äöü {{this}}")
            );
        }

        [TestMethod]
        public void TestMethodReadItemsInfo()
        {
            var localization = new Localization(@"C:\steamcmd\empyrion\Content");
            var items        = new ItemInfos   (@"C:\steamcmd\empyrion\Content", localization).ItemInfo;
        }

        [TestMethod]
        public void TestMethodTestScroll()
        {
            var lcdMod = new EmpyrionScripting();
            var outtext = lcdMod.ExecuteHandlebarScript("", "{{#scroll 5 1}}\nLine 1\nLine 2\nLine 3\nLine 4\nLine 5\nLine 6\nLine 7\nLine 8\nLine 9\nLine 10\nLine 11\nLine 12\nLine 13\n{{/scroll}}");
        }

        [TestMethod]
        public void TestMethodItemsMove()
        {
            var boxA = new Mock<IContainer>();
            boxA.Setup(i => i.RemoveItems(1, int.MaxValue)).Returns(10);
            boxA.Setup(i => i.AddItems(1, 1)).Returns(0);

            var boxB = new Mock<IContainer>();
            boxB.Setup(i => i.AddItems(1, 10)).Returns(1);

            var es = new Mock<IStructure>();
            es.Setup(i => i.GetDevice<IContainer>("BoxB")).Returns(boxB.Object);

            var s = new Mock<StructureData>();
            s.Object.AllCustomDeviceNames = new[] { "BoxA", "BoxB", "BoxC", "BoxX" };
            s.Object.Items = new[] {
                new ItemsData(){ Id = 1, Count = 11, Name = "a", Source = new []{ new ItemsSource() { CustomName = "BoxA", Id = 1, Count = 10, Container = boxA.Object } }.ToList() },
                new ItemsData(){ Id = 2, Count = 20, Name = "b", Source = new []{ new ItemsSource() { CustomName = "BoxB", Id = 2, Count = 20, Container = boxB.Object } }.ToList() },
                new ItemsData(){ Id = 3, Count = 30, Name = "c", Source = new []{ new ItemsSource() { CustomName = "BoxC", Id = 3, Count = 30 } }.ToList()  },
            };
            s.Setup(i => i.GetCurrent()).Returns(es.Object);

            var lcdData = Mock.Of<ScriptRootData>();
            lcdData.E = Mock.Of<EntityData>();
            lcdData.E.S = s.Object;
            var lcdMod = new EmpyrionScripting();
            Assert.AreEqual("", lcdMod.ExecuteHandlebarScript(lcdData, "{{#items E.S 'BoxA'}}{{move this ../E.S 'BoxB'}}{{/move}}{{/items}}"));
        }

        [TestMethod]
        public void TestMethodItemsMoveWithSameBox()
        {
            var e = new Mock<EntityData>();
            e.Setup(i => i.Name).Returns("CVa");

            var boxA = new Mock<IContainer>();
            boxA.Setup(i => i.RemoveItems(1, int.MaxValue)).Returns(10);
            boxA.Setup(i => i.AddItems(1, 1)).Returns(0);
            boxA.Setup(i => i.AddItems(1, 10)).Throws(new Exception("in the same box"));

            var boxB = new Mock<IContainer>();
            boxB.Setup(i => i.AddItems(1, 10)).Returns(1);

            var es = new Mock<IStructure>();
            es.Setup(i => i.GetDevice<IContainer>("BoxA")).Returns(boxA.Object);
            es.Setup(i => i.GetDevice<IContainer>("BoxB")).Returns(boxB.Object);

            var s = new Mock<StructureData>();
            s.Object.AllCustomDeviceNames = new[] { "BoxA", "BoxB", "BoxC", "BoxX" };
            s.Object.Items = new[] {
                new ItemsData(){ Id = 1, Count = 11, Name = "a", Source = new []{ new ItemsSource() { CustomName = "BoxA", Id = 1, Count = 10, Container = boxA.Object, E = e.Object } }.ToList() },
                new ItemsData(){ Id = 2, Count = 20, Name = "b", Source = new []{ new ItemsSource() { CustomName = "BoxB", Id = 2, Count = 20, Container = boxB.Object, E = e.Object } }.ToList() },
                new ItemsData(){ Id = 3, Count = 30, Name = "c", Source = new []{ new ItemsSource() { CustomName = "BoxC", Id = 3, Count = 30 } }.ToList()  },
            };
            s.Setup(i => i.GetCurrent()).Returns(es.Object);
            s.Setup(i => i.E).Returns(e.Object);

            var lcdData = Mock.Of<ScriptRootData>();

            lcdData.E = e.Object;
            lcdData.E.S = s.Object;
            var lcdMod = new EmpyrionScripting();
            Assert.AreEqual("1#9 CVa:BoxA->CVa:BoxB", lcdMod.ExecuteHandlebarScript(lcdData, "{{#items E.S 'BoxA'}}{{move this ../E.S 'Box*'}}{{Id}}#{{Count}} {{SourceE.Name}}:{{Source}}->{{DestinationE.Name}}:{{Destination}}{{/move}}{{/items}}"));
        }

        [TestMethod]
        public void TestMethodItemsMoveToOtherEntity()
        {
            // SV ==================================================================
            var SVboxA = new Mock<IContainer>();
            SVboxA.Setup(i => i.RemoveItems(1, int.MaxValue)).Returns(10);
            SVboxA.Setup(i => i.AddItems(1, 1)).Returns(0);
            SVboxA.Setup(i => i.AddItems(1, 10)).Throws(new Exception("in the same box"));

            var SVboxB = new Mock<IContainer>();
            SVboxB.Setup(i => i.AddItems(1, 10)).Returns(1);

            var esSV = new Mock<IStructure>();
            esSV.Setup(i => i.GetDevice<IContainer>("BoxA")).Returns(SVboxA.Object);
            esSV.Setup(i => i.GetDevice<IContainer>("BoxB")).Returns(SVboxB.Object);

            var eeSV = new Mock<IEntity>();
            eeSV.Setup(i => i.Structure).Returns(esSV.Object);

            var eSV = new Mock<EntityData>();
            eSV.Setup(i => i.Name).Returns("SVa");
            eSV.Setup(i => i.GetCurrent()).Returns(eeSV.Object);

            var sSV = new Mock<StructureData>();
            sSV.Object.AllCustomDeviceNames = new[] { "BoxA", "BoxB", "BoxC", "BoxX" };
            sSV.Object.Items = new[] {
                new ItemsData(){ Id = 1, Count = 11, Name = "a", Source = new []{ new ItemsSource() { CustomName = "BoxA", Id = 1, Count = 10, Container = SVboxA.Object, E = eSV.Object } }.ToList() },
                new ItemsData(){ Id = 2, Count = 20, Name = "b", Source = new []{ new ItemsSource() { CustomName = "BoxB", Id = 2, Count = 20, Container = SVboxB.Object, E = eSV.Object } }.ToList() },
                new ItemsData(){ Id = 3, Count = 30, Name = "c", Source = new []{ new ItemsSource() { CustomName = "BoxC", Id = 3, Count = 30 } }.ToList()  },
            };
            sSV.Setup(i => i.GetCurrent()).Returns(esSV.Object);
            sSV.Setup(i => i.E).Returns(eSV.Object);
            eSV.Object.S = sSV.Object;

            // CV ==================================================================
            var CVboxA = new Mock<IContainer>();
            CVboxA.Setup(i => i.RemoveItems(1, int.MaxValue)).Returns(10);
            CVboxA.Setup(i => i.AddItems(1, 1)).Returns(0);
            CVboxA.Setup(i => i.AddItems(1, 10)).Throws(new Exception("in the same box"));

            var CVboxB = new Mock<IContainer>();
            CVboxB.Setup(i => i.AddItems(1, 10)).Returns(1);

            var esCV = new Mock<IStructure>();
            esCV.Setup(i => i.GetDevice<IContainer>("BoxA")).Returns(CVboxA.Object);
            esCV.Setup(i => i.GetDevice<IContainer>("BoxB")).Returns(CVboxB.Object);

            var eeCV = new Mock<IEntity>();
            eeCV.Setup(i => i.Structure).Returns(esCV.Object);

            var eCV = new Mock<EntityData>();
            eCV.Setup(i => i.Name).Returns("CVb");
            eCV.Setup(i => i.GetCurrent()).Returns(eeCV.Object);

            var sCV = new Mock<StructureData>();
            sCV.Object.AllCustomDeviceNames = new[] { "BoxA", "BoxB", "BoxC", "BoxX" };
            sCV.Object.Items = new[] {
                new ItemsData(){ Id = 1, Count = 11, Name = "a", Source = new []{ new ItemsSource() { CustomName = "BoxA", Id = 1, Count = 10, Container = CVboxA.Object, E = eCV.Object } }.ToList() },
                new ItemsData(){ Id = 2, Count = 20, Name = "b", Source = new []{ new ItemsSource() { CustomName = "BoxB", Id = 2, Count = 20, Container = CVboxB.Object, E = eCV.Object } }.ToList() },
                new ItemsData(){ Id = 3, Count = 30, Name = "c", Source = new []{ new ItemsSource() { CustomName = "BoxC", Id = 3, Count = 30 } }.ToList()  },
            };
            sCV.Setup(i => i.GetCurrent()).Returns(esCV.Object);
            sCV.Setup(i => i.E).Returns(eCV.Object);
            eCV.Object.S = sCV.Object;

            sCV.Object.DockedE = new[] { eSV.Object };

            // MOVE ==================================================================

            var lcdData = Mock.Of<ScriptRootData>();

            lcdData.E   = eCV.Object;
            lcdData.E.S = sCV.Object;
            var lcdMod = new EmpyrionScripting();
            Assert.AreEqual("SVa:1#9 SVa:BoxA->CVb:BoxB", lcdMod.ExecuteHandlebarScript(lcdData, "{{#each E.S.DockedE}}{{Name}}:{{#items S 'BoxA'}}{{move this @root/E.S 'Box*'}}{{Id}}#{{Count}} {{SourceE.Name}}:{{Source}}->{{DestinationE.Name}}:{{Destination}}{{/move}}{{/items}}{{/each}}"));
        }

        [TestMethod]
        public void TestMethodSaveGamesScripts()
        {
            var lcdMod = new EmpyrionScripting
            {
                SaveGameModPath = @"C:\steamcmd\empyrion\PlayfieldServer/../Saves/Games/Test\Mods\EmpyrionScripting"
            };
            lcdMod.SaveGamesScripts.ReadSaveGamesScripts();

            lcdMod.ExecFoundSaveGameScripts(new ScriptSaveGameRootData(null, null, null), Path.Combine(lcdMod.SaveGamesScripts.MainScriptPath, "CV"));
        }

        [TestMethod]
        public void TestMethodSaveGamesScriptsRoot()
        {
            var lcdData = Mock.Of<ScriptSaveGameRootData>();
            lcdData.MainScriptPath = "abc";
            var lcdMod = new EmpyrionScripting();
            Assert.AreEqual(
                "abc",
                lcdMod.ExecuteHandlebarScript(lcdData, "{{MainScriptPath}}")
            );
        }

        [TestMethod]
        public void TestMethodFileCache()
        {
            var fg = HelpersTools.GetFileContent(@"C:\steamcmd\empyrion\Saves\Games\Test\Mods\EmpyrionScripting\Codes\x\..\Codes.txt");
        }
    }
}
