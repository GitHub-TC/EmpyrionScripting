using System;
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
                lcdMod.RenderLCDText(new { I = new[] {
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
                lcdMod.RenderLCDText(new
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
            var lcdData = Mock.Of<LCDData>();
            lcdData.S = Mock.Of<StructureData>();
            lcdData.S.Items = new[] {
                new ItemsData(){ Id = 1, Count = 10, Name = "a" },
                new ItemsData(){ Id = 2, Count = 20, Name = "b" },
                new ItemsData(){ Id = 3, Count = 30, Name = "c" },
            };
            var lcdMod = new EmpyrionScripting();
            Assert.AreEqual(
                "Yes:aYes:bNo:c",
                lcdMod.RenderLCDText(lcdData, "{{#each S.Items}}{{test Id leq 2}}Yes:{{Name}}{{else}}No:{{Name}}{{/test}}{{/each}}")
            );
        }

        [TestMethod]
        public void TestMethodTestINList()
        {
            var lcdMod = new EmpyrionScripting();
            Assert.AreEqual(
                "Yes:aYes:bNo:c",
                lcdMod.RenderLCDText(new
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
                lcdMod.RenderLCDText(new
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
                lcdMod.RenderLCDText(new
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
                lcdMod.RenderLCDText(new
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
                lcdMod.RenderLCDText("", "{{datetime}}")
            );
        }

        [TestMethod]
        public void TestMethodTestDateTimeMESZ()
        {
            var lcdMod = new EmpyrionScripting();
            Assert.AreEqual(
                DateTime.Now.ToString(),
                lcdMod.RenderLCDText("", "{{datetime MESZ}}")
            );
        }

        [TestMethod]
        public void TestMethodTestDateTimeMESZFormat()
        {
            var lcdMod = new EmpyrionScripting();
            Assert.AreEqual(
                DateTime.Now.ToString("dd MMM HH:mm:ss"),
                lcdMod.RenderLCDText("", "{{datetime MESZ 'dd MMM HH:mm:ss'}}")
            );
        }

        [TestMethod]
        public void TestMethodReadItemsInfo()
        {
            var localization = new Localization(@"C:\steamcmd\empyrion\Content");
            var items        = new ItemInfos   (@"C:\steamcmd\empyrion\Content", localization).ItemInfo;
        }

        [TestMethod]
        public void TestMethodExtract()
        {
            var r = "abc\tc\t123456\txyz";
            Assert.AreEqual("123456", EmpyrionScripting.Extract("c", ref r));
            Assert.AreEqual("abcxyz", r);
        }

        [TestMethod]
        public void TestMethodTestScroll()
        {
            var lcdMod = new EmpyrionScripting();
            var outtext = lcdMod.RenderLCDText("", "{{#scroll 5 1}}\nLine 1\nLine 2\nLine 3\nLine 4\nLine 5\nLine 6\nLine 7\nLine 8\nLine 9\nLine 10\nLine 11\nLine 12\nLine 13\n{{/scroll}}");
        }
    }
}
