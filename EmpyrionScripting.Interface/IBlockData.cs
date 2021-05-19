using Eleon.Modding;

namespace EmpyrionScripting.Interface
{
    public interface IBlockData
    {
        bool Active { get; set; }
        string CustomName { get; }
        int Damage { get; }
        object Device { get; }
        int East { get; set; }
        int HitPoints { get; }
        int Id { get; }
        int BlockType { get; set; }
        int? LockCode { get; }
        VectorInt3 Position { get; }
        int Rotation { get; set; }
        int Shape { get; set; }
        bool SwitchState { get; set; }

        int Top { get; set; }
        int Bottom { get; set; }
        int West { get; set; }
        int South { get; set; }
        int North { get; set; }

        int TopColor { get; set; }
        int BottomColor { get; set; }
        int NorthColor { get; set; }
        int SouthColor { get; set; }
        int WestColor { get; set; }
        int EastColor { get; set; }
        bool IsDamaged { get; }
        string SendSignalName { get; }
        bool SignalState { get; }

        void SetColorForWholeBlock(int texIdx);
        void SetColors(int? top, int? bottom, int? north, int? south, int? west, int? east);
        void SetTextureForWholeBlock(int texIdx);
        void SetTextures(int? top, int? bottom, int? north, int? south, int? west, int? east);
    }
}