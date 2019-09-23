using System;

namespace EmpyrionScripting.DataWrapper
{
    public class SignalEvent : SignalEventBase
    {
        public SignalEvent(SignalEventBase signalBase) : base(signalBase) {}

        public IEntityData TriggeredByShip => EmpyrionScripting.ModApi.Playfield.Entities.TryGetValue(TriggeredByEntityId, out var entity) ? new EntityData(entity, true) : null;
        public LimitedPlayerData TriggeredByPlayer => EmpyrionScripting.ModApi.Playfield.Players.TryGetValue(TriggeredByEntityId, out var player) ? new LimitedPlayerData(player) : null;
    }
}