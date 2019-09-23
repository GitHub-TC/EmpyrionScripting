using System.Collections.Concurrent;
using Eleon.Modding;

namespace EmpyrionScripting.DataWrapper
{
    public interface IStructureData
    {
        string[] AllCustomDeviceNames { get; }
        SignalData[] ControlPanelSignals { get; }
        SignalData[] BlockSignals { get; }
        ConcurrentDictionary<string, ContainerSource> ContainerSource { get; }
        float DamageLevel { get; }
        IEntityData[] DockedE { get; }
        IEntityData E { get; }
        string[] GetDeviceTypeNames { get; }
        bool IsOfflineProtectable { get; }
        bool IsPowerd { get; }
        bool IsReady { get; }
        ItemsData[] Items { get; }
        IStructureTankWrapper OxygenTank { get; }
        IStructureTankWrapper FuelTank { get; }
        IStructureTankWrapper PentaxidTank { get; }
        PlayerData[] Passengers { get; }
        PlayerData Pilot { get; }

        IStructure GetCurrent();
    }
}