using Microsoft.CodeAnalysis;
using System.Linq;
using System.Text;

namespace EmpyrionScripting.CsCompiler
{
    public static class AnalysisExtensions
    {
        public static string GetFullMetadataName(this ISymbol s)
        {
            if (s == null || s.IsRootNamespace()) return string.Empty;

            var sb = new StringBuilder(s.MetadataName);
            var last = s;

            s = s.ContainingSymbol;

            while (!IsRootNamespace(s))
            {
                if (s is ITypeSymbol && last is ITypeSymbol) sb.Insert(0, '+');
                else                                         sb.Insert(0, '.');

                sb.Insert(0, s.OriginalDefinition.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat));
                s = s.ContainingSymbol;
            }

            return sb.ToString();
        }

        private static bool IsRootNamespace(this ISymbol symbol)
        {
            return (symbol is INamespaceSymbol s) && s.IsGlobalNamespace;
        }

        public static bool IsInSource(this ISymbol symbol) => symbol.Locations.All(L => L.IsInSource);

    }
}
