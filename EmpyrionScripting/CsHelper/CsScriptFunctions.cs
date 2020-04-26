using EmpyrionScripting.Internal.Interface;

namespace EmpyrionScripting.CsHelper
{
    public partial class CsScriptFunctions
    {
        public IScriptRootData Root { get; set; }

        public CsScriptFunctions(IScriptRootData root){ Root = root; }
    }
}
