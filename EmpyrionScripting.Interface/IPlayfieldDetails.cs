namespace EmpyrionScripting.Interface
{
    public interface IPlayfieldDetails
    {
        bool AtmosphereBreathable { get; }
        double AtmosphereDensity { get; }
        double AtmosphereO2 { get; }
        string Description { get; }
        double Gravity { get; }
        double PlanetAxis { get; }
        int PlanetSize { get; }
        string PlanetType { get; }
        string PlayfieldType { get; }
        double Radiation { get; }
        int[] TemperatureMinMax { get; }
    }
}