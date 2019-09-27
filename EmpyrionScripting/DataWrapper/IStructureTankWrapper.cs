namespace EmpyrionScripting.DataWrapper
{
    public enum StructureTankType
    {
        Oxygen,
        Fuel,
        Pentaxid
    }

    public interface IStructureTankWrapper
    {
        float Capacity { get; }
        float Content { get; }
        bool AllowedItem(int itemId);
        int AddItems(int itemId, int count);
        int ItemsNeededForFill(int itemId, int maxLimit);
    }
}