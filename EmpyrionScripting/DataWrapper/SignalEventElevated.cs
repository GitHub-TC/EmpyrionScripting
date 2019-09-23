using System;

namespace EmpyrionScripting.DataWrapper
{
    public class SignalEventElevated : SignalEventBase
    {
        public SignalEventElevated(SignalEventBase signalBase) : base(signalBase) {}
        public IEntityData TriggeredByShip => EmpyrionScripting.ModApi.Playfield.Entities.TryGetValue(TriggeredByEntityId, out var entity) ? new EntityData(entity, false) : null;
        public PlayerData TriggeredByPlayer => EmpyrionScripting.ModApi.Playfield.Players.TryGetValue(TriggeredByEntityId, out var player) ? new PlayerData(player) : null;
    }
}