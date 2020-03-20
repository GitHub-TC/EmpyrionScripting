using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Eleon.Modding;

namespace EmpyrionScripting.DataWrapper
{
    public sealed class EventStore : IDisposable
    {
        readonly IEntity _Entity;
        readonly ConcurrentDictionary<string, List<SignalEventBase>> _Events = new ConcurrentDictionary<string, List<SignalEventBase>>();

        public EventStore(IEntity entity)
        {
            _Entity = entity;
            if (_Entity?.Structure != null) _Entity.Structure.SignalChanged += Structure_SignalChanged;
        }

        public ConcurrentDictionary<string, List<SignalEventBase>> GetEvents() => _Events;

        private void Structure_SignalChanged(string name, bool newState, int triggeringEntityId)
        {
            var e = new SignalEventBase() {
                Name                = name,
                TimeStamp           = DateTime.Now,
                State               = newState,
                TriggeredByEntityId  = triggeringEntityId,
            };

            GetEvents().AddOrUpdate(name, N => new List<SignalEventBase>() { e }, (N, L) => {
                L.Add(e);
                if (L.Count > EmpyrionScripting.Configuration?.Current?.MaxStoredEventsPerSignal) L.RemoveAt(0);
                return L;
            } );
        }

        public void Dispose()
        {
            if(_Entity?.Structure != null) _Entity.Structure.SignalChanged -= Structure_SignalChanged;
        }
    }
}
