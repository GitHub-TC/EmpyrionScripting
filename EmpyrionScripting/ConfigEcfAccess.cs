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
        public EcfFile Configuration_Ecf { get; set; }
        public EcfFile Config_Ecf { get; set; }
        public Dictionary<int, EcfBlock> ConfigBlockById { get; set; }
        public Dictionary<string, EcfBlock> ConfigBlockByName { get; set; }
        public static Action<string, LogLevel> Log { get; set; } = (s, l) => Console.WriteLine(s);

        public void ReadConfigEcf(string contentPath)
        {
            try
            {
                Configuration_Ecf = EcfParser.Parse.Deserialize(File.ReadAllLines(Path.Combine(contentPath, "Configuration", "Config_Example.ecf")));

                Log($"EmpyrionScripting Config_Example.ecf: #{Configuration_Ecf.Blocks.Count}", LogLevel.Message);
            }
            catch (Exception error)
            {
                Log($"EmpyrionScripting Config_Example.ecf: {error}", LogLevel.Error);
            }

            try
            {
                Config_Ecf = EcfParser.Parse.Deserialize(File.ReadAllLines(Path.Combine(contentPath, "Configuration", "Config.ecf")));

                Log($"EmpyrionScripting Config.ecf: #{Config_Ecf.Blocks.Count}", LogLevel.Message);
            }
            catch (Exception error)
            {
                Log($"EmpyrionScripting Config.ecf: {error}", LogLevel.Error);
            }

            try
            {
                Configuration_Ecf.MergeWith(Config_Ecf);
            }
            catch (Exception error)
            {
                Log($"EmpyrionScripting MergeWith: {error}", LogLevel.Error);
            }

            try
            {
                ConfigBlockById = Configuration_Ecf.Blocks
                    .Where(B => (B.Name == "Block" || B.Name == "Item") && B.Attributes.Any(A => A.Name == "Id"))
                    .ToDictionary(B => (int)B.Attributes.First(A => A.Name == "Id").Value, B => B);
            }
            catch (Exception error)
            {
                ConfigBlockById = new Dictionary<int, EcfBlock>();
                Log($"EmpyrionScripting ConfigBlockById: {error}", LogLevel.Error);
            }

            try 
            { 
                ConfigBlockByName = Configuration_Ecf.Blocks
                    .Where(B => (B.Name == "Block" || B.Name == "Item") && B.Attributes.Any(A => A.Name == "Id" && A.AdditionalPayload != null && A.AdditionalPayload.Any(a => a.Key == "Name")))
                    .ToDictionary(B => B.Attributes.First(A => A.Name == "Id").AdditionalPayload.First(A => A.Key == "Name").Value.ToString(), B => B);
            }
            catch (Exception error)
            {
                ConfigBlockByName = new Dictionary<string, EcfBlock>();
                Log($"EmpyrionScripting ConfigBlockByName: {error}", LogLevel.Error);
            }

            Log($"EmpyrionScripting Configuration_Ecf: #{Configuration_Ecf?.Blocks?.Count} BlockById: #{ConfigBlockById?.Count} BlockByName: #{ConfigBlockByName?.Count}", LogLevel.Message);
        }

    }
}
