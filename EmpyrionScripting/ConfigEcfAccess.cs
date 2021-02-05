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
        public EcfFile BlocksConfig_Ecf { get; set; } = new EcfFile();
        public EcfFile Configuration_Ecf { get; set; } = new EcfFile();
        public EcfFile Config_Ecf { get; set; } = new EcfFile();
        public EcfFile Flat_Config_Ecf { get; set; } = new EcfFile();
        public IDictionary<int, EcfBlock> ConfigBlockById { get; set; } = new Dictionary<int, EcfBlock>();
        public IDictionary<string, EcfBlock> ConfigBlockByName { get; set; } = new Dictionary<string, EcfBlock>();
        public IDictionary<int, EcfBlock> FlatConfigBlockById { get; set; } = new Dictionary<int, EcfBlock>();
        public IDictionary<string, EcfBlock> FlatConfigBlockByName { get; set; } = new Dictionary<string, EcfBlock>();
        public IDictionary<string, EcfBlock> FlatConfigTemplatesByName { get; set; } = new Dictionary<string, EcfBlock>();
        public IDictionary<int, Dictionary<int, int>> ResourcesForBlockById { get; set; } = new Dictionary<int, Dictionary<int, int>>();
        public static Action<string, LogLevel> Log { get; set; } = (s, l) => Console.WriteLine(s);
        public string ContentPath { get; private set; }
        public string ScenarioContentPath { get; private set; }

        public void ReadConfigEcf(string contentPath, string activeScenario)
        {
            ContentPath         = contentPath;
            ScenarioContentPath = string.IsNullOrEmpty(activeScenario) ? null : Path.Combine(contentPath, "Scenarios", activeScenario, "Content");

            Log($"EmpyrionScripting ReadConfigEcf: ContentPath:{contentPath} Scenario:{activeScenario} -> {ScenarioContentPath}", LogLevel.Message);

            Configuration_Ecf   = ReadEcf("Config_Example.ecf");
            Config_Ecf          = ReadEcf("Config.ecf");
            BlocksConfig_Ecf    = ReadEcf("BlocksConfig.ecf");

            try { Configuration_Ecf.MergeWith(BlocksConfig_Ecf); }
            catch (Exception error) { Log($"EmpyrionScripting MergeWith: {error}", LogLevel.Error); }

            try { Configuration_Ecf.MergeWith(Config_Ecf); }
            catch (Exception error) { Log($"EmpyrionScripting MergeWith: {error}", LogLevel.Error); }

            try { ConfigBlockById = BlocksById(Configuration_Ecf.Blocks); }
            catch (Exception error) { Log($"EmpyrionScripting ConfigBlockById: {error}", LogLevel.Error); }

            try { ConfigBlockByName = BlocksByName(Configuration_Ecf.Blocks); }
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

            try { ResourcesForBlockById = ResourcesForBlock(); }
            catch (Exception error) { Log($"EmpyrionScripting RecipeForBlock: {error}", LogLevel.Error); }

            Log($"EmpyrionScripting Configuration_Ecf: #{Configuration_Ecf?.Blocks?.Count} BlockById: #{ConfigBlockById?.Count} BlockByName: #{ConfigBlockByName?.Count}", LogLevel.Message);
        }

        private EcfFile ReadEcf(string filename)
        {
            var result = new EcfFile();
            try
            {
                var fullFilename = GetConfigurationFile(filename);
                Log($"EmpyrionScripting {fullFilename}: start", LogLevel.Message);
                result = EcfParser.Parse.Deserialize(File.ReadAllLines(fullFilename));
                Log($"EmpyrionScripting {fullFilename}: #{result.Blocks.Count}", LogLevel.Message);
            }
            catch (Exception error) { Log($"EmpyrionScripting {filename}: {error}", LogLevel.Error); }

            return result;
        }

        private string GetConfigurationFile(string fileName)
            => !string.IsNullOrEmpty(ScenarioContentPath) && File.Exists(Path.Combine(ScenarioContentPath, "Configuration", fileName))
                ? Path.Combine(ScenarioContentPath, "Configuration", fileName)
                : Path.Combine(ContentPath,         "Configuration", fileName);

        private Dictionary<int, Dictionary<int, int>> ResourcesForBlock()
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

                    ScanTemplates(templateRootBlock, ressList);

                    if (ressList.Count > 0) templates.Add(id, ressList);
                });

            return templates;
        }

        private void ScanTemplates(EcfBlock templateRootBlock, Dictionary<int, int> ressList)
        {
            var templateName = templateRootBlock.Attr.FirstOrDefault(A => A.Name == "Name")?.Value.ToString();
            bool.TryParse(templateRootBlock.Attr.FirstOrDefault(A => A.Name == "BaseItem")?.Value.ToString(), out var isBaseItem);

            templateRootBlock.Childs?
                .FirstOrDefault(C => C.Key == "Child Inputs").Value?.Attr?
                .ForEach(C => {
                    if (C.Name.ToString() == templateName) return;

                    if (!isBaseItem && FlatConfigTemplatesByName.TryGetValue(C.Name.ToString(), out var recipe))
                    {
                        bool.TryParse(recipe.Attr.FirstOrDefault(A => A.Name == "BaseItem")?.Value.ToString(), out var isSubBaseItem);
                        if (!isSubBaseItem)
                        {
                            ScanTemplates(recipe, ressList);
                            return;
                        }
                    }

                    if (!FlatConfigBlockByName.TryGetValue(C.Name.ToString(), out var ressource)) return;
                    if (!int.TryParse(ressource.Attr.FirstOrDefault(A => A.Name == "Id")?.Value.ToString(), out var ressId)) return;

                    if (ressList.TryGetValue(ressId, out var count)) ressList[ressId] = count + (int)C.Value;
                    else ressList.Add(ressId, (int)C.Value);
                });
        }


        public EcfBlock MergeRefBlocks(EcfBlock target, EcfBlock source)
        {
            var refAttr = source?.Attr?.FirstOrDefault(A => A.Name == "Id")?.AddOns?.FirstOrDefault(A => A.Key == "Ref");
            if (refAttr?.Value != null && ConfigBlockByName.TryGetValue(refAttr.Value.Value.ToString(), out var refBlock)) target = MergeRefBlocks(target, refBlock);

            target.MergeWith(source);

            return target;
        }

        public IDictionary<int, EcfBlock> BlocksById(IEnumerable<EcfBlock> blocks) =>
            blocks.EcfBlocksToDictionary(B => (B.Name == "Block" || B.Name == "Item") && B.Attr.Any(A => A.Name == "Id"),
                                         B => (int)B.Attr.First(A => A.Name == "Id").Value);

        public IDictionary<string, EcfBlock> BlocksByName(IEnumerable<EcfBlock> blocks) =>
            blocks.EcfBlocksToDictionary(B => (B.Name == "Block" || B.Name == "Item") && B.Attr.Any(A => A.Name == "Id" && A.AddOns != null && A.AddOns.Any(a => a.Key == "Name")),
                                         B => B.Attr.First(A => A.Name == "Id").AddOns.First(A => A.Key == "Name").Value.ToString());

        public IDictionary<string, EcfBlock> TemplatesByName(IEnumerable<EcfBlock> blocks) =>
            blocks.EcfBlocksToDictionary(B => B.Name == "Template" && B.Attr.Any(A => A.Name == "Name"),
                                         B => B.Attr.First(A => A.Name == "Name").Value.ToString());

    }
}
