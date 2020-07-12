using Eleon.Modding;
using EmpyrionScripting.CustomHelpers;
using EmpyrionScripting.DataWrapper;
using EmpyrionScripting.Interface;
using System.Linq;

namespace EmpyrionScripting.CsHelper
{
    public partial class CsScriptFunctions
    {
        public IBlockData[] Devices(IStructureData structure, string names) => BlockHelpers.Devices(structure, names);
        public IBlockData[] DevicesOfType(IStructureData structure, DeviceTypeName deviceType) => BlockHelpers.DevicesOfType(structure, deviceType);
        public IBlockData Block(IStructureData structure, int x, int y, int z) => new BlockData(structure.GetCurrent(), new Eleon.Modding.VectorInt3(x, y, z));
        public T[] GetDevices<T>(IStructureData structure, string names) where T : class, IDevice
            => BlockHelpers.Devices(structure, names)
            .OfType<BlockData>()
            .Select(B => B?.GetStructure()?.GetDevice<T>(B.Position))
            .Where(B => B != null)
            .ToArray();
        public T[] GetDevices<T>(params IBlockData[] block) where T : class, IDevice 
            => block.OfType<BlockData>()
            .Select(B => B?.GetStructure()?.GetDevice<T>(B.Position))
            .Where(B => B != null)
            .ToArray();
    }
}
