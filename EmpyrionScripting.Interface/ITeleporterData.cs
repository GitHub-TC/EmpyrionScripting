namespace EmpyrionScripting.Interface
{
    public interface ITeleporterData
    {
        string Destination { get; set; }
        string DeviceName { get; set; }
        byte Origin { get; set; }
        string Playfield { get; set; }
        string SolarSystemName { get; set; }
        string Target { get; set; }
    }
}