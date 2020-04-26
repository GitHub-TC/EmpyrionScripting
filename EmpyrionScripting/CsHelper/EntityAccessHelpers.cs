using EmpyrionScripting.CustomHelpers;
using EmpyrionScripting.DataWrapper;
using EmpyrionScripting.Interface;
using System.Collections.Generic;
using System.Linq;

namespace EmpyrionScripting.CsHelper
{
    public partial class CsScriptFunctions
    {
        public IEnumerable<IEntityData> EntitiesByName(params string[] names) => EntitiesByName(string.Join(";", names));
        public IEnumerable<IEntityData> EntitiesByName(string names) => Root.GetEntities().Where(E => new[] { E.Name }.GetUniqueNames(names).Any()).Select(E => new EntityData(Root.GetCurrentPlayfield(), E));
        public IEnumerable<IEntityData> EntitiesById(params int[] ids) => EntitiesByName(string.Join(";", ids.ToString()));
        public IEnumerable<IEntityData> EntitiesById(string ids) => Root.GetEntities().Where(E => new[] { E.Id.ToString() }.GetUniqueNames(ids).Any()).Select(E => new EntityData(Root.GetCurrentPlayfield(), E));
    }
}
