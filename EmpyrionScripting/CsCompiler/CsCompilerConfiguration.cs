using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Linq;

namespace EmpyrionScripting.CsCompiler
{
    public class SymbolList : ConcurrentDictionary<string, CsModPermission> { }

    public class CsSymbolsConfiguration
    {
        public CsSymbolsConfiguration()
        {
            Symbols = new Lazy<SymbolList>(() => 
                SymbolPermissions.Keys
                    .SelectMany(permission => SymbolPermissions[permission].Select(item => (permission, item)))
                    .Aggregate(new SymbolList(), 
                        (list, data) => { list.AddOrUpdate(data.item, data.permission, (i, p) => data.permission); return list; })
            );
        }

        public ConcurrentDictionary<CsModPermission, string[]> SymbolPermissions { get; set; } = new ConcurrentDictionary<CsModPermission, string[]>();

        [JsonIgnore]
        public Lazy<SymbolList> Symbols;

        public void AddNewSymbols()
        {
            AddNewSymbols(CsModPermission.SaveGame, Symbols.Value.Where(P => P.Value == CsModPermission.SaveGame).Select(P => P.Key).ToArray());
            AddNewSymbols(CsModPermission.Admin,    Symbols.Value.Where(P => P.Value == CsModPermission.Admin   ).Select(P => P.Key).ToArray());
            AddNewSymbols(CsModPermission.Player,   Symbols.Value.Where(P => P.Value == CsModPermission.Player  ).Select(P => P.Key).ToArray());
        }

        private void AddNewSymbols(CsModPermission permission, string[] values)
        {
            SymbolPermissions.AddOrUpdate(permission, values, (p, v) => v.Concat(values).Distinct().OrderBy(N => N).ToArray());
        }
    }

    public class CsCompilerConfiguration : CsSymbolsConfiguration
    {
        public bool WithinLearnMode { get; set; }
        public string[] CustomAssemblies { get; set; } = new string[] { };
        public string[] Usings { get; set; } = new string[] { };
        public string[] AssemblyReferences { get; set; } = new string[] { };
        public void PrepareForSave()
        {
            AddNewSymbols();
            Usings              = Usings.OrderBy(N => N).ToArray();
            AssemblyReferences  = AssemblyReferences.OrderBy(N => N).ToArray();
        }
    }
}