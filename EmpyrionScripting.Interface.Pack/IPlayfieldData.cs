using System.Collections.Generic;

namespace EmpyrionScripting.Interface
{
    public interface IPlayfieldData
    {
        bool IsPvP { get; }
        string Name { get; }
        string PlanetClass { get; }
        string PlanetType { get; }
        string PlayfieldType { get; }
        IEnumerable<ILimitedPlayerData> Players { get; }
    }
}