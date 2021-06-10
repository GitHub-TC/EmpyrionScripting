using System.Collections.Generic;
using System.Linq;
using Eleon.Modding;
using EmpyrionScripting.Interface;

namespace EmpyrionScripting.DataWrapper
{
    public class PlayfieldData : IPlayfieldData
    {
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

        public VectorInt3 SolarSystemCoordinates => playfield.SolarSystemCoordinates;
        public string SolarSystemName            => playfield.SolarSystemName;

        public float GetTerrainHeightAt(float x, float z) => playfield.GetTerrainHeightAt(x, z);

        public ILimitedPlayerData[] Players => _p == null ? _p = playfield.Players.Values.Select(P => new LimitedPlayerData(P)).ToArray() : _p;
        ILimitedPlayerData[] _p;
    }
}
