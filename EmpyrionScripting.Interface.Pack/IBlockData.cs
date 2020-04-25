using Eleon.Modding;

namespace EmpyrionScripting.Interface
{
    public interface IBlockData
    {
        bool Active { get; set; }
        int Bottom { get; set; }
        string CustomName { get; }
        int Damage { get; }
        object Device { get; }
        int East { get; set; }
        int HitPoints { get; }
        int Id { get; }
        int? LockCode { get; }
        int North { get; set; }
        VectorInt3 Position { get; }
        int Rotation { get; set; }
        int Shape { get; set; }
        int South { get; set; }
        bool SwitchState { get; set; }
        int Top { get; set; }
        int West { get; set; }
    }
}