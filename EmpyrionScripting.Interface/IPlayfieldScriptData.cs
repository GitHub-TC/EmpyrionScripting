using Eleon.Modding;
using System.Collections.Concurrent;

namespace EmpyrionScripting.Interface
{
    public interface IPlayfieldScriptData
    {
        IEntity[] AllEntities { get; set; }
        IEntity[] CurrentEntities { get; set; }
        ConcurrentDictionary<int, IEventStore> EventStore { get; set; }
        ConcurrentDictionary<string, object> PersistendData { get; set; }
        IPlayfield Playfield { get; set; }
        string PlayfieldName { get; set; }
    }
}