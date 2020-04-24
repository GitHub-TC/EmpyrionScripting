using EmpyrionScripting.CustomHelpers;
using EmpyrionScripting.DataWrapper;
using EmpyrionScripting.Interface;
using System.Collections.Generic;
using System.Linq;

namespace EmpyrionScripting.CsHelper
{
    public partial class CsScriptFunctions
    {
        public IEnumerable<IEntityData> EntitiesByName(string names) => Root.GetEntities().Where(E => new[] { E.Id.ToString() }.GetUniqueNames(names).Any()).Select(E => new EntityData(Root.GetCurrentPlayfield(), E));
    }
}
