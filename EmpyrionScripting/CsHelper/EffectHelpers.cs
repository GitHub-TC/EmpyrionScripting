using EmpyrionScripting.CustomHelpers;
using EmpyrionScripting.Internal.Interface;
using System.Collections.Generic;

namespace EmpyrionScripting.CsHelper
{
    public partial class CsScriptFunctions
    {
        public IList<string> Scroll(string content, int lines, int delay, int step = 1) => EffectHelpers.Scroll(ScriptRoot as IScriptRootData, content, lines, delay, step);
    }
}
