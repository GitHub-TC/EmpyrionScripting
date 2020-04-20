using Eleon.Modding;
using EmpyrionScripting.CustomHelpers;
using System.Collections.Generic;
using System.Linq;

namespace EmpyrionScripting.CsHelper
{
    public partial class CsScriptFunctions
    {
        public IEnumerable<IEntity> EntitiesByName(string names) => Root.Entites.Where(E => new[] { E.Id.ToString() }.GetUniqueNames(names).Any());
    }
}
