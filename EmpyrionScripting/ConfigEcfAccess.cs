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
        public IDictionary<string, int> BlockIdMapping;
        public IDictionary<int, string> IdBlockMapping;

        public EcfFile Templates_Ecf { get; private set; }
        public EcfFile BlocksConfig_Ecf { get; set; } = new EcfFile();
        public EcfFile ItemsConfig_Ecf { get; set; } = new EcfFile();
        public EcfFile Configuration_Ecf { get; set; } = new EcfFile();
        public EcfFile Flat_Config_Ecf { get; set; } = new EcfFile();
        public IDictionary<int, EcfBlock> ConfigBlockById { get; set; } = new Dictionary<int, EcfBlock>();
        public IDictionary<string, EcfBlock> ConfigBlockByName { get; set; } = new Dictionary<string, EcfBlock>();
        public IDictionary<string, string> ParentBlockName { get; set; } = new Dictionary<string, string>();
        public IDictionary<int, EcfBlock> FlatConfigBlockById { get; set; } = new Dictionary<int, EcfBlock>();
        public IDictionary<string, EcfBlock> FlatConfigBlockByName { get; set; } = new Dictionary<string, EcfBlock>();
        public IDictionary<string, EcfBlock> FlatConfigTemplatesByName { get; set; } = new Dictionary<string, EcfBlock>();
        public IDictionary<int, Dictionary<int, double>> ResourcesForBlockById { get; set; } = new Dictionary<int, Dictionary<int, double>>();
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

            if(EmpyrionScripting.Configuration?.Current?.LogLevel == LogLevel.Debug) { 
                BlockIdMapping          .ForEach(B => Log($"ReadBlockMappingFile: '{B.Key}' -> {B.Value}]", LogLevel.Debug));
                Templates_Ecf    .Blocks.ForEach(B => Log($"Templates_Ecf.Blocks: '{B.Name}[{B.Values?.FirstOrDefault(A => A.Key == "Id").Value}] '{B.Values?.FirstOrDefault(A => A.Key == "Name").Value}'", LogLevel.Debug));
                BlocksConfig_Ecf .Blocks.ForEach(B => Log($"BlocksConfig_Ecf.Blocks: '{B.Name}[{B.Values?.FirstOrDefault(A => A.Key == "Id").Value}] '{B.Values?.FirstOrDefault(A => A.Key == "Name").Value}'", LogLevel.Debug));
                ItemsConfig_Ecf  .Blocks.ForEach(B => Log($"ItemsConfig_Ecf.Blocks: '{B.Name}[{B.Values?.FirstOrDefault(A => A.Key == "Id").Value}] '{B.Values?.FirstOrDefault(A => A.Key == "Name").Value}'", LogLevel.Debug));
                Configuration_Ecf.Blocks.ForEach(B => Log($"Configuration_Ecf.Blocks: '{B.Name}[{B.Values?.FirstOrDefault(A => A.Key == "Id").Value}] '{B.Values?.FirstOrDefault(A => A.Key == "Name").Value}'", LogLevel.Debug));
                Flat_Config_Ecf  .Blocks.ForEach(B => Log($"Flat_Config_Ecf.Blocks: '{B.Name}[{B.Values?.FirstOrDefault(A => A.Key == "Id").Value}] '{B.Values?.FirstOrDefault(A => A.Key == "Name").Value}'", LogLevel.Debug));
                FlatConfigBlockById     .ForEach(B => Log($"FlatConfigBlockById: '{B.Key}' -> {B.Value?.Values?.FirstOrDefault(A => A.Key == "Name").Value}]", LogLevel.Debug));
                FlatConfigBlockByName   .ForEach(B => Log($"FlatConfigBlockByName: '{B.Key}' -> {B.Value?.Values?.FirstOrDefault(A => A.Key == "Name").Value}]", LogLevel.Debug));
                ResourcesForBlockById   .ForEach(B => Log($"ResourcesForBlockById: [{B.Key}] {(EmpyrionScripting.ConfigEcfAccess.FlatConfigBlockById.TryGetValue(B.Key, out var data) ? data.Values["Name"] : "")} -> {B.Value.Aggregate("", (r, i) => $"{r}\n{i.Value}: [{i.Key}] {(EmpyrionScripting.ConfigEcfAccess.FlatConfigBlockById.TryGetValue(i.Key, out var data) ? data.Values["Name"] : "")}")}", LogLevel.Message));
                ParentBlockName         .ForEach(B => Log($"ParentBlockName: {B.Key} -> {B.Value}", LogLevel.Debug));
            }

            Log($"EmpyrionScripting Configuration_Ecf: #{Configuration_Ecf?.Blocks?.Count} BlockById: #{ConfigBlockById?.Count} BlockByName: #{ConfigBlockByName?.Count} ParentBlockNames: #{ParentBlockName.Count} BlockIdMapping:[{BlockIdMapping?.Count}] {blockMappingFile} takes:{timer.Elapsed}", LogLevel.Message);
        }

        public void CalcBlockRessources()
        {
            try { ResourcesForBlockById = ResourcesForBlock(); }
            catch (Exception error) { Log($"EmpyrionScripting RecipeForBlock: {error}", LogLevel.Error); }
        }

        public void FlatEcfConfigData()
        {
            Flat_Config_Ecf = new EcfFile() { Blocks = new List<EcfBlock>() };
            try { Configuration_Ecf.Blocks?.ForEach(B => Flat_Config_Ecf.Blocks.Add(MergeRefBlocks(new EcfBlock(), B))); }
            catch (Exception error) { Log($"EmpyrionScripting Configuration_Ecf.Blocks MergeRefBlocks: {error}", LogLevel.Error); }

            try { FlatConfigBlockById = BlocksById(Flat_Config_Ecf.Blocks); }
            catch (Exception error) { Log($"EmpyrionScripting FlatConfigBlockById: {error}", LogLevel.Error); }

            try { FlatConfigBlockByName = BlocksByName(Flat_Config_Ecf.Blocks); }
            catch (Exception error) { Log($"EmpyrionScripting FlatConfigBlockByName: {error}", LogLevel.Error); }

            try { FlatConfigTemplatesByName = TemplatesByName(Flat_Config_Ecf.Blocks); }
            catch (Exception error) { Log($"EmpyrionScripting FlatConfigBlockByName: {error}", LogLevel.Error); }

            try{ GenerateParentBlockList(); }
            catch (Exception error) { Log($"EmpyrionScripting FlatConfigBlockByName: {error}", LogLevel.Error); }
        }

        private void GenerateParentBlockList()
        {
            FlatConfigBlockByName
                .Where(B => B.Value?.Values?.ContainsKey("ChildBlocks") == true)
                .ForEach(B => B.Value.Values["ChildBlocks"].ToString()
                    .Split(',').Select(N => N.Trim())
                    .ForEach(N => { if (!ParentBlockName.ContainsKey(N)) ParentBlockName.Add(N, B.Key); })
                );

            FlatConfigBlockByName
                .Where(B => B.Value?.Values?.ContainsKey("ParentBlocks") == true)
                .ForEach(B => B.Value.Values["ParentBlocks"].ToString()
                    .Split(',').Select(N => N.Trim())
                    .ForEach(N => {
                        if (!B.Value.Values.TryGetValue("Name", out var name)           || 
                            !FlatConfigBlockByName.TryGetValue(N, out var parentBlock)  || 
                            !parentBlock.Values.TryGetValue("AllowPlacingAt", out var placeableAt)) return;

                        placeableAt.ToString().Split(',').Select(N => N.Trim())
                            .ForEach(P => { if (!ParentBlockName.ContainsKey(P + name.ToString())) ParentBlockName.Add(P + name.ToString(), N); });
                    })
                );
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
            try { Configuration_Ecf.MergeWith(Templates_Ecf); }
            catch (Exception error) { Log($"EmpyrionScripting MergeWith: {error}", LogLevel.Error); }

            try { Configuration_Ecf.MergeWith(BlocksConfig_Ecf); }
            catch (Exception error) { Log($"EmpyrionScripting MergeWith: {error}", LogLevel.Error); }

            try { Configuration_Ecf.MergeWith(ItemsConfig_Ecf); }
            catch (Exception error) { Log($"EmpyrionScripting MergeWith: {error}", LogLevel.Error); }
        }

        public void ReadEcfFiles()
        {
            Templates_Ecf       = ReadEcf("Templates.ecf",    B => { });
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

            try
            {
                IdBlockMapping = BlockIdMapping.ToDictionary(I => I.Value, I => I.Key);
            }
            catch (Exception error)
            {
                IdBlockMapping = new Dictionary<int, string>();
                Log($"IdBlockMapping: {error}", LogLevel.Error);
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

        private Dictionary<int, Dictionary<int, double>> ResourcesForBlock()
        {
            var templates = new Dictionary<int, Dictionary<int, double>>();

            FlatConfigBlockById
                .ForEach(B => {
                    var idCfg = B.Value.Attr.FirstOrDefault(A => A.Name == "Id");
                    if (!int.TryParse(idCfg?.Value?.ToString(), out var id)) return;

                    var ressList = new Dictionary<int, double>();
                    var templateRoot = B.Value.Attr.FirstOrDefault(A => A.Name == "TemplateRoot")?.Value?.ToString() ??
                                       idCfg.AddOns?.FirstOrDefault(A => A.Key == "Name").Value?.ToString();
                    if (string.IsNullOrEmpty(templateRoot)) return;
                    if (!FlatConfigTemplatesByName.TryGetValue(templateRoot, out var templateRootBlock)) return;

                    ScanTemplates(templateRootBlock, ressList, 1);

                    if (ressList.Count > 0 && !templates.ContainsKey(id)) templates.Add(id, ressList);
                });

            return templates;
        }

        private void ScanTemplates(EcfBlock templateRootBlock, Dictionary<int, double> ressList, double multiplyer)
        {
            var templateName = templateRootBlock.Attr.FirstOrDefault(A => A.Name == "Name")?.Value.ToString();
            bool.TryParse(templateRootBlock.Attr.FirstOrDefault(A => A.Name == "BaseItem")?.Value.ToString(), out var isBaseItem);

            templateRootBlock.Childs?
                .FirstOrDefault(C => C.Key == "Child Inputs").Value?.Attr?
                .ForEach(C => {
                    if (C.Name.ToString() == templateName) return;

                    if (!isBaseItem && FlatConfigTemplatesByName.TryGetValue(C.Name, out var recipe))
                    {
                        bool.TryParse(recipe.Attr.FirstOrDefault(A => A.Name == "BaseItem")?.Value.ToString(), out var isSubBaseItem);
                        if (!isSubBaseItem)
                        {
                            var recipeMultiplyer = multiplyer * (int)C.Value;
                            if (recipe.Values.TryGetValue("OutputCount", out var outputCount)) recipeMultiplyer /= (int)outputCount;
                            ScanTemplates(recipe, ressList, recipeMultiplyer);
                            return;
                        }
                    }

                    if (!FlatConfigBlockByName.TryGetValue(C.Name, out var ressource)) return;
                    if (!ressource.Values.TryGetValue("Id", out var ressId)) return;

                    if (ressList.TryGetValue((int)ressId, out var count)) ressList[(int)ressId] = count + multiplyer * (int)C.Value;
                    else                                                  ressList.Add((int)ressId,       multiplyer * (int)C.Value);
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
