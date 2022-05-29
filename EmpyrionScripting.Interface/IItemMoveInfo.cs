namespace EmpyrionScripting.Interface
{

    public interface IItemMoveInfo : IItemBase
    {
        int Count { get; }
        int Ammo { get; }
        int Decay { get; }
        string Destination { get; }
        IEntityData DestinationE { get; }
        string Error { get; }
        string Source { get; }
        IEntityData SourceE { get; }
    }
}