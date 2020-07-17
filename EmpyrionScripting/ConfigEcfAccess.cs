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

            Log($"EmpyrionScripting Configuration_Ecf: #{Configuration_Ecf?.Blocks?.Count} BlockById: #{ConfigBlockById?.Count} BlockByName: #{ConfigBlockByName?.Count}", LogLevel.Message);
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

    }
}
