using Eleon.Modding;
using System;

namespace EmpyrionScripting.DataWrapper
{
    public class SignalEventElevated : SignalEventBase
    {
        private readonly IPlayfield _CurrentPlayfield;

        public SignalEventElevated(IPlayfield playfield, SignalEventBase signalBase) : base(signalBase) {
            _CurrentPlayfield = playfield;
        }
        public IEntityData TriggeredByShip => _CurrentPlayfield.Entities.TryGetValue(TriggeredByEntityId, out var entity) ? new EntityData(_CurrentPlayfield, entity, false) : null;
        public PlayerData TriggeredByPlayer => _CurrentPlayfield.Players.TryGetValue(TriggeredByEntityId, out var player) ? new PlayerData(player) : null;

    }
}