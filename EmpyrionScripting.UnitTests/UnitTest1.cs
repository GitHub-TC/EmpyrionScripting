using System;
using System.Linq;
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
    }
}
