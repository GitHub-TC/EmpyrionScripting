using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Linq;

namespace EmpyrionScripting.CsCompiler
{
    public class SymbolList : ConcurrentDictionary<string, ModPermission> { }

    public class CsCompilerConfiguration
    {
        public CsCompilerConfiguration()
        {
            Symbols = new Lazy<SymbolList>(() => 
                SymbolPermissions.Keys
                    .SelectMany(permission => SymbolPermissions[permission].Select(item => (permission, item)))
                    .Aggregate(new SymbolList(), 
                        (list, data) => { list.AddOrUpdate(data.item, data.permission, (i, p) => data.permission); return list; })
            );
        }

        public bool WithinLearnMode { get; set; }
        public string[] Usings { get; set; } = new string[] { };
        public string[] AssemblyReferences { get; set; } = new string[] { };
        public ConcurrentDictionary<ModPermission, string[]> SymbolPermissions { get; set; } = new ConcurrentDictionary<ModPermission, string[]>();

        [JsonIgnore]
        public Lazy<SymbolList> Symbols;

        public void AddNewSymbols()
        {
            AddNewSymbols(ModPermission.SaveGame, Symbols.Value.Where(P => P.Value == ModPermission.SaveGame).Select(P => P.Key).ToArray());
            AddNewSymbols(ModPermission.Admin,    Symbols.Value.Where(P => P.Value == ModPermission.Admin   ).Select(P => P.Key).ToArray());
            AddNewSymbols(ModPermission.Player,   Symbols.Value.Where(P => P.Value == ModPermission.Player  ).Select(P => P.Key).ToArray());
        }

        private void AddNewSymbols(ModPermission permission, string[] values)
        {
            SymbolPermissions.AddOrUpdate(permission, values, (p, v) => v.Concat(values).Distinct().OrderBy(N => N).ToArray());
        }
    }
}