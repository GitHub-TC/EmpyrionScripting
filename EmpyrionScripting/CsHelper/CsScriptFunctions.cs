using EmpyrionScripting.Internal.Interface;

namespace EmpyrionScripting.CsHelper
{
    public partial class CsScriptFunctions
    {
        IScriptRootData Root { get; }

        public CsScriptFunctions(IScriptRootData root){ Root = root; }
    }
}
