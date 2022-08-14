namespace EmpyrionScripting.Interface
{
    public interface IHarvestInfo
    {
        int Id { get; }
        string Name { get; }
        int ChildOnHarvestId { get; }
        int DropOnHarvestCount { get; }
        int DropOnHarvestId { get; }
        string DropOnHarvestItem { get; }
        string ChildOnHarvestItem { get; }
        int PickupTargetId { get; }
    }
}