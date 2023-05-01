using Eleon.Modding;

namespace EmpyrionScripting.Interface
{
    public interface IPlayfieldData
    {
        bool IsPvP { get; }
        string Name { get; }
        string PlanetClass { get; }
        string PlanetType { get; }
        string PlayfieldType { get; }
        ILimitedPlayerData[] Players { get; }
        VectorInt3 SolarSystemCoordinates { get; }
        string SolarSystemName { get; }
        IPlayfieldDetails Details { get; }

        float GetTerrainHeightAt(float x, float z);
    }
}