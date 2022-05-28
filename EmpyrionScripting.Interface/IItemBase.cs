namespace EmpyrionScripting.Interface
{
    public interface IItemBase
    {
        int Id { get; }
        int ItemId { get; }
        int TokenId { get; }
        bool IsToken { get; }
    }
}