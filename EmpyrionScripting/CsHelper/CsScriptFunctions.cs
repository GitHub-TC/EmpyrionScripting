using EmpyrionScripting.Interface;
using EmpyrionScripting.Internal.Interface;
using System;

namespace EmpyrionScripting.CsHelper
{
    public partial class CsScriptFunctions
    {
        public IScriptRootData Root { get; set; }

        public CsScriptFunctions(IScriptRootData root){ Root = root; }

        public bool FunctionNeedsMainThread(Exception error) => FunctionNeedsMainThread(error, Root);

        public static bool FunctionNeedsMainThread(Exception error, object rootObject)
        {
            if (string.IsNullOrEmpty(error?.Message) || error.Message.IndexOf("can only be called from the main thread", StringComparison.InvariantCultureIgnoreCase) == -1) return false;
            if (rootObject is IScriptModData root)
            {
                if (root.ScriptWithinMainThread) return false;
                root.ScriptNeedsMainThread = true;
                return true;
            }
            else return false;
        }


    }
}
