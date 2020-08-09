using EcfParser;
using EmpyrionNetAPIDefinitions;
using EmpyrionScripting.Interface;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace EmpyrionScripting
{
    public class ConfigEcfAccess : IConfigEcfAccess
    {
        public EcfFile Configuration_Ecf { get; set; } = new EcfFile();
        public EcfFile Config_Ecf { get; set; } = new EcfFile();
        public EcfFile Flat_Config_Ecf { get; set; } = new EcfFile();
        public Dictionary<int, EcfBlock> ConfigBlockById { get; set; } = new Dictionary<int, EcfBlock>();
        public Dictionary<string, EcfBlock> ConfigBlockByName { get; set; } = new Dictionary<string, EcfBlock>();
        public Dictionary<int, EcfBlock> FlatConfigBlockById { get; set; } = new Dictionary<int, EcfBlock>();
        public Dictionary<string, EcfBlock> FlatConfigBlockByName { get; set; } = new Dictionary<string, EcfBlock>();
        public Dictionary<string, EcfBlock> FlatConfigTemplatesByName { get; set; } = new Dictionary<string, EcfBlock>();
        public Dictionary<int, Dictionary<int, int>> RecipeForBlockById { get; set; } = new Dictionary<int, Dictionary<int, int>>();
        public static Action<string, LogLevel> Log { get; set; } = (s, l) => Console.WriteLine(s);

        public void ReadConfigEcf(string contentPath)
        {
            try
            {
                Configuration_Ecf = EcfParser.Parse.Deserialize(File.ReadAllLines(Path.Combine(contentPath, "Configuration", "Config_Example.ecf")));
                Log($"EmpyrionScripting Config_Example.ecf: #{Configuration_Ecf.Blocks.Count}", LogLevel.Message);
            }
            catch (Exception error){ Log($"EmpyrionScripting Config_Example.ecf: {error}", LogLevel.Error); }

            try
            {
                Config_Ecf = EcfParser.Parse.Deserialize(File.ReadAllLines(Path.Combine(contentPath, "Configuration", "Config.ecf")));
                Log($"EmpyrionScripting Config.ecf: #{Config_Ecf.Blocks.Count}", LogLevel.Message);
            }
            catch (Exception error){ Log($"EmpyrionScripting Config.ecf: {error}", LogLevel.Error); }

            try { Configuration_Ecf.MergeWith(Config_Ecf); }
            catch (Exception error) { Log($"EmpyrionScripting MergeWith: {error}", LogLevel.Error); }

            try{ ConfigBlockById = BlocksById(Configuration_Ecf.Blocks);  }
            catch (Exception error){ Log($"EmpyrionScripting ConfigBlockById: {error}", LogLevel.Error); }

            try{ ConfigBlockByName = BlocksByName(Configuration_Ecf.Blocks); }
            catch (Exception error) { Log($"EmpyrionScripting ConfigBlockByName: {error}", LogLevel.Error); }

            Flat_Config_Ecf = new EcfFile() { Version = Configuration_Ecf.Version, Blocks = new List<EcfBlock>() };
            try { Configuration_Ecf.Blocks?.ForEach(B => Flat_Config_Ecf.Blocks.Add(MergeRefBlocks(new EcfBlock(), B))); }
            catch (Exception error) { Log($"EmpyrionScripting Configuration_Ecf.Blocks MergeRefBlocks: {error}", LogLevel.Error); }

            try { FlatConfigBlockById = BlocksById(Flat_Config_Ecf.Blocks); }
            catch (Exception error) { Log($"EmpyrionScripting FlatConfigBlockById: {error}", LogLevel.Error); }

            try { FlatConfigBlockByName = BlocksByName(Flat_Config_Ecf.Blocks); }
            catch (Exception error) { Log($"EmpyrionScripting FlatConfigBlockByName: {error}", LogLevel.Error); }

            try { FlatConfigTemplatesByName = TemplatesByName(Flat_Config_Ecf.Blocks); }
            catch (Exception error) { Log($"EmpyrionScripting FlatConfigBlockByName: {error}", LogLevel.Error); }

            try { RecipeForBlockById = RecipeForBlock(); }
            catch (Exception error) { Log($"EmpyrionScripting RecipeForBlock: {error}", LogLevel.Error); }
            
            Log($"EmpyrionScripting Configuration_Ecf: #{Configuration_Ecf?.Blocks?.Count} BlockById: #{ConfigBlockById?.Count} BlockByName: #{ConfigBlockByName?.Count}", LogLevel.Message);
        }

        private Dictionary<int, Dictionary<int, int>> RecipeForBlock()
        {
            var templates = new Dictionary<int, Dictionary<int, int>>();

            FlatConfigBlockById
                .ForEach(B => {
                    var idCfg = B.Value.Attr.FirstOrDefault(A => A.Name == "Id");
                    if (!int.TryParse(idCfg?.Value?.ToString(), out var id)) return;

                    var ressList = new Dictionary<int, int>();
                    var templateRoot = B.Value.Attr.FirstOrDefault(A => A.Name == "TemplateRoot")?.Value?.ToString() ??
                                       idCfg.AddOns?.FirstOrDefault(A => A.Key == "Name").Value?.ToString();
                    if (string.IsNullOrEmpty(templateRoot)) return;
                    if (!FlatConfigTemplatesByName.TryGetValue(templateRoot, out var templateRootBlock)) return;

                    templateRootBlock.Childs?
                        .FirstOrDefault(C => C.Key == "Child Inputs").Value?.Attr?
                        .ForEach(C => {
                            if (!FlatConfigTemplatesByName.TryGetValue(C.Name.ToString(), out var recipe)) return;

                            recipe.Childs?
                                .FirstOrDefault(R => R.Key == "Child Inputs").Value?.Attr?
                                .ForEach(R => {
                                    if (!FlatConfigBlockByName.TryGetValue(R.Name.ToString(), out var ressource)) return;
                                    if (!int.TryParse(ressource.Attr.FirstOrDefault(A => A.Name == "Id")?.Value.ToString(), out var ressId)) return;

                                    if (ressList.TryGetValue(ressId, out var count)) ressList[ressId] = count + (int)R.Value;
                                    else ressList.Add(ressId, (int)R.Value);
                                });
                        });

                    if (ressList.Count > 0) templates.Add(id, ressList);
                });

            return templates;
        }

        public EcfBlock MergeRefBlocks(EcfBlock target, EcfBlock source)
        {
            var refAttr = source?.Attr?.FirstOrDefault(A => A.Name == "Id")?.AddOns?.FirstOrDefault(A => A.Key == "Ref");
            if (refAttr?.Value != null && ConfigBlockByName.TryGetValue(refAttr.Value.Value.ToString(), out var refBlock)) target = MergeRefBlocks(target, refBlock);

            target.MergeWith(source);

            return target;
        }

        public Dictionary<int, EcfBlock> BlocksById(IEnumerable<EcfBlock> blocks) =>
            blocks
                .Where(B => (B.Name == "Block" || B.Name == "Item") && B.Attr.Any(A => A.Name == "Id"))
                .ToDictionary(B => (int)B.Attr.First(A => A.Name == "Id").Value, B => B);

        public Dictionary<string, EcfBlock> BlocksByName(IEnumerable<EcfBlock> blocks) =>
            blocks
                .Where(B => (B.Name == "Block" || B.Name == "Item") && B.Attr.Any(A => A.Name == "Id" && A.AddOns != null && A.AddOns.Any(a => a.Key == "Name")))
                .ToDictionary(B => B.Attr.First(A => A.Name == "Id").AddOns.First(A => A.Key == "Name").Value.ToString(), B => B);

        public Dictionary<string, EcfBlock> TemplatesByName(IEnumerable<EcfBlock> blocks) =>
            blocks
                .Where(B => B.Name == "Template" && B.Attr.Any(A => A.Name == "Name"))
                .ToDictionary(B => B.Attr.First(A => A.Name == "Name").Value.ToString(), B => B);

    }
}
