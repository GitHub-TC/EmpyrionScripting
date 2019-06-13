using System.Collections.Generic;
using System.Linq;
using Eleon.Modding;

namespace EmpyrionScripting
{
    public class PlayfieldData
    {
        private IPlayfield playfield;

        public PlayfieldData()
        {
        }
        public PlayfieldData(IPlayfield playfield)
        {
            this.playfield = playfield;
        }

        public string Name => playfield.Name;
        public string PlayfieldType => playfield.PlayfieldType;
        public string PlanetType => playfield.PlanetType;
        public string PlanetClass => playfield.PlanetClass;
        public bool IsPvP => playfield.IsPvP;

        public IEnumerable<LimitedPlayerData> Player => _p == null ? _p = playfield.Players.Values.Select(P => new LimitedPlayerData(P)) : _p;
        IEnumerable<LimitedPlayerData> _p;
        
    }
}
