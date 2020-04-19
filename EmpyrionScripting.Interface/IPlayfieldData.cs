using System.Collections.Generic;

namespace EmpyrionScripting.Interface
{
    public interface IPlayfieldData
    {
        bool IsPvP { get; }
        string Name { get; }
        string PlanetClass { get; }
        string PlanetType { get; }
        IEnumerable<ILimitedPlayerData> Player { get; }
        IEnumerable<ILimitedPlayerData> Players { get; }
        string PlayfieldType { get; }
    }
}