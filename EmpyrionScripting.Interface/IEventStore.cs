using System.Collections.Concurrent;
using System.Collections.Generic;

namespace EmpyrionScripting.Interface
{
    public interface IEventStore
    {
        ConcurrentDictionary<string, List<ISignalEventBase>> GetEvents();
    }
}