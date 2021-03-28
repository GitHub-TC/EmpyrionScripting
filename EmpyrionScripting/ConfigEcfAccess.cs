using EcfParser;
using Eleon.Modding;
using EmpyrionNetAPIDefinitions;
using EmpyrionScripting.Interface;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace EmpyrionScripting
{
    public class ConfigEcfAccess : IConfigEcfAccess
    {
        private IDictionary<string, int> BlockIdMapping;

        public EcfFile BlocksConfig_Ecf { get; set; } = new EcfFile();
        public EcfFile ItemsConfig_Ecf { get; set; } = new EcfFile();
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
        public string ContentPath { get; set; }
        public string ScenarioContentPath { get; set; }

        public void ReadConfigEcf(string contentPath, string activeScenario, string blockMappingFile, IModApi modApi)
        {
            ContentPath = contentPath;
            ScenarioContentPath = string.IsNullOrEmpty(activeScenario) ? null : Path.Combine(contentPath, "Scenarios", activeScenario, "Content");

            Log($"EmpyrionScripting ReadConfigEcf: ContentPath:{contentPath} Scenario:{activeScenario} -> {ScenarioContentPath}", LogLevel.Message);

            var timer = Stopwatch.StartNew();
            ReadBlockMappingFile(blockMappingFile, modApi);
            ReadEcfFiles();
            MergeEcfFiles();
            BuildIdAndNameAccess();
            FlatEcfConfigData();
            CalcBlockRessources();
            timer.Stop();

            BlockIdMapping          .ForEach(B => Log($"ReadBlockMappingFile: '{B.Key}' -> {B.Value}]", LogLevel.Debug));
            BlocksConfig_Ecf .Blocks.ForEach(B => Log($"BlocksConfig_Ecf.Blocks: '{B.Name}[{B.Values?.FirstOrDefault(A => A.Key == "Id").Value}] '{B.Values?.FirstOrDefault(A => A.Key == "Name").Value}'", LogLevel.Debug));
            ItemsConfig_Ecf  .Blocks.ForEach(B => Log($"ItemsConfig_Ecf.Blocks: '{B.Name}[{B.Values?.FirstOrDefault(A => A.Key == "Id").Value}] '{B.Values?.FirstOrDefault(A => A.Key == "Name").Value}'", LogLevel.Debug));
            Configuration_Ecf.Blocks.ForEach(B => Log($"Configuration_Ecf.Blocks: '{B.Name}[{B.Values?.FirstOrDefault(A => A.Key == "Id").Value}] '{B.Values?.FirstOrDefault(A => A.Key == "Name").Value}'", LogLevel.Debug));
            Flat_Config_Ecf  .Blocks.ForEach(B => Log($"Flat_Config_Ecf.Blocks: '{B.Name}[{B.Values?.FirstOrDefault(A => A.Key == "Id").Value}] '{B.Values?.FirstOrDefault(A => A.Key == "Name").Value}'", LogLevel.Debug));
            FlatConfigBlockById     .ForEach(B => Log($"FlatConfigBlockById: '{B.Key}' -> {B.Value?.Values?.FirstOrDefault(A => A.Key == "Name").Value}]", LogLevel.Debug));
            FlatConfigBlockByName   .ForEach(B => Log($"FlatConfigBlockByName: '{B.Key}' -> {B.Value?.Values?.FirstOrDefault(A => A.Key == "Name").Value}]", LogLevel.Debug));

            Log($"EmpyrionScripting Configuration_Ecf: #{Configuration_Ecf?.Blocks?.Count} BlockById: #{ConfigBlockById?.Count} BlockByName: #{ConfigBlockByName?.Count} BlockIdMapping:[{BlockIdMapping?.Count}] {blockMappingFile} takes:{timer.Elapsed}", LogLevel.Message);
        }

        public void CalcBlockRessources()
        {
            try { ResourcesForBlockById = ResourcesForBlock(); }
            catch (Exception error) { Log($"EmpyrionScripting RecipeForBlock: {error}", LogLevel.Error); }
        }

        public void FlatEcfConfigData()
        {
            Flat_Config_Ecf = new EcfFile() { Version = Configuration_Ecf.Version, Blocks = new List<EcfBlock>() };
            try { Configuration_Ecf.Blocks?.ForEach(B => Flat_Config_Ecf.Blocks.Add(MergeRefBlocks(new EcfBlock(), B))); }
            catch (Exception error) { Log($"EmpyrionScripting Configuration_Ecf.Blocks MergeRefBlocks: {error}", LogLevel.Error); }

            try { FlatConfigBlockById = BlocksById(Flat_Config_Ecf.Blocks); }
            catch (Exception error) { Log($"EmpyrionScripting FlatConfigBlockById: {error}", LogLevel.Error); }

            try { FlatConfigBlockByName = BlocksByName(Flat_Config_Ecf.Blocks); }
            catch (Exception error) { Log($"EmpyrionScripting FlatConfigBlockByName: {error}", LogLevel.Error); }

            try { FlatConfigTemplatesByName = TemplatesByName(Flat_Config_Ecf.Blocks); }
            catch (Exception error) { Log($"EmpyrionScripting FlatConfigBlockByName: {error}", LogLevel.Error); }
        }

        public void BuildIdAndNameAccess()
        {
            try { ConfigBlockById = BlocksById(Configuration_Ecf.Blocks); }
            catch (Exception error) { Log($"EmpyrionScripting ConfigBlockById: {error}", LogLevel.Error); }

            try { ConfigBlockByName = BlocksByName(Configuration_Ecf.Blocks); }
            catch (Exception error) { Log($"EmpyrionScripting ConfigBlockByName: {error}", LogLevel.Error); }
        }

        public void MergeEcfFiles()
        {
            try { Configuration_Ecf.MergeWith(BlocksConfig_Ecf); }
            catch (Exception error) { Log($"EmpyrionScripting MergeWith: {error}", LogLevel.Error); }

            try { Configuration_Ecf.MergeWith(ItemsConfig_Ecf); }
            catch (Exception error) { Log($"EmpyrionScripting MergeWith: {error}", LogLevel.Error); }

            try { Configuration_Ecf.MergeWith(Config_Ecf); }
            catch (Exception error) { Log($"EmpyrionScripting MergeWith: {error}", LogLevel.Error); }
        }

        public void ReadEcfFiles()
        {
            Configuration_Ecf   = ReadEcf("Config_Example.ecf", B => { });
            Config_Ecf          = ReadEcf("Config.ecf",         B => {
                if(B.Attr.Any(A => A.Name == "Id")) // Id forbidden in Config.ecf
                {
                    B.Attr?.Clear();
                    B.Childs?.Clear();
                    B.EcfValues?.Clear();
                }
            });
            BlocksConfig_Ecf    = ReadEcf("BlocksConfig.ecf", B => { });
            ItemsConfig_Ecf     = ReadEcf("ItemsConfig.ecf",  B => { });
        }

        public void ReadBlockMappingFile(string blockMappingFile, IModApi modApi)
        {
            try
            {
                BlockIdMapping = modApi?.Application.GetBlockAndItemMapping();
                if (BlockIdMapping?.Count == 0)
                {
                    Log($"GetBlockAndItemMapping empty try self", LogLevel.Message);
                    BlockIdMapping = Parse.ReadBlockMapping(blockMappingFile);
                }
                else
                {
                    Log($"ReadBlockMappingFile: BlockIdMapping API:[{BlockIdMapping?.Count}]", LogLevel.Message);
                }
            }
            catch (Exception error)
            {
                Log($"GetBlockAndItemMapping: {error}", LogLevel.Error);
                BlockIdMapping = Parse.ReadBlockMapping(blockMappingFile);
            }
        }

        private EcfFile ReadEcf(string filename, Action<EcfBlock> mapId)
        {
            var result = new EcfFile();
            try
            {
                var fullFilename = GetConfigurationFile(filename);
                Log($"EmpyrionScripting {fullFilename}: start", LogLevel.Message);
                result = Parse.Deserialize(File.ReadAllLines(fullFilename));
                result.Blocks.ForEach(mapId);
                if (BlockIdMapping != null) Parse.ReplaceWithMappedIds(result, BlockIdMapping);
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

                    if (ressList.Count > 0 && !templates.ContainsKey(id)) templates.Add(id, ressList);
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
            if (source == null || source.Values == null) return target;

            if (source.Values.TryGetValue("Ref", out var refValue) && ConfigBlockByName.TryGetValue(refValue?.ToString(), out var refBlock)) target = MergeRefBlocks(target, refBlock);

            target.MergeWith(source);

            return target;
        }

        public IDictionary<int, EcfBlock> BlocksById(IEnumerable<EcfBlock> blocks) =>
            blocks.EcfBlocksToDictionary(B => (B.Name == "Block" || B.Name == "Item") && B.Values?.ContainsKey("Id") == true,
                                         B => (int)B.Values["Id"]);

        public IDictionary<string, EcfBlock> BlocksByName(IEnumerable<EcfBlock> blocks) =>
            blocks.EcfBlocksToDictionary(B => (B.Name == "Block" || B.Name == "Item") && B.Values?.ContainsKey("Name") == true,
                                         B => B.Values["Name"]?.ToString());

        public IDictionary<string, EcfBlock> TemplatesByName(IEnumerable<EcfBlock> blocks) =>
            blocks.EcfBlocksToDictionary(B => B.Name == "Template" && B.Values?.ContainsKey("Name") == true,
                                         B => B.Values["Name"]?.ToString());

    }
}
