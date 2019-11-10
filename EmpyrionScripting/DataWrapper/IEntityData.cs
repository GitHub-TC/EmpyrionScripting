using Eleon.Modding;
using UnityEngine;

namespace EmpyrionScripting.DataWrapper
{
    public interface IEntityData
    {
        string[] DeviceNames { get; }
        EntityType EntityType { get; }
        FactionData Faction { get; }
        int Id { get; }
        string Name { get; }
        Vector3 Pos { get; }
        IStructureData S { get; }

        IEntity GetCurrent();
        IPlayfield GetCurrentPlayfield();
    }
}