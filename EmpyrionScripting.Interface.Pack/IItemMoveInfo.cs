﻿namespace EmpyrionScripting.Interface
{
    public interface IItemMoveInfo
    {
        int Count { get; }
        string Destination { get; }
        IEntityData DestinationE { get; }
        string Error { get; }
        int Id { get; }
        string Source { get; }
        IEntityData SourceE { get; }
    }
}