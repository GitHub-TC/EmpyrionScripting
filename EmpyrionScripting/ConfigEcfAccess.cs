using EcfParser;
using Eleon.Modding;
using EmpyrionNetAPIDefinitions;
using EmpyrionScripting.Interface;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace EmpyrionScripting
{
    public class ConfigEcfAccess : IConfigEcfAccess
    {
        public IReadOnlyDictionary<string, int> BlockIdMapping { get; set; }
        public IReadOnlyDictionary<int, string> IdBlockMapping { get; set; }

        public EcfFile Templates_Ecf { get; private set; }
        public EcfFile BlocksConfig_Ecf { get; set; } = new EcfFile();
        public EcfFile ItemsConfig_Ecf { get; set; } = new EcfFile();
        public EcfFile TokenConfig_Ecf { get; set; } = new EcfFile();
        public EcfFile Configuration_Ecf { get; set; } = new EcfFile();
        public EcfFile Flat_Config_Ecf { get; set; } = new EcfFile();
        public IReadOnlyDictionary<int, EcfBlock> ConfigBlockById { get; set; } = new Dictionary<int, EcfBlock>();
        public IReadOnlyDictionary<string, EcfBlock> ConfigBlockByName { get; set; } = new Dictionary<string, EcfBlock>();
        public IReadOnlyDictionary<int, EcfBlock> TokenById { get; set; } = new Dictionary<int, EcfBlock>();
        public IDictionary<string, string> ParentBlockName { get; set; } = new Dictionary<string, string>();
        public IReadOnlyDictionary<int, EcfBlock> FlatConfigBlockById { get; set; } = new Dictionary<int, EcfBlock>();
        public IReadOnlyDictionary<string, EcfBlock> FlatConfigBlockByName { get; set; } = new Dictionary<string, EcfBlock>();
        public IReadOnlyDictionary<string, EcfBlock> FlatConfigTemplatesByName { get; set; } = new Dictionary<string, EcfBlock>();
        public IReadOnlyDictionary<int, Dictionary<int, double>> ResourcesForBlockById { get; set; } = new Dictionary<int, Dictionary<int, double>>();
        public static Action<string, LogLevel> Log { get; set; } = (s, l) => Console.WriteLine(s);
        public string ContentPath { get; set; }
        public string ScenarioContentPath { get; set; }
        public Dictionary<int, IHarvestInfo> HarvestBlockData { get; set; }

        public void ReadConfigEcf(string contentPath, string activeScenario, string blockMappingFile, IModApi modApi)
        {
            Log($"EmpyrionScripting ReadConfigEcf: ContentPath:{contentPath} Scenario:{activeScenario}", LogLevel.Message);

            ContentPath = contentPath;
            ScenarioContentPath = string.IsNullOrEmpty(activeScenario) ? null : Path.Combine(contentPath, "Scenarios", activeScenario, "Content");

            Log($"EmpyrionScripting ReadConfigEcf: ContentPath:{contentPath} Scenario:{activeScenario} -> {ScenarioContentPath}", LogLevel.Message);

            var timer = Stopwatch.StartNew();
            ReadBlockMappingFile(blockMappingFile, modApi);
            Log($"ReadEcfFiles"              , LogLevel.Message); ReadEcfFiles();
            Log($"MergeEcfFiles"             , LogLevel.Message); MergeEcfFiles();
            Log($"BuildIdAndNameAccess"      , LogLevel.Message); BuildIdAndNameAccess();
            Log($"FlatEcfConfigData"         , LogLevel.Message); FlatEcfConfigData();
            Log($"CalcBlockRessources"       , LogLevel.Message); CalcBlockRessources();
            Log($"GetHarvestBlockData"       , LogLevel.Message); GetHarvestBlockData();
            timer.Stop();

            if(EmpyrionScripting.Configuration?.Current?.LogLevel == LogLevel.Debug) { 
                BlockIdMapping          .ForEach(B => Log($"ReadBlockMappingFile: '{B.Key}' -> {B.Value}]"                                                                                                     , LogLevel.Debug));
                Templates_Ecf    .Blocks.ForEach(B => Log($"Templates_Ecf.Blocks: '{B.Name}[{B.Values?.FirstOrDefault(A => A.Key == "Id").Value}] '{B.Values?.FirstOrDefault(A => A.Key == "Name").Value}'"    , LogLevel.Debug));
                BlocksConfig_Ecf .Blocks.ForEach(B => Log($"BlocksConfig_Ecf.Blocks: '{B.Name}[{B.Values?.FirstOrDefault(A => A.Key == "Id").Value}] '{B.Values?.FirstOrDefault(A => A.Key == "Name").Value}'" , LogLevel.Debug));
                ItemsConfig_Ecf  .Blocks.ForEach(B => Log($"ItemsConfig_Ecf.Blocks: '{B.Name}[{B.Values?.FirstOrDefault(A => A.Key == "Id").Value}] '{B.Values?.FirstOrDefault(A => A.Key == "Name").Value}'"  , LogLevel.Debug));
                TokenConfig_Ecf  .Blocks.ForEach(B => Log($"TokenConfig_Ecf.Blocks: '{B.Name}[{B.Values?.FirstOrDefault(A => A.Key == "Id").Value}] '{B.Values?.FirstOrDefault(A => A.Key == "Name").Value}'"  , LogLevel.Debug));
                Configuration_Ecf.Blocks.ForEach(B => Log($"Configuration_Ecf.Blocks: '{B.Name}[{B.Values?.FirstOrDefault(A => A.Key == "Id").Value}] '{B.Values?.FirstOrDefault(A => A.Key == "Name").Value}'", LogLevel.Debug));
                Flat_Config_Ecf  .Blocks.ForEach(B => Log($"Flat_Config_Ecf.Blocks: '{B.Name}[{B.Values?.FirstOrDefault(A => A.Key == "Id").Value}] '{B.Values?.FirstOrDefault(A => A.Key == "Name").Value}'"  , LogLevel.Debug));
                FlatConfigBlockById     .ForEach(B => Log($"FlatConfigBlockById: '{B.Key}' -> {B.Value?.Values?.FirstOrDefault(A => A.Key == "Name").Value}]"                                                  , LogLevel.Debug));
                FlatConfigBlockByName   .ForEach(B => Log($"FlatConfigBlockByName: '{B.Key}' -> {B.Value?.Values?.FirstOrDefault(A => A.Key == "Name").Value}]"                                                , LogLevel.Debug));
                TokenById               .ForEach(B => Log($"TokenById: '{B.Key}' -> {B.Value?.Values?.FirstOrDefault(A => A.Key == "Name").Value}]"                                                            , LogLevel.Debug));
                ResourcesForBlockById   .ForEach(B => Log($"ResourcesForBlockById: [{B.Key}] {(EmpyrionScripting.ConfigEcfAccess.FlatConfigBlockById.TryGetValue(B.Key, out var data) ? data.Values["Name"] : "")} -> {B.Value.Aggregate("", (r, i) => $"{r}\n{i.Value}: [{i.Key}] {(EmpyrionScripting.ConfigEcfAccess.FlatConfigBlockById.TryGetValue(i.Key, out var data) ? data.Values["Name"] : "")}")}", LogLevel.Message));
                ParentBlockName         .ForEach(B => Log($"ParentBlockName: {B.Key} -> {B.Value}"                                                                                                             , LogLevel.Debug));
            }

            var nameIdMappingFile = Path.Combine(EmpyrionScripting.SaveGameModPath, "NameIdMapping.json");
            try
            {
                if (!File.Exists(nameIdMappingFile) || (DateTime.Now - File.GetLastWriteTime(nameIdMappingFile)) > new TimeSpan(1, 0, 0))
                {
                    File.WriteAllText(nameIdMappingFile, Newtonsoft.Json.JsonConvert.SerializeObject(BlockIdMapping, Newtonsoft.Json.Formatting.Indented));
                }
            }
            catch (Exception error)
            {
                Log($"EmpyrionScripting Configuration_Ecf: write id name mapping to '{nameIdMappingFile}' : {error}", LogLevel.Error);
            }

            Log($"EmpyrionScripting Configuration_Ecf: #{Configuration_Ecf?.Blocks?.Count} BlockById: #{ConfigBlockById?.Count} BlockByName: #{ConfigBlockByName?.Count} ParentBlockNames: #{ParentBlockName.Count} BlockIdMapping:[{BlockIdMapping?.Count}] {blockMappingFile} takes:{timer.Elapsed}", LogLevel.Message);
        }

        public void CalcBlockRessources()
        {
            try { ResourcesForBlockById = ResourcesForBlock(); }
            catch (Exception error) { Log($"EmpyrionScripting RecipeForBlock: {error}", LogLevel.Error); }
        }

        public void GetHarvestBlockData()
        {
            try
            {
                var deadPlants = new Dictionary<int, IHarvestInfo>();

                HarvestBlockData = FlatConfigBlockById
                  .Where(B => B.Value?.Values?.TryGetValue("Class", out object className) == true && (className.ToString() == "CropsGrown" || className.ToString() == "PlantGrowing"))
                  .Select(B => {
                      var logLevel = LogLevel.Debug;
                      try
                      {
                          int pickupTargetId = 0;
                          var withPickupTarget = B.Value.Values.TryGetValue("PickupTarget", out var pickupTarget) && BlockIdMapping.TryGetValue(pickupTarget.ToString(), out pickupTargetId);

                          var childDropOnHarvest = B.Value.Childs.FirstOrDefault(c => c.Key == "Child DropOnHarvest");
                          var dropOnHarvestItem  = childDropOnHarvest.Value?.Attr.FirstOrDefault(a => a.Name == "Item").Value?.ToString() ?? string.Empty;
                          var dropOnHarvestCount = int.TryParse(childDropOnHarvest.Value?.Attr.FirstOrDefault(a => a.Name == "Count")?.Value?.ToString(), out var count) ? count : 0;

                          var childCropsGrown   = B.Value.Childs?.FirstOrDefault(c => c.Key == "Child CropsGrown").Value?.Attr;
                          var childOnHarvest    = childCropsGrown?.FirstOrDefault(a => a.Name == "OnHarvest")?.Value?.ToString() ?? string.Empty;
                          var childOnDead       = childCropsGrown?.FirstOrDefault(a => a.Name == "OnDeath"  )?.Value?.ToString() ?? string.Empty;
                          if (BlockIdMapping.TryGetValue(childOnDead, out int childOnDeadId) && !deadPlants.ContainsKey(childOnDeadId)) deadPlants.Add(childOnDeadId, new HarvestInfo { Id = childOnDeadId });

                          logLevel = LogLevel.Error;

                          return new HarvestInfo
                          {
                              Id                    = B.Key,
                              Name                  = B.Value.Values.TryGetValue("Name", out var name) ? name.ToString() : null,
                              DropOnHarvestId       = BlockIdMapping.TryGetValue(dropOnHarvestItem, out int harvestItemId) ? harvestItemId : 0,
                              DropOnHarvestItem     = dropOnHarvestItem,
                              DropOnHarvestCount    = dropOnHarvestCount,
                              ChildOnHarvestId      = BlockIdMapping.TryGetValue(childOnHarvest, out int harvestItemChildId) ? harvestItemChildId : 0,
                              ChildOnHarvestItem    = childOnHarvest,
                              PickupTargetId        = withPickupTarget ? pickupTargetId : 0,
                          };
                      }
                      catch (Exception error )
                      {
                          Log?.Invoke($"HarvestBlockData failed {B.Key} {error}", logLevel);
                          return null;
                      }
                  })
                  .Where(h => h != null && h.Id != 0 && ((h.DropOnHarvestCount != 0 && h.DropOnHarvestId != 0 && h.ChildOnHarvestId != 0) || h.PickupTargetId != 0))
                  .ToDictionary(h => h.Id, h => (IHarvestInfo)h);

                deadPlants.ForEach(d => HarvestBlockData.Add(d.Key, d.Value));
            }
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

            try { TokenById = TokensById(TokenConfig_Ecf.Blocks); }
            catch (Exception error) { Log($"EmpyrionScripting TokenById: {error}", LogLevel.Error); }
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
            Templates_Ecf       = ReadEcf("Templates.ecf",    true,  B => { });
            BlocksConfig_Ecf    = ReadEcf("BlocksConfig.ecf", true,  B => { });
            ItemsConfig_Ecf     = ReadEcf("ItemsConfig.ecf",  true,  B => { });
            TokenConfig_Ecf     = ReadEcf("TokenConfig.ecf",  false, B => { });
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
                    try {
                        BlockIdMapping = JsonConvert.DeserializeObject<Dictionary<string, int>>(File.ReadAllText(blockMappingFile)); 
                    } 
                    catch {
                        Log($"ReadBlockMappingFile: BlockIdMapping File{blockMappingFile}:[{BlockIdMapping?.Count}]", LogLevel.Message);
                    }
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

        private EcfFile ReadEcf(string filename, bool withIdMapping, Action<EcfBlock> mapId)
        {
            var result = new EcfFile();
            try
            {
                var fullFilename = GetConfigurationFile(filename);
                Log($"EmpyrionScripting {fullFilename}: start", LogLevel.Message);
                result = Parse.Deserialize(File.ReadAllLines(fullFilename));
                result.Blocks.ForEach(mapId);
                if (withIdMapping && BlockIdMapping != null) Parse.ReplaceWithMappedIds(result, BlockIdMapping);
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

                    ScanTemplates(templateRootBlock, ressList, 1, new string [] { });

                    if (ressList.Count > 0 && !templates.ContainsKey(id)) templates.Add(id, ressList);
                });

            return templates;
        }

        private void ScanTemplates(EcfBlock templateRootBlock, Dictionary<int, double> ressList, double multiplyer, string[] scanedNames)
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
                            if (scanedNames.Contains(C.Name)) Log($"Cyclic Templates: {string.Join(",", scanedNames)} -> {C.Name}", LogLevel.Error);
                            else                              ScanTemplates(recipe, ressList, recipeMultiplyer, scanedNames.Concat(new[] {C.Name}).ToArray());
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

        public IReadOnlyDictionary<int, EcfBlock> BlocksById(IEnumerable<EcfBlock> blocks) =>
            blocks.EcfBlocksToDictionary(B => (B.Name == "Block" || B.Name == "Item") && B.Values?.ContainsKey("Id") == true,
                                         B => (int)B.Values["Id"]);

        public IReadOnlyDictionary<string, EcfBlock> BlocksByName(IEnumerable<EcfBlock> blocks) =>
            blocks.EcfBlocksToDictionary(B => (B.Name == "Block" || B.Name == "Item") && B.Values?.ContainsKey("Name") == true,
                                         B => B.Values["Name"]?.ToString());

        public IReadOnlyDictionary<int, EcfBlock> TokensById(IEnumerable<EcfBlock> blocks) =>
            blocks.EcfBlocksToDictionary(B => B.Name == "Token" && B.Values?.ContainsKey("Id") == true,
                                         B => (int)B.Values["Id"]);
        public IReadOnlyDictionary<string, EcfBlock> TemplatesByName(IEnumerable<EcfBlock> blocks) =>
            blocks.EcfBlocksToDictionary(B => B.Name == "Template" && B.Values?.ContainsKey("Name") == true,
                                         B => B.Values["Name"]?.ToString());

    }
}
