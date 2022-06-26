using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using Microsoft.CodeAnalysis;
using System;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Scripting.Hosting;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;
using System.Threading;
using System.Linq;
using System.Reflection;

namespace RoslynCsCompiler
{
    public class CompilerAccess
    {
        public static Action<string> Log { get; set; }
        public static Exception LastError { get; set; }

        public class CompileResult<T>
        {
            public Script<T> Script { get; set; }
            public Compilation Compilation { get; set; }
            public AnalysisResult AnalysisResult { get; set; }
        }

        public static async Task<CompileResult<T>> CompileAsync<T> (string scriptString, ScriptOptions optionsDefault, Type rootType, InteractiveAssemblyLoader loader, DiagnosticAnalyzer diagnosticAnalyzer)
        {
            try
            {
                var def = ScriptMetadataResolver.Default;

                var reType = AppDomain.CurrentDomain.GetAssemblies()
                           .Where(a => a.FullName.Contains("Microsoft.CodeAnalysis.Scripting"))
                           .Select(t =>
                           {
                               try
                               {
                                   return t.GetType("Microsoft.CodeAnalysis.Scripting.Hosting.RuntimeMetadataReferenceResolver");
                               }
                               catch (Exception error)
                               {
                                   Log($"CompileAsync: Microsoft.CodeAnalysis.Scripting.Hosting.RuntimeMetadataReferenceResolver: {error}");
                                   return null;
                               }
                           })
                           .FirstOrDefault();

                var reConstrutor = reType.GetConstructors(BindingFlags.NonPublic | BindingFlags.Instance).FirstOrDefault();

                var imString              = reConstrutor.GetParameters().First().ParameterType;
                var searchPaths           = Activator.CreateInstance(imString);
                var platformAssemblyPaths = Activator.CreateInstance(imString);

                var re           = reConstrutor.Invoke(new object[] { searchPaths, null, null, null, platformAssemblyPaths, null });

                var meType = AppDomain.CurrentDomain.GetAssemblies()
                           .Where(a => a.FullName.Contains("Microsoft.CodeAnalysis.Scripting"))
                           .Select(t =>
                           {
                               try
                               {
                                   return t.GetType("Microsoft.CodeAnalysis.Scripting.ScriptMetadataResolver");
                               }
                               catch (Exception error)
                               {
                                   Log($"CompileAsync: Microsoft.CodeAnalysis.Scripting.ScriptMetadataResolver: {error}");
                                   return null;
                               }
                           })
                           .FirstOrDefault();

                var meConstrutor = meType.GetConstructors(BindingFlags.NonPublic | BindingFlags.Instance).FirstOrDefault();
                var me = meConstrutor.Invoke(new[] { re }) as ScriptMetadataResolver;

                var options = optionsDefault
                    .WithMetadataResolver(me);

                var script = CSharpScript.Create<T>(scriptString, options, rootType, loader);
                var compilation = script.GetCompilation();
                return new CompileResult<T>
                {
                    Script         = script,
                    Compilation    = compilation,
                    AnalysisResult = await compilation
                                        .WithAnalyzers(ImmutableArray.Create(diagnosticAnalyzer))
                                        .GetAnalysisResultAsync(CancellationToken.None)
                };
            }
            catch (Exception error)
            {
                LastError = error;
                Log($"CompileAsync: {error}");
                return null;
            }
        }
    }
}
