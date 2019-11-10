using System;
using System.IO;
using System.Linq;
using Eleon.Modding;
using EmpyrionScripting.CustomHelpers;
using EmpyrionScripting.DataWrapper;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;

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
            var pf = new PlayfieldScriptData(lcdMod);
            Assert.AreEqual(
                "Yes:aNo:bNo:c",
                lcdMod.ExecuteHandlebarScript(pf, new { I = new[] {
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
            var pf = new PlayfieldScriptData(lcdMod);
            Assert.AreEqual(
                "Yes:aYes:bNo:c",
                lcdMod.ExecuteHandlebarScript(pf, new
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
            var S = Substitute.For<IStructureData>();
            S.Items.Returns(new[] {
                new ItemsData(){ Id = 1, Count = 10, Name = "a" },
                new ItemsData(){ Id = 2, Count = 20, Name = "b" },
                new ItemsData(){ Id = 3, Count = 30, Name = "c" },
            });

            var E = Substitute.For<IEntityData>();
            E.S.Returns(S);

            var lcdData = Substitute.For<IScriptRootData>();
            lcdData.E.Returns(E);

            var lcdMod = new EmpyrionScripting();
            var pf = new PlayfieldScriptData(lcdMod);
            Assert.AreEqual(
                "Yes:aYes:bNo:c",
                lcdMod.ExecuteHandlebarScript(pf, lcdData, "{{#each E.S.Items}}{{test Id leq 2}}Yes:{{Name}}{{else}}No:{{Name}}{{/test}}{{/each}}")
            );
        }

        [TestMethod]
        public void TestMethodItems()
        {
            var S = Substitute.For<IStructureData>();
            S.AllCustomDeviceNames.Returns(new[] { "BoxA", "BoxB", "BoxC" });
            S.Items.Returns(new[] {
                new ItemsData(){ Id = 1, Count = 10, Name = "a", Source = new []{ new ItemsSource() { CustomName = "BoxA", Id = 1, Count = 10 } }.ToList() },
                new ItemsData(){ Id = 2, Count = 20, Name = "b", Source = new []{ new ItemsSource() { CustomName = "BoxB", Id = 2, Count = 20 } }.ToList()  },
                new ItemsData(){ Id = 3, Count = 30, Name = "c", Source = new []{ new ItemsSource() { CustomName = "BoxC", Id = 3, Count = 30 } }.ToList()  },
            });

            var E = Substitute.For<IEntityData>();
            E.S.Returns(S);

            var lcdData = Substitute.For<IScriptRootData>();
            lcdData.E.Returns(E);

            var lcdMod = new EmpyrionScripting();
            var pf = new PlayfieldScriptData(lcdMod);
            Assert.AreEqual(
                "Yes:1 #10",
                lcdMod.ExecuteHandlebarScript(pf, lcdData, "{{#items E.S 'BoxA'}}{{test Id leq 2}}Yes:{{Name}} #{{Count}}{{else}}No:{{Name}}{{/test}}{{/items}}")
            );
        }

        [TestMethod]
        public void TestMethodItemsPartial()
        {
            var S = Substitute.For<IStructureData>();
            S.AllCustomDeviceNames.Returns(new[] { "BoxA", "BoxB", "BoxC", "BoxX" });
            S.Items.Returns(new[] {
                new ItemsData(){ Id = 1, Count = 11, Name = "a", Source = new []{ new ItemsSource() { CustomName = "BoxA", Id = 1, Count = 10 }, new ItemsSource() { CustomName = "BoxX", Id = 1, Count = 1 } }.ToList() },
                new ItemsData(){ Id = 2, Count = 20, Name = "b", Source = new []{ new ItemsSource() { CustomName = "BoxB", Id = 2, Count = 20 } }.ToList()  },
                new ItemsData(){ Id = 3, Count = 30, Name = "c", Source = new []{ new ItemsSource() { CustomName = "BoxC", Id = 3, Count = 30 } }.ToList()  },
            });

            var E = Substitute.For<IEntityData>();
            E.S.Returns(S);

            var lcdData = Substitute.For<IScriptRootData>();
            lcdData.E.Returns(E);

            var lcdMod = new EmpyrionScripting();
            var pf = new PlayfieldScriptData(lcdMod);
            Assert.AreEqual(
                "Yes:1 #10",
                lcdMod.ExecuteHandlebarScript(pf, lcdData, "{{#items E.S 'BoxA'}}{{test Id leq 2}}Yes:{{Name}} #{{Count}}{{else}}No:{{Name}}{{/test}}{{/items}}")
            );
        }

        [TestMethod]
        public void TestMethodColor()
        {
            var lcdData = Substitute.For<IScriptRootData>();
            var lcdMod = new EmpyrionScripting();
            var pf = new PlayfieldScriptData(lcdMod);
            lcdMod.ExecuteHandlebarScript(pf, lcdData, "{{color 'ff00ff'}}");
            Assert.AreEqual(new UnityEngine.Color(255,0,255), lcdData.Color);
        }

        [TestMethod]
        public void TestMethodTestINList()
        {
            var lcdMod = new EmpyrionScripting();
            var pf = new PlayfieldScriptData(lcdMod);
            Assert.AreEqual(
                "Yes:aYes:bNo:c",
                lcdMod.ExecuteHandlebarScript(pf, new
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
            var pf = new PlayfieldScriptData(lcdMod);
            Assert.AreEqual(
                "Yes:aYes:bNo:c",
                lcdMod.ExecuteHandlebarScript(pf, new
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
            var pf = new PlayfieldScriptData(lcdMod);
            Assert.AreEqual(
                "Yes:aYes:bNo:c",
                lcdMod.ExecuteHandlebarScript(pf, new
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
            var pf = new PlayfieldScriptData(lcdMod);
            Assert.AreEqual(
                "No:aNo:bNo:c",
                lcdMod.ExecuteHandlebarScript(pf, new
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
            var pf = new PlayfieldScriptData(lcdMod);
            Assert.AreEqual(
                DateTime.Now.ToString(),
                lcdMod.ExecuteHandlebarScript(pf, "", "{{datetime}}")
            );
        }

        [TestMethod]
        public void TestMethodFormatPercentPos()
        {
            var lcdMod = new EmpyrionScripting();
            var pf = new PlayfieldScriptData(lcdMod);
            Assert.AreEqual(
                "50,0%",
                lcdMod.ExecuteHandlebarScript(pf, 0.5, "{{format . '{0:P1}'}}")
            );
        }

        [TestMethod]
        public void TestMethodFormatPercentNeg()
        {
            var lcdMod = new EmpyrionScripting();
            var pf = new PlayfieldScriptData(lcdMod);
            Assert.AreEqual(
                "-50,0%",
                lcdMod.ExecuteHandlebarScript(pf, -0.5, "{{format . '{0:P1}'}}")
            );
        }

        [TestMethod]
        public void TestMethodTestDateTimeMESZ()
        {
            var lcdMod = new EmpyrionScripting();
            var pf = new PlayfieldScriptData(lcdMod);
            Assert.AreEqual(
                DateTime.Now.ToString(),
                lcdMod.ExecuteHandlebarScript(pf, "", "{{datetime MESZ}}")
            );
        }

        [TestMethod]
        public void TestMethodTestDateTimeMESZFormat()
        {
            var lcdMod = new EmpyrionScripting();
            var pf = new PlayfieldScriptData(lcdMod);
            Assert.AreEqual(
                DateTime.Now.ToString("dd MMM HH:mm:ss"),
                lcdMod.ExecuteHandlebarScript(pf, "", "{{datetime 'dd MMM HH:mm:ss'}}")
            );
        }

        [TestMethod]
        public void TestMethodUnicodeEscape()
        {
            var lcdMod = new EmpyrionScripting();
            var pf = new PlayfieldScriptData(lcdMod);
            Assert.AreEqual(
                "äöü äöü",
                lcdMod.ExecuteHandlebarScript(pf, "äöü", "äöü {{this}}")
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
            var pf = new PlayfieldScriptData(lcdMod);
            var outtext = lcdMod.ExecuteHandlebarScript(pf, "", "{{#scroll 5 1}}\nLine 1\nLine 2\nLine 3\nLine 4\nLine 5\nLine 6\nLine 7\nLine 8\nLine 9\nLine 10\nLine 11\nLine 12\nLine 13\n{{/scroll}}");
        }

        [TestMethod]
        public void TestMethodItemsMove()
        {
            ConveyorHelpers.CreateDeviceLock = (R, P, S, V) => new MockDeviceLock();

            var boxA = Substitute.For<IContainer>();
            boxA.RemoveItems(1, int.MaxValue).Returns(10);
            boxA.AddItems(1, 1).Returns(0);

            var boxB = Substitute.For<IContainer>();
            boxB.AddItems(1, 10).Returns(1);

            var es = Substitute.For<IStructure>();
            es.GetDevice<IContainer>("BoxB").Returns(boxB);

            var s = Substitute.For<IStructureData>();
            s.AllCustomDeviceNames.Returns(new[] { "BoxA", "BoxB", "BoxC", "BoxX" });
            s.Items.Returns(new[] {
                new ItemsData(){ Id = 1, Count = 11, Name = "a", Source = new []{ new ItemsSource() { CustomName = "BoxA", Id = 1, Count = 10, Container = boxA } }.ToList() },
                new ItemsData(){ Id = 2, Count = 20, Name = "b", Source = new []{ new ItemsSource() { CustomName = "BoxB", Id = 2, Count = 20, Container = boxB } }.ToList() },
                new ItemsData(){ Id = 3, Count = 30, Name = "c", Source = new []{ new ItemsSource() { CustomName = "BoxC", Id = 3, Count = 30 } }.ToList()  },
            });
            s.ContainerSource.Returns(new System.Collections.Concurrent.ConcurrentDictionary<string, ContainerSource>() {
                ["BoxA"] = new ContainerSource() { Container = boxA, CustomName = "BoxA"},
                ["BoxB"] = new ContainerSource() { Container = boxB, CustomName = "BoxB"},
            });

            s.GetCurrent().Returns(es);

            var E = Substitute.For<IEntityData>();
            E.S.Returns(s);

            var lcdData = Substitute.For<IScriptRootData>();
            lcdData.E.Returns(E);

            var lcdMod = new EmpyrionScripting();
            var pf = new PlayfieldScriptData(lcdMod);
            Assert.AreEqual("9-1", lcdMod.ExecuteHandlebarScript(pf, lcdData, "{{#items E.S 'BoxA'}}{{move this ../E.S 'BoxB'}}{{Count}}-{{Id}}{{/move}}{{/items}}"));
        }

        class MockDeviceLock : IDeviceLock
        {
            public bool Success => true;

            public void Dispose()
            {
            }
        }

        [TestMethod]
        public void TestMethodItemsMoveWithSameBox()
        {
            ConveyorHelpers.CreateDeviceLock = (R, P, S, V) => new MockDeviceLock();

            var e = Substitute.For<IEntityData>();
            e.Name.Returns("CVa");

            var boxA = Substitute.For<IContainer>();
            boxA.RemoveItems(1, int.MaxValue).Returns(10);
            boxA.AddItems(1, 1).Returns(0);
            boxA.AddItems(1, 10).Returns(C => throw new Exception("in the same box"));

            var boxB = Substitute.For<IContainer>();
            boxB.AddItems(1, 10).Returns(1);

            var es = Substitute.For<IStructure>();
            es.GetDevice<IContainer>("BoxA").Returns(boxA);
            es.GetDevice<IContainer>("BoxB").Returns(boxB);

            var s = Substitute.For<IStructureData>();
            s.AllCustomDeviceNames.Returns(new[] { "BoxA", "BoxB", "BoxC", "BoxX" });
            s.Items.Returns(new[] {
                new ItemsData(){ Id = 1, Count = 11, Name = "a", Source = new []{ new ItemsSource() { CustomName = "BoxA", Id = 1, Count = 10, Container = boxA, E = e } }.ToList() },
                new ItemsData(){ Id = 2, Count = 20, Name = "b", Source = new []{ new ItemsSource() { CustomName = "BoxB", Id = 2, Count = 20, Container = boxB, E = e } }.ToList() },
                new ItemsData(){ Id = 3, Count = 30, Name = "c", Source = new []{ new ItemsSource() { CustomName = "BoxC", Id = 3, Count = 30 } }.ToList()  },
            });
            s.ContainerSource.Returns(new System.Collections.Concurrent.ConcurrentDictionary<string, ContainerSource>()
            {
                ["BoxA"] = new ContainerSource() { Container = boxA, CustomName = "BoxA" },
                ["BoxB"] = new ContainerSource() { Container = boxB, CustomName = "BoxB" },
            });
            s.GetCurrent().Returns(es);
            s.E.Returns(e);

            var lcdData = Substitute.For<IScriptRootData>();

            lcdData.E.Returns(e);
            lcdData.E.S.Returns(s);
            var lcdMod = new EmpyrionScripting();
            var pf = new PlayfieldScriptData(lcdMod);
            Assert.AreEqual("1#9 CVa:BoxA->CVa:BoxB", lcdMod.ExecuteHandlebarScript(pf, lcdData, "{{#items E.S 'BoxA'}}{{move this ../E.S 'Box*'}}{{Id}}#{{Count}} {{SourceE.Name}}:{{Source}}->{{DestinationE.Name}}:{{Destination}}{{/move}}{{/items}}"));
        }

        [TestMethod]
        public void TestMethodItemsMoveToOtherEntity()
        {
            ConveyorHelpers.CreateDeviceLock = (R, P, S, V) => new MockDeviceLock();
            
            // SV ==================================================================
            var SVboxA = Substitute.For<IContainer>();
            SVboxA.RemoveItems(1, int.MaxValue).Returns(10);
            SVboxA.AddItems(1, 1).Returns(0);
            SVboxA.AddItems(1, 10).Returns(C => throw new Exception("in the same box"));

            var SVboxB = Substitute.For<IContainer>();
            SVboxB.AddItems(1, 10).Returns(1);

            var esSV = Substitute.For<IStructure>();
            esSV.GetDevice<IContainer>("BoxA").Returns(SVboxA);
            esSV.GetDevice<IContainer>("BoxB").Returns(SVboxB);

            var eeSV = Substitute.For<IEntity>();
            eeSV.Structure.Returns(esSV);

            var eSV = Substitute.For<IEntityData>();
            eSV.Name.Returns("SVa");
            eSV.GetCurrent().Returns(eeSV);

            var sSV = Substitute.For<IStructureData>();
            sSV.AllCustomDeviceNames.Returns(new[] { "BoxA", "BoxB", "BoxC", "BoxX" });
            sSV.Items.Returns(new[] {
                new ItemsData(){ Id = 1, Count = 11, Name = "a", Source = new []{ new ItemsSource() { CustomName = "BoxA", Id = 1, Count = 10, Container = SVboxA, E = eSV } }.ToList() },
                new ItemsData(){ Id = 2, Count = 20, Name = "b", Source = new []{ new ItemsSource() { CustomName = "BoxB", Id = 2, Count = 20, Container = SVboxB, E = eSV } }.ToList() },
                new ItemsData(){ Id = 3, Count = 30, Name = "c", Source = new []{ new ItemsSource() { CustomName = "BoxC", Id = 3, Count = 30 } }.ToList()  },
            });
            sSV.ContainerSource.Returns(new System.Collections.Concurrent.ConcurrentDictionary<string, ContainerSource>()
            {
                ["BoxA"] = new ContainerSource() { Container = SVboxA, CustomName = "BoxA" },
                ["BoxB"] = new ContainerSource() { Container = SVboxB, CustomName = "BoxB" },
            });
            sSV.GetCurrent().Returns(esSV);
            sSV.E.Returns(eSV);
            eSV.S.Returns(sSV);

            // CV ==================================================================
            var CVboxA = Substitute.For<IContainer>();
            CVboxA.RemoveItems(1, int.MaxValue).Returns(10);
            CVboxA.AddItems(1, 1).Returns(0);
            CVboxA.AddItems(1, 10).Returns(C => throw new Exception("in the same box"));

            var CVboxB = Substitute.For<IContainer>();
            CVboxB.AddItems(1, 10).Returns(1);

            var esCV = Substitute.For<IStructure>();
            esCV.GetDevice<IContainer>("BoxA").Returns(CVboxA);
            esCV.GetDevice<IContainer>("BoxB").Returns(CVboxB);

            var eeCV = Substitute.For<IEntity>();
            eeCV.Structure.Returns(esCV);

            var eCV = Substitute.For<IEntityData>();
            eCV.Name.Returns("CVb");
            eCV.GetCurrent().Returns(eeCV);

            var sCV = Substitute.For<IStructureData>();
            sCV.AllCustomDeviceNames.Returns(new[] { "BoxA", "BoxB", "BoxC", "BoxX" });
            sCV.Items.Returns(new[] {
                new ItemsData(){ Id = 1, Count = 11, Name = "a", Source = new []{ new ItemsSource() { CustomName = "BoxA", Id = 1, Count = 10, Container = CVboxA, E = eCV } }.ToList() },
                new ItemsData(){ Id = 2, Count = 20, Name = "b", Source = new []{ new ItemsSource() { CustomName = "BoxB", Id = 2, Count = 20, Container = CVboxB, E = eCV } }.ToList() },
                new ItemsData(){ Id = 3, Count = 30, Name = "c", Source = new []{ new ItemsSource() { CustomName = "BoxC", Id = 3, Count = 30 } }.ToList()  },
            });
            sCV.ContainerSource.Returns(new System.Collections.Concurrent.ConcurrentDictionary<string, ContainerSource>()
            {
                ["BoxA"] = new ContainerSource() { Container = CVboxA, CustomName = "BoxA" },
                ["BoxB"] = new ContainerSource() { Container = CVboxB, CustomName = "BoxB" },
            });
            sCV.GetCurrent().Returns(esCV);
            sCV.E.Returns(eCV);
            eCV.S.Returns(sCV);

            sCV.DockedE.Returns(new[] { eSV });

            // MOVE ==================================================================

            var lcdData = Substitute.For<IScriptRootData>();

            lcdData.E.Returns(eCV);
            lcdData.E.S.Returns(sCV);
            var lcdMod = new EmpyrionScripting();
            var pf = new PlayfieldScriptData(lcdMod);
            Assert.AreEqual("SVa:1#9 SVa:BoxA->CVb:BoxB", lcdMod.ExecuteHandlebarScript(pf, lcdData, "{{#each E.S.DockedE}}{{Name}}:{{#items S 'BoxA'}}{{move this @root/E.S 'Box*'}}{{Id}}#{{Count}} {{SourceE.Name}}:{{Source}}->{{DestinationE.Name}}:{{Destination}}{{/move}}{{/items}}{{/each}}"));
        }

        [TestMethod]
        public void TestMethodSaveGamesScripts()
        {
            var lcdMod = new EmpyrionScripting
            {
                SaveGameModPath = @"C:\steamcmd\empyrion\PlayfieldServer/../Saves/Games/Test\Mods\EmpyrionScripting"
            };
            lcdMod.SaveGamesScripts = new SaveGamesScripts(null) { SaveGameModPath = lcdMod.SaveGameModPath };
            lcdMod.SaveGamesScripts.ReadSaveGamesScripts();

            var pf = new PlayfieldScriptData(lcdMod);
            lcdMod.ExecFoundSaveGameScripts(pf, new ScriptSaveGameRootData(pf, null, null, null, null, null), Path.Combine(lcdMod.SaveGamesScripts.MainScriptPath, "CV"));
        }

        [TestMethod]
        public void TestMethodSaveGamesScriptsRoot()
        {
            var lcdData = Substitute.For<ScriptSaveGameRootData>();
            lcdData.MainScriptPath = "abc";
            var lcdMod = new EmpyrionScripting();
            var pf = new PlayfieldScriptData(lcdMod);
            Assert.AreEqual(
                "abc",
                lcdMod.ExecuteHandlebarScript(pf, lcdData, "{{MainScriptPath}}")
            );
        }

        [TestMethod]
        public void TestMethodFileCache()
        {
            var fg = HelpersTools.GetFileContent(@"C:\steamcmd\empyrion\Saves\Games\Test\Mods\EmpyrionScripting\Codes\x\..\Codes.txt");
        }
    }
}
