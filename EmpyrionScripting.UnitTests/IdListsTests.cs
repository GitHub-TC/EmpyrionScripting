using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;

namespace EmpyrionScripting.Tests
{
    [TestClass]
    public class IdListsTests
    {
        [TestMethod]
        public void ProcessListsTest1()
        {
            var ids = new IdLists
            {
                BlockIdMapping = new Dictionary<string, int>()
                {
                    ["a"] = 1,
                    ["b"] = 2,
                    ["c"] = 3,
                    ["x"] = 10,
                    ["y"] = -10,
                },
                IdBlockMapping = new Dictionary<int, string>()
                {
                    [1] = "a",
                    [2] = "b",
                    [3] = "c",
                    [10] = "x",
                    [-10] = "y",
                }
            };

            ids.ProcessLists(new Dictionary<string, string>() { ["test"] = "1,2,3,10,-10" });

            Assert.AreEqual(",-10,1-3,10,", ids.MappedIds["test"]);
            Assert.AreEqual(",y,a,b,c,x,",    ids.NamedIds ["test"]);
        }

        [TestMethod]
        public void ProcessListsTest2()
        {
            var ids = new IdLists
            {
                BlockIdMapping = new Dictionary<string, int>()
                {
                    ["a"] = 1,
                    ["b"] = 2,
                    ["c"] = 3,
                    ["x"] = 10,
                    ["y"] = -10,
                },
                IdBlockMapping = new Dictionary<int, string>()
                {
                    [1] = "a",
                    [2] = "b",
                    [3] = "c",
                    [10] = "x",
                    [-10] = "y",
                }
            };

            ids.ProcessLists(new Dictionary<string, string>() { ["test"] = "1,b,3,10,-10" });

            Assert.AreEqual(",-10,1-3,10,", ids.MappedIds["test"]);
            Assert.AreEqual(",y,a,b,c,x,", ids.NamedIds["test"]);
        }

        [TestMethod]
        public void ProcessListsTest3()
        {
            var ids = new IdLists
            {
                BlockIdMapping = new Dictionary<string, int>()
                {
                    ["a"] = 1,
                    ["b"] = 2,
                    ["c"] = 3,
                    ["x"] = 10,
                    ["y"] = -10,
                },
                IdBlockMapping = new Dictionary<int, string>()
                {
                    [1] = "a",
                    [2] = "b",
                    [3] = "c",
                    [10] = "x",
                    [-10] = "y",
                }
            };

            ids.ProcessLists(new Dictionary<string, string>() { ["test"] = "1,b-c,10,-10" });

            Assert.AreEqual(",-10,1-3,10,", ids.MappedIds["test"]);
            Assert.AreEqual(",y,a,b,c,x,", ids.NamedIds["test"]);
        }

        [TestMethod]
        public void ProcessListsTest4()
        {
            var ids = new IdLists
            {
                BlockIdMapping = new Dictionary<string, int>()
                {
                    ["a"] = 1,
                    ["b"] = 2,
                    ["c"] = 3,
                    ["x"] = 10,
                    ["y"] = -10,
                },
                IdBlockMapping = new Dictionary<int, string>()
                {
                    [1] = "a",
                    [2] = "b",
                    [3] = "c",
                    [10] = "x",
                    [-10] = "y",
                }
            };

            ids.ProcessLists(new Dictionary<string, string>() { ["test"] = "1,b-c,20,-10" });

            Assert.AreEqual(",-10,1-3,20,", ids.MappedIds["test"]);
            Assert.AreEqual(",y,a,b,c,20,", ids.NamedIds["test"]);
        }

        [TestMethod]
        public void ProcessListsTest5()
        {
            var ids = new IdLists
            {
                BlockIdMapping = new Dictionary<string, int>()
                {
                    ["a"] = 1,
                    ["b"] = 2,
                    ["c"] = 3,
                    ["x"] = 10,
                    ["y"] = -10,
                },
                IdBlockMapping = new Dictionary<int, string>()
                {
                    [1] = "a",
                    [2] = "b",
                    [3] = "c",
                    [10] = "x",
                    [-10] = "y",
                }
            };

            ids.ProcessLists(new Dictionary<string, string>() { ["test"] = "1,b-c,z,-10" });

            Assert.AreEqual(",-10,1-3,", ids.MappedIds["test"]);
            Assert.AreEqual(",y,a,b,c,z,", ids.NamedIds["test"]);
        }

        [TestMethod]
        public void ProcessListsTest6()
        {
            var ids = new IdLists
            {
                BlockIdMapping = new Dictionary<string, int>()
                {
                    ["a"] = 1,
                    ["b"] = 2,
                    ["c"] = 3,
                    ["x"] = 10,
                    ["y"] = -10,
                },
                IdBlockMapping = new Dictionary<int, string>()
                {
                    [1] = "a",
                    [2] = "b",
                    [3] = "c",
                    [10] = "x",
                    [-10] = "y",
                }
            };

            ids.ProcessLists(new Dictionary<string, string>() { ["test"] = ",1,b,z, b-c,z,-10," });

            Assert.AreEqual(",-10,1-3,", ids.MappedIds["test"]);
            Assert.AreEqual(",y,a,b,c,z,", ids.NamedIds["test"]);
        }

    }
}