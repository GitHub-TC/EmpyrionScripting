using EmpyrionScripting.CustomHelpers;
using EmpyrionScripting.DataWrapper;
using EmpyrionScripting.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmpyrionScripting.CsHelper
{
    public partial class CsScriptFunctions
    {
        public IBlockData[] Devices(IStructureData structure, string names) => BlockHelpers.Devices(structure, names);
        public IBlockData[] DevicesOfType(IStructureData structure, DeviceTypeName deviceType) => BlockHelpers.DevicesOfType(structure, deviceType);
        public IBlockData Block(IStructureData structure, int x, int y, int z) => new BlockData(structure.GetCurrent(), new Eleon.Modding.VectorInt3(x, y, z));
    }
}
