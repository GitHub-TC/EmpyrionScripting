using EcfParser;
using EmpyrionScripting.CustomHelpers;
using EmpyrionScripting.Interface;

namespace EmpyrionScripting.CsHelper
{
    public partial class CsScriptFunctions
    {
        public IItemsData[] Items(IStructureData structure, string names) => ItemAccessHelpers.Items(structure, names);

        public object ConfigFindAttribute(int id, string name) => EmpyrionScripting.Configuration_Ecf.FindAttribute(id, name);
        public EcfBlock ConfigFindBlockById(int id) => EmpyrionScripting.Configuration_Ecf.FindBlockById(id);
        public EcfBlock ConfigFindBlockByName(string name) => EmpyrionScripting.Configuration_Ecf.FindBlockByName(name);
    }
}
