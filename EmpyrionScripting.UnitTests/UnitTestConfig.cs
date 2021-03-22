using System;
using System.Collections.Generic;
using System.Linq;
using EcfParser;
using EmpyrionScripting;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EmpyrionLCDInfo.UnitTests
{
    [TestClass]
    public class UnitTestEcfTemplates
    {
        [TestMethod]
        public void TestMethodConfigTemplates()
        {
            var config = new ConfigEcfAccess();
            //config.ReadConfigEcf(@"C:\steamcmd\empyrion\Content", null, null, null);
            config.ReadConfigEcf(@"C:\steamcmd\empyrion\Content", "Reforged Eden", @"C:\steamcmd\empyrion.server\Saves\Games\Default\blocksmap.dat", null);
            var templates = new Dictionary<int, Dictionary<int, int>>();

            config.FlatConfigBlockById
                .ForEach(B => {
                    var idCfg = B.Value.Attr.FirstOrDefault(A => A.Name == "Id");
                    if (!int.TryParse(idCfg?.Value?.ToString(), out var id)) return;

                    var ressList = new Dictionary<int, int>();
                    var templateRoot = B.Value.Attr.FirstOrDefault(A => A.Name == "TemplateRoot")?.Value?.ToString() ??
                                       idCfg.AddOns?.FirstOrDefault(A => A.Key == "Name").Value?.ToString();
                    if (string.IsNullOrEmpty(templateRoot)) return;
                    if (!config.FlatConfigTemplatesByName.TryGetValue(templateRoot, out var templateRootBlock)) return;

                    ScanTemplates(config, templateRootBlock, ressList);

                    if (ressList.Count > 0) templates.Add(id, ressList);
                });

            Console.WriteLine(templates.Count);
        }

        private void ScanTemplates(ConfigEcfAccess config, EcfBlock templateRootBlock, Dictionary<int, int> ressList)
        {
            var templateName = templateRootBlock.Attr.FirstOrDefault(A => A.Name == "Name")?.Value.ToString();
            bool.TryParse(templateRootBlock.Attr.FirstOrDefault(A => A.Name == "BaseItem")?.Value.ToString(), out var isBaseItem);

            templateRootBlock.Childs?
                .FirstOrDefault(C => C.Key == "Child Inputs").Value?.Attr?
                .ForEach(C => {

                    if (C.Name.ToString() == templateName) return;

                    if (!isBaseItem && config.FlatConfigTemplatesByName.TryGetValue(C.Name.ToString(), out var recipe))
                    {
                        bool.TryParse(recipe.Attr.FirstOrDefault(A => A.Name == "BaseItem")?.Value.ToString(), out var isSubBaseItem);
                        if (!isSubBaseItem)
                        {
                            ScanTemplates(config, recipe, ressList);
                            return;
                        }
                    }

                    if (!config.FlatConfigBlockByName.TryGetValue(C.Name.ToString(), out var ressource)) return;
                    if (!int.TryParse(ressource.Attr.FirstOrDefault(A => A.Name == "Id")?.Value.ToString(), out var ressId)) return;

                    if (ressList.TryGetValue(ressId, out var count)) ressList[ressId] = count + (int)C.Value;
                    else ressList.Add(ressId, (int)C.Value);
                });
        }
    }
}
