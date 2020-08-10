using EcfParser;
using EmpyrionScripting.CustomHelpers;
using EmpyrionScripting.Interface;
using System.Collections.Generic;

namespace EmpyrionScripting.CsHelper
{
    public partial class CsScriptFunctions
    {
        public IItemsData[] Items(IStructureData structure, string names) => ItemAccessHelpers.Items(structure, names);

        public object ConfigFindAttribute(int id, string name) => EmpyrionScripting.ConfigEcfAccess.FindAttribute(id, name);
        public EcfBlock ConfigById(int id) => EmpyrionScripting.ConfigEcfAccess.FlatConfigBlockById.TryGetValue(id, out var block) ? block : null;
        public EcfBlock ConfigByName(string name) => EmpyrionScripting.ConfigEcfAccess.FlatConfigBlockByName.TryGetValue(name, out var block) ? block : null;
        public Dictionary<int, int> ResourcesForBlockById(int id) => EmpyrionScripting.ConfigEcfAccess.ResourcesForBlockById.TryGetValue(id, out var recipe) ? recipe : null;
    }
}
