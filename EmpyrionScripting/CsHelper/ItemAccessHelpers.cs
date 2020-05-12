using EcfParser;
using EmpyrionScripting.CustomHelpers;
using EmpyrionScripting.Interface;

namespace EmpyrionScripting.CsHelper
{
    public partial class CsScriptFunctions
    {
        public IItemsData[] Items(IStructureData structure, string names) => ItemAccessHelpers.Items(structure, names);

        public object ConfigFindAttribute(int id, string name) => EmpyrionScripting.ConfigEcfAccess.FindAttribute(id, name);
        public EcfBlock ConfigById(int id) => EmpyrionScripting.ConfigEcfAccess.FlatConfigBlockById.TryGetValue(id, out var block) ? block : null;
        public EcfBlock ConfigByName(string name) => EmpyrionScripting.ConfigEcfAccess.FlatConfigBlockByName.TryGetValue(name, out var block) ? block : null;
    }
}
