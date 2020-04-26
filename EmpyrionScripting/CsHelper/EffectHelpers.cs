using EmpyrionScripting.CustomHelpers;
using System.Collections.Generic;

namespace EmpyrionScripting.CsHelper
{
    public partial class CsScriptFunctions
    {
        public IList<string> Scroll(string content, int lines, int delay, int step = 1) => EffectHelpers.Scroll(Root, content, lines, delay, step);
    }
}
