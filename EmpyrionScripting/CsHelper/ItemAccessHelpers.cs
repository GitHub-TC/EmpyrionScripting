using EmpyrionScripting.CustomHelpers;
using EmpyrionScripting.DataWrapper;
using EmpyrionScripting.Interface;

namespace EmpyrionScripting.CsHelper
{
    public partial class CsScriptFunctions
    {
        public IItemsData[] Items(IStructureData structure, string names)
        {
            return ItemAccessHelpers.Items(structure, names);
        }
    }
}
