using Eleon.Modding;

namespace EmpyrionScripting.Interface
{
    public interface IItemsSource : IItemBase
    {
        IContainer Container { get; set; }
        int Count { get; set; }
        int Ammo { get; set; }
        int Decay { get; set; }
        string CustomName { get; set; }
        IEntityData E { get; set; }
        VectorInt3 Position { get; set; }
        int MaxSlots { get; set; }
    }
}