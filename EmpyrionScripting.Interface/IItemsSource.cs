using Eleon.Modding;

namespace EmpyrionScripting.Interface
{
    public interface IItemsSource
    {
        IContainer Container { get; set; }
        int Count { get; set; }
        string CustomName { get; set; }
        IEntityData E { get; set; }
        int Id { get; set; }
        VectorInt3 Position { get; set; }
    }
}