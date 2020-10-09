using Eleon.Modding;
using UnityEngine;

namespace EmpyrionScripting.Interface
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
        bool IsLocal { get; }
        bool IsPoi { get; }
        bool IsProxy { get; }
        Vector3 Forward { get; }
        int BelongsTo { get; }
        int DockedTo { get; }

        IEntity GetCurrent();
        IPlayfield GetCurrentPlayfield();
    }
}