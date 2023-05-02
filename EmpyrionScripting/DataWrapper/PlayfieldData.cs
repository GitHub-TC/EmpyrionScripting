using System;
using System.Collections.Concurrent;
using System.Linq;
using Eleon.Modding;
using EmpyrionScripting.Interface;
using YamlDotNet.Serialization;
using System.IO;
using EmpyrionScripting.CustomHelpers;

namespace EmpyrionScripting.DataWrapper
{
    public class PlayfieldDetails : IPlayfieldDetails
    {
        public string PlayfieldType { get; set; }
        public string PlanetType { get; set; }
        public int PlanetSize { get; set; }
        public double PlanetAxis { get; set; }
        public string Description
        {
            get => _Description;
            set => _Description = value?.FormatToHtml();
        }
        string _Description;

        public double Gravity { get; set; }
        public int[] TemperatureMinMax { get; set; }
        public int TemperatureDay { get; set; }
        public int TemperatureMin => TemperatureMinMax == null || TemperatureMinMax.Length == 0 ? TemperatureDay : TemperatureMinMax.FirstOrDefault();
        public int TemperatureMax => TemperatureMinMax == null || TemperatureMinMax.Length == 0 ? TemperatureDay : TemperatureMinMax.LastOrDefault();
        public bool AtmosphereBreathable { get; set; }
        public double AtmosphereO2 { get; set; }
        public double AtmosphereDensity { get; set; }
        public double Radiation { get; set; }

    }


    public class PlayfieldData : IPlayfieldData
    {
        static ConcurrentDictionary<string, PlayfieldDetails> playfieldInfos = new ConcurrentDictionary<string, PlayfieldDetails>();
        static IDeserializer yamlDeserializer { get; } = new DeserializerBuilder()
            .IgnoreUnmatchedProperties()
            .Build();

        private IPlayfield playfield;

        public PlayfieldData()
        {
        }
        public PlayfieldData(IPlayfield playfield)
        {
            this.playfield = playfield;
        }

        public IPlayfield GetCurrent() => playfield;

        public string Name => playfield.Name;
        public string PlayfieldType => playfield.PlayfieldType;
        public string PlanetType => playfield.PlanetType;
        public string PlanetClass => playfield.PlanetClass;
        public bool IsPvP => playfield.IsPvP;

        public IPlayfieldDetails Details => playfieldInfos.GetOrAdd(Name, ReadPlayfieldInfo);

        public static PlayfieldDetails ReadPlayfieldInfo(string playfieldName)
        {
            try
            {
                return yamlDeserializer.Deserialize<PlayfieldDetails>(File.ReadAllText(Path.Combine(EmpyrionScripting.SaveGameModPath, "..", "..", "Templates", playfieldName, "playfield.yaml")));
            }
            catch (Exception error)
            {
                EmpyrionScripting.Log($"ReadPlayfieldInfo:{playfieldName} -> {error}", EmpyrionNetAPIDefinitions.LogLevel.Message);
                return null;
            }
        }

        public VectorInt3 SolarSystemCoordinates => playfield.SolarSystemCoordinates;
        public string SolarSystemName            => playfield.SolarSystemName;

        public float GetTerrainHeightAt(float x, float z) => playfield.GetTerrainHeightAt(x, z);

        public ILimitedPlayerData[] Players => _p == null ? _p = playfield.Players.Values.Select(P => new LimitedPlayerData(P)).ToArray() : _p;

        ILimitedPlayerData[] _p;
    }
}
