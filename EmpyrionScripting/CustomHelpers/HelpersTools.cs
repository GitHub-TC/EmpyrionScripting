using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmpyrionScripting.CustomHelpers
{
    public static class HelpersTools
    {
        public static Dictionary<string, string> GetUniqueNames(this StructureData structure, string namesSearch)
        {
            var names = new List<string>();

            namesSearch
                .Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(N => N.Trim())
                .ForEach(N =>
                {
                    if (N == "*") names.AddRange(structure.AllCustomDeviceNames);
                    else if (N.EndsWith("*"))
                    {
                        var startsWith = N.Substring(0, N.Length - 1);
                        names.AddRange(structure.AllCustomDeviceNames.Where(SN => SN.StartsWith(startsWith)));
                    }
                    else names.AddRange(structure.AllCustomDeviceNames.Where(SN => SN == N));
                });

            return names.GroupBy(N => N).ToDictionary(N => N.Key, N => N.Key);
        }

    }
}
