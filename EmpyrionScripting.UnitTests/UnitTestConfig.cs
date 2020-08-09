using System;
using System.Collections.Generic;
using System.Linq;
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
            config.ReadConfigEcf(@"C:\steamcmd\empyrion\Content");
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

                    templateRootBlock.Childs?
                        .FirstOrDefault(C => C.Key == "Child Inputs").Value?.Attr?
                        .ForEach(C => {
                            if (!config.FlatConfigTemplatesByName.TryGetValue(C.Name.ToString(), out var recipe)) return;

                            recipe.Childs?
                                .FirstOrDefault(R => R.Key == "Child Inputs").Value?.Attr?
                                .ForEach(R => {
                                    if (!config.FlatConfigBlockByName.TryGetValue(R.Name.ToString(), out var ressource)) return;
                                    if(!int.TryParse(ressource.Attr.FirstOrDefault(A => A.Name == "Id")?.Value.ToString(), out var ressId)) return;

                                    if (ressList.TryGetValue(ressId, out var count)) ressList[ressId] = count + (int)R.Value;
                                    else                                             ressList.Add(ressId,       (int)R.Value);
                                });
                        });

                    if (ressList.Count > 0) templates.Add(id, ressList);
                });

            Console.WriteLine(templates.Count);
        }
    }
}
