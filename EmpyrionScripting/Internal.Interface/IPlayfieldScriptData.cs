using Eleon.Modding;
using EmpyrionScripting.Interface;
using System.Collections.Concurrent;

namespace EmpyrionScripting.Internal.Interface
{
    public interface IExclusiveAccess
    {
        string CommandId { get; }
        string EntityName { get; }
        int EntityId { get; }
    }

    public interface IPlayfieldScriptData
    {
        IEntity[] AllEntities { get; }
        IEntity[] CurrentEntities { get; }
        ConcurrentDictionary<int, IEventStore> EventStore { get; }
        ConcurrentDictionary<string, object> PersistendData { get; }
        ConcurrentDictionary<int, IExclusiveAccess> EntityExclusiveAccess { get; }
        IPlayfield Playfield { get; }
        string PlayfieldName { get; }
        ConcurrentQueue<IItemMoveInfo> MoveLostItems { get; }
    }
}