using EmpyrionNetAPITools;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;

namespace EmpyrionScripting.CsCompiler
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class WhitelistDiagnosticAnalyzer : DiagnosticAnalyzer
    {
#pragma warning disable RS2008 // Enable analyzer release tracking
        internal static readonly DiagnosticDescriptor PROHIBITED_OBJECT_RULE
            = new DiagnosticDescriptor("ProhibitedLanguageElement", "Prohibited Language Element", "The language element '{0}' is prohibited", "Whitelist", DiagnosticSeverity.Error, true);
#pragma warning restore RS2008 // Enable analyzer release tracking

        public WhitelistDiagnosticAnalyzer(
            ConfigurationManager<CsCompilerConfiguration> defaultConfiguration, 
            ConfigurationManager<CsCompilerConfiguration> configuration, 
            ConfigurationManager<CsCompilerConfiguration> unkownConfiguration)
        {
            DefaultConfiguration = defaultConfiguration;
            Configuration        = configuration;
            UnkownConfiguration  = unkownConfiguration;
        }

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(PROHIBITED_OBJECT_RULE);

        public ModPermission PermissionNeeded { get; set; }
        public ConfigurationManager<CsCompilerConfiguration> DefaultConfiguration { get; }
        public ConfigurationManager<CsCompilerConfiguration> Configuration { get; }
        public ConfigurationManager<CsCompilerConfiguration> UnkownConfiguration { get; }
        public bool UnkownConfigurationIsChanged { get; set; }

        public override void Initialize(AnalysisContext context)
        {
            context.EnableConcurrentExecution();
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze | GeneratedCodeAnalysisFlags.ReportDiagnostics);
            context.RegisterSyntaxNodeAction(Analyze,
                SyntaxKind.AliasQualifiedName,
                SyntaxKind.QualifiedName,
                SyntaxKind.GenericName,
                SyntaxKind.IdentifierName);
        }

        void Analyze(SyntaxNodeAnalysisContext context)
        {
            var node = context.Node;

            if (IsQualifiedName(node.Parent)) return;

            var info = context.SemanticModel.GetSymbolInfo(node);
            if (info.Symbol == null || info.Symbol.IsInSource()) return;

            var fullName = info.Symbol.GetFullMetadataName();
            if (DefaultConfiguration.Current.Symbols.Value.TryGetValue(fullName, out var defaultPermission))
            {
                if (PermissionNeeded < defaultPermission) PermissionNeeded = defaultPermission;
                return;
            }

            if (Configuration.Current.Symbols.Value.TryGetValue(fullName, out var permission))
            {
                if (PermissionNeeded < permission) PermissionNeeded = permission;
                return;
            }

            if (Configuration.Current.WithinLearnMode)
            {
                UnkownConfiguration.Current.Symbols.Value.TryAdd(fullName, ModPermission.SaveGame);
                UnkownConfigurationIsChanged = true;
            }
            else
            {
                var diagnostic = Diagnostic.Create(PROHIBITED_OBJECT_RULE, node.GetLocation(), info.Symbol.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat));
                context.ReportDiagnostic(diagnostic);
            }

            PermissionNeeded = ModPermission.SaveGame;  // Im SaveGame ist alles erlaubt
        }

        bool IsQualifiedName(SyntaxNode arg)
        {
            switch (arg.Kind())
            {
                default: return false;
                case SyntaxKind.QualifiedName:
                case SyntaxKind.AliasQualifiedName: return true;
            }
        }

    }
}
