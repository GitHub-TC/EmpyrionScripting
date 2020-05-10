using EcfParser;
using EmpyrionScripting.CustomHelpers;
using EmpyrionScripting.Interface;

namespace EmpyrionScripting.CsHelper
{
    public partial class CsScriptFunctions
    {
        public IItemsData[] Items(IStructureData structure, string names) => ItemAccessHelpers.Items(structure, names);

        public object ConfigFindAttribute(int id, string name) => EmpyrionScripting.ConfigEcfAccess.FindAttribute(id, name);
        public EcfBlock ConfigFindBlockById(int id) => EmpyrionScripting.ConfigEcfAccess.ConfigBlockById.TryGetValue(id, out var block) ? block : null;
        public EcfBlock ConfigFindBlockByName(string name) => EmpyrionScripting.ConfigEcfAccess.ConfigBlockByName.TryGetValue(name, out var block) ? block : null;
    }
}
