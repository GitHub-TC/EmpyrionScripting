using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Eleon.Modding;
using EmpyrionScripting.Interface;

namespace EmpyrionScripting.DataWrapper
{
    public sealed class EventStore : IDisposable, IEventStore
    {
        readonly IEntity _Entity;
        readonly ConcurrentDictionary<string, List<ISignalEventBase>> _Events = new ConcurrentDictionary<string, List<ISignalEventBase>>();

        public EventStore(IEntity entity)
        {
            _Entity = entity;
            if (_Entity?.Structure != null) _Entity.Structure.SignalChanged += Structure_SignalChanged;
        }

        public ConcurrentDictionary<string, List<ISignalEventBase>> GetEvents() => _Events;

        private void Structure_SignalChanged(string name, bool newState, int triggeringEntityId)
        {
            ISignalEventBase e = new SignalEventBase()
            {
                Name = name,
                TimeStamp = DateTime.Now,
                State = newState,
                TriggeredByEntityId = triggeringEntityId,
            };

            GetEvents().AddOrUpdate(name, N => new List<ISignalEventBase>() { e }, (N, L) =>
            {
                L.Add(e);
                if (L.Count > EmpyrionScripting.Configuration?.Current?.MaxStoredEventsPerSignal) L.RemoveAt(0);
                return L;
            });
        }

        public void Dispose()
        {
            if (_Entity?.Structure != null) _Entity.Structure.SignalChanged -= Structure_SignalChanged;
        }
    }
}
