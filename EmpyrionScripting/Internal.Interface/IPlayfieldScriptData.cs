using Eleon.Modding;
using EmpyrionScripting.Interface;
using System.Collections.Concurrent;

namespace EmpyrionScripting.Internal.Interface
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