using Eleon.Modding;

namespace EmpyrionScripting.Interface
{
    public interface ISignalData
    {
        VectorInt3? BlockPos { get; }
        int Index { get; }
        string Name { get; }
        bool State { get; }
    }
}