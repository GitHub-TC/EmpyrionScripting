using System.Collections.Concurrent;
using Eleon.Modding;

namespace EmpyrionScripting.DataWrapper
{
    public interface IStructureData
    {
        string[] AllCustomDeviceNames { get; }
        ConcurrentDictionary<string, ContainerSource> ContainerSource { get; }
        float DamageLevel { get; }
        IEntityData[] DockedE { get; }
        IEntityData E { get; }
        StructureTank FuelTank { get; }
        string[] GetDeviceTypeNames { get; }
        bool IsOfflineProtectable { get; }
        bool IsPowerd { get; }
        bool IsReady { get; }
        ItemsData[] Items { get; }
        StructureTank OxygenTank { get; }
        PlayerData[] Passengers { get; }
        PlayerData Pilot { get; }

        IStructure GetCurrent();
    }
}