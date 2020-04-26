using EmpyrionNetAPITools;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using System;
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
            ConfigurationManager<CsSymbolsConfiguration> unkownConfiguration)
        {
            DefaultConfiguration = defaultConfiguration;
            Configuration = configuration;
            UnkownConfiguration  = unkownConfiguration;
        }

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(PROHIBITED_OBJECT_RULE);

        public CsModPermission PermissionNeeded { get; set; }
        public ConfigurationManager<CsCompilerConfiguration> DefaultConfiguration { get; }
        public ConfigurationManager<CsCompilerConfiguration> Configuration { get; }
        public ConfigurationManager<CsSymbolsConfiguration> UnkownConfiguration { get; }
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
            var genericTypePos = fullName.IndexOf('`');
            if (genericTypePos >= 0) fullName = fullName.Substring(0, genericTypePos);

            if (FoundPermission(fullName)) return;
            if (FoundPermissionWithWildcard(fullName)) return;

            if (Configuration.Current.WithinLearnMode)
            {
                UnkownConfiguration.Current.Symbols.Value.TryAdd(fullName, CsModPermission.SaveGame);
                UnkownConfigurationIsChanged = true;
            }

            PermissionNeeded = CsModPermission.SaveGame;  // Im SaveGame ist alles erlaubt
        }

        private bool FoundPermissionWithWildcard(string name)
        {
            var testName = name;
            while(true)
            {
                if (DefaultConfiguration.Current.Symbols.Value.TryGetValue(testName + ".*", out var defaultPermission))
                {
                    if (PermissionNeeded < defaultPermission) PermissionNeeded = defaultPermission;
                    return true;
                }

                if (Configuration.Current.Symbols.Value.TryGetValue(testName + ".*", out var permission))
                {
                    if (PermissionNeeded < permission) PermissionNeeded = permission;
                    return true;
                }

                var lastDotPos = testName.LastIndexOf('.');
                if (lastDotPos > 0) testName = testName.Substring(0, lastDotPos);
                else                return false;
            };
        }

        private bool FoundPermission(string name)
        {
            if (DefaultConfiguration.Current.Symbols.Value.TryGetValue(name, out var defaultPermission))
            {
                if (PermissionNeeded < defaultPermission) PermissionNeeded = defaultPermission;
                return true;
            }

            if (Configuration.Current.Symbols.Value.TryGetValue(name, out var permission))
            {
                if (PermissionNeeded < permission) PermissionNeeded = permission;
                return true;
            }

            return false;
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
