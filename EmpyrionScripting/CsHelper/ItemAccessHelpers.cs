using EmpyrionScripting.CustomHelpers;
using EmpyrionScripting.DataWrapper;

namespace EmpyrionScripting.CsHelper
{
    public partial class CsScriptFunctions
    {
        public ItemsData[] Items(IStructureData structure, string names)
        {
            return ItemAccessHelpers.Items(structure, names);
        }
    }
}
