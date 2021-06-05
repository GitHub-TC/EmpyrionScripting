using System.Collections.Concurrent;
using Eleon.Modding;
using UnityEngine;

namespace EmpyrionScripting.Interface
{
    public interface IStructureData
    {
        string[] AllCustomDeviceNames { get; }
        ISignalData[] ControlPanelSignals { get; }
        ISignalData[] BlockSignals { get; }
        ConcurrentDictionary<string, IContainerSource> ContainerSource { get; }
        float DamageLevel { get; }
        IEntityData[] DockedE { get; }
        IEntityData E { get; }
        string[] GetDeviceTypeNames { get; }
        bool IsOfflineProtectable { get; }
        bool IsPowerd { get; }
        bool IsReady { get; }
        IItemsData[] Items { get; }
        IStructureTankWrapper OxygenTank { get; }
        IStructureTankWrapper FuelTank { get; }
        IStructureTankWrapper PentaxidTank { get; }
        IPlayerData[] Passengers { get; }
        IPlayerData Pilot { get; }
        VectorInt3 MinPos { get; }
        VectorInt3 MaxPos { get; }
        int BlockCount { get; }
        int TriangleCount { get; }
        int LightCount { get; }
        float Fuel { get; }
        int PowerConsumption { get; }
        int PowerOutCapacity { get; }
        int SizeClass { get; }
        bool IsShieldActive { get; }
        int ShieldLevel { get; }
        float TotalMass { get; }
        bool HasLandClaimDevice { get; }
        ulong LastVisitedTicks { get; }
        string PlayerCreatedSteamId { get; }

        VectorInt3 GlobalToStructPos(Vector3 globalPos);
        Vector3 StructToGlobalPos(VectorInt3 structPos);

        IStructure GetCurrent();
    }
}