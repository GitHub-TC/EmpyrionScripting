using Eleon.Modding;

namespace EmpyrionScripting.DataWrapper
{
    public class SignalEvent : SignalEventBase
    {
        private readonly IPlayfield _CurrentPlayfield;
        public SignalEvent(IPlayfield playfield, SignalEventBase signalBase) : base(signalBase) {
            _CurrentPlayfield = playfield;
        }

        public IEntityData TriggeredByShip => _CurrentPlayfield.Entities.TryGetValue(TriggeredByEntityId, out var entity) ?  (entity.Type != EntityType.Player ? new EntityData(_CurrentPlayfield, entity, true) : null) : null;
        public LimitedPlayerData TriggeredByPlayer => _CurrentPlayfield.Players.TryGetValue(TriggeredByEntityId, out var player) ? new LimitedPlayerData(player) : null;
    }
}