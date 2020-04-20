using EmpyrionScripting.CustomHelpers;
using EmpyrionScripting.Interface;

namespace EmpyrionScripting.CsHelper
{
    public partial class CsScriptFunctions
    {
        public IItemsData[] Items(IStructureData structure, string names) => ItemAccessHelpers.Items(structure, names);
    }
}
