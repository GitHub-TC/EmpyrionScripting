using EcfParser;
using EmpyrionScripting.CustomHelpers;
using EmpyrionScripting.Interface;
using System.Collections.Generic;
using System.Linq;

namespace EmpyrionScripting.CsHelper
{
    public partial class CsScriptFunctions
    {
        public IItemsData[] Items(IStructureData structure, string names) => ItemAccessHelpers.Items(ScriptRoot, structure, names);

        public object ConfigFindAttribute(int id, string name) => EmpyrionScripting.ConfigEcfAccess.FindAttribute(id, name);
        public int ConfigId(string name) => EmpyrionScripting.ConfigEcfAccess.FlatConfigBlockByName.TryGetValue(name, out var config) ? (int)config.Attr?.FirstOrDefault(A => A.Name == "Id")?.Value : 0;
        public EcfBlock ConfigById(int id) => EmpyrionScripting.ConfigEcfAccess.FlatConfigBlockById.TryGetValue(id, out var block) ? block : null;
        public EcfBlock ConfigByName(string name) => EmpyrionScripting.ConfigEcfAccess.FlatConfigBlockByName.TryGetValue(name, out var block) ? block : null;
        public Dictionary<int, int> ResourcesForBlockById(int id) => EmpyrionScripting.ConfigEcfAccess.ResourcesForBlockById.TryGetValue(id, out var recipe) ? recipe : null;
    }
}
