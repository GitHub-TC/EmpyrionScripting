namespace EmpyrionScripting.Interface
{

    public interface IItemMoveInfo : IItemBase
    {
        int Count { get; }
        string Destination { get; }
        IEntityData DestinationE { get; }
        string Error { get; }
        string Source { get; }
        IEntityData SourceE { get; }
    }
}