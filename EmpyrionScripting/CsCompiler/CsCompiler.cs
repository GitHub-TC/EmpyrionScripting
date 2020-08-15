using EmpyrionNetAPIDefinitions;
using EmpyrionNetAPITools;
using EmpyrionScripting.CsHelper;
using EmpyrionScripting.DataWrapper;
using EmpyrionScripting.Interface;
using EmpyrionScripting.Internal.Interface;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Scripting;
using Microsoft.CodeAnalysis.Scripting.Hosting;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace EmpyrionScripting.CsCompiler
{
    public class CsCompiler
    {
        public ConfigurationManager<CsCompilerConfiguration> Configuration { get; set; } = new ConfigurationManager<CsCompilerConfiguration>() { Current = new CsCompilerConfiguration() };
        public ConfigurationManager<CsCompilerConfiguration> DefaultConfiguration { get; set; } = new ConfigurationManager<CsCompilerConfiguration>() { Current = new CsCompilerConfiguration() };
        public string SaveGameModPath { get; }
        public event EventHandler ConfigurationChanged;

        public ConcurrentDictionary<string, CustomAssembly> CustomAssemblies { get; set; } = new ConcurrentDictionary<string, CustomAssembly>();

        public class CustomAssembly
        {
            public string FullAssemblyDllName { get; set; }
            public Assembly LoadedAssembly { get; set; }
        }

        public CsCompiler(string saveGameModPath)
        {
            SaveGameModPath = saveGameModPath;

            LoadConfiguration();
        }

        public static Action<string, LogLevel> Log { get; set; }

        private void LoadConfiguration()
        {
            if(EmpyrionScripting.ModApi != null) ConfigurationManager<CsCompilerConfiguration>.Log = EmpyrionScripting.ModApi.Log;

            Configuration = new ConfigurationManager<CsCompilerConfiguration>()
            {
                ConfigFilename = Path.Combine(SaveGameModPath, "CsCompilerConfiguration.json")
            };
            Configuration.ConfigFileLoaded += (o, a) =>
            {
                try
                {
                    CheckCustomAssemblies();
                    ConfigurationChanged?.Invoke(this, EventArgs.Empty);
                }
                catch (Exception error)
                {
                    Log?.Invoke($"CsCompilerConfiguration.ConfigFileLoaded: {Configuration.ConfigFilename} -> {error}", LogLevel.Error);
                }
            };

            Configuration.Load();
            if(Configuration.LoadException != null) Configuration.Save();

            DefaultConfiguration = new ConfigurationManager<CsCompilerConfiguration>()
            {
                ConfigFilename = Path.Combine(Path.GetDirectoryName(typeof(EmpyrionScripting).Assembly.Location), "DefaultCsCompilerConfiguration.json")
            };
            DefaultConfiguration.ConfigFileLoaded += (o, a) => ConfigurationChanged?.Invoke(this, EventArgs.Empty); ;
            DefaultConfiguration.Load();
        }

        private void CheckCustomAssemblies()
        {
            var processed = CustomAssemblies.Select(dll => dll.Key).ToList();

            Configuration?.Current?.CustomAssemblies?.ForEach(dll => {
                string dllPath = dll;
                try
                {
                    dllPath = Path.Combine(SaveGameModPath, dll).NormalizePath();
                    if (!CustomAssemblies.ContainsKey(dllPath)) LoadCustomAssembly(dllPath);
                    else                                        processed.Remove(dllPath);
                }
                catch (Exception error)
                {
                    Log?.Invoke($"CheckCustomAssemblies: {dll} ({dllPath}) -> {error}", LogLevel.Error);
                }
            });

            processed.ForEach(dll => CustomAssemblies.TryRemove(dll, out var customAssembly));
        }

        private void LoadCustomAssembly(string dll)
        {
            string dllPath = dll;
            try
            {
                dllPath = Path.Combine(SaveGameModPath, dll).NormalizePath();
                Log?.Invoke($"CustomAssemblyLoad: {dll} ({dllPath})", LogLevel.Message);
                var loadedAssembly = Assembly.LoadFile(dllPath);
                CustomAssembly current = null;
                CustomAssemblies.AddOrUpdate(dllPath,
                    current = new CustomAssembly() { FullAssemblyDllName = dllPath, LoadedAssembly = loadedAssembly }, 
                    (d, a) => { current = a;  a.LoadedAssembly = loadedAssembly; return a; });
            }
            catch (Exception error)
            {
                Log?.Invoke($"CustomAssemblyLoad: {dll} ({dllPath}) -> {error}", LogLevel.Error);
            }
        }

        private void AnalyzeDiagnostics(ImmutableArray<Diagnostic> diagnostics, List<string> messages, ref bool success)
        {
            success = success && !diagnostics.Any(d => d.Severity == DiagnosticSeverity.Error);

            var orderedDiagnostics = diagnostics
                .Where(d => d.Severity >= DiagnosticSeverity.Warning)
                .OrderByDescending(d => d.Severity);

            foreach (var diagnostic in orderedDiagnostics)
            {
                var lineSpan = diagnostic.Location.GetMappedLineSpan();
                var message = string.Format("{0}({1},{2}): {3}: {4}",
                    lineSpan.Path,
                    lineSpan.StartLinePosition.Line + 1,
                    lineSpan.StartLinePosition.Character,
                    diagnostic.Severity == DiagnosticSeverity.Warning ? "Warning" : "ERROR",
                    diagnostic.GetMessage());

                messages.Add(message);
            }
        }

        internal Func<object, string> GetExec<T>(CsModPermission csScriptsAllowed, T rootObjectCompileTime, string script) where T : IScriptRootData
        {
            var messages = new List<string>();
            bool success = true;
            MethodInfo mainMethod = null;

            Script<object> csScript = null;
            CsModPermission permissionNeeded;
            var rootCompileTime = rootObjectCompileTime as IScriptRootData;

            using (var loader = new InteractiveAssemblyLoader())
            {
                var options = ScriptOptions.Default
                        .WithAllowUnsafe(false)
                        .WithEmitDebugInformation(false)
                        .WithCheckOverflow(true)
                        .WithOptimizationLevel(OptimizationLevel.Release)

                        .WithImports(DefaultConfiguration.Current.Usings)
                        .AddImports(Configuration.Current.Usings)

                        .WithReferences(DefaultConfiguration.Current.AssemblyReferences)
                        .AddReferences(Configuration.Current.AssemblyReferences)
                        .AddReferences(typeof(EmpyrionScripting).Assembly.Location, typeof(IScriptRootData).Assembly.Location)
                        .AddReferences(CustomAssemblies.Values.Select(A => A.LoadedAssembly));

                csScript = CSharpScript.Create<object>(script, options, typeof(IScriptModData), loader);
                var compilation = csScript.GetCompilation();

                var WhitelistDiagnosticAnalyzer = new WhitelistDiagnosticAnalyzer(DefaultConfiguration, Configuration);

                var analyzerCompilation = compilation
                    .WithAnalyzers(ImmutableArray.Create<DiagnosticAnalyzer>(WhitelistDiagnosticAnalyzer))
                    .GetAnalysisResultAsync(CancellationToken.None)
                    .GetAwaiter().GetResult().CompilationDiagnostics;

                analyzerCompilation.ForEach(A => AnalyzeDiagnostics(A.Value, messages, ref success));

                if (WhitelistDiagnosticAnalyzer.ConfigurationIsChanged)
                {
                    Configuration.Current.PrepareForSave();
                    Configuration.Save();

                    DefaultConfiguration.Current.PrepareForSave();
                    DefaultConfiguration.Save();
                }

                permissionNeeded = WhitelistDiagnosticAnalyzer.PermissionNeeded;

                Assembly assembly = null;

                if (compilation.Assembly.TypeNames.Contains("ModMain"))
                {
                    using (var assemblyStream = new MemoryStream())
                    {
                        try
                        {
                            var result = compilation.Emit(assemblyStream);
                            var resultSuccess = result.Success;

                            if (resultSuccess)
                            {
                                assembly = Assembly.ReflectionOnlyLoad(assemblyStream.ToArray());
                                var callMainType = assembly.GetTypes().SingleOrDefault(MT => MT.Name == "ModMain");
                                mainMethod = callMainType.GetMethod("Main");

                                if (mainMethod != null)
                                {
                                    assemblyStream.Seek(0, SeekOrigin.Begin);
                                    assembly = Assembly.Load(assemblyStream.ToArray());

                                    callMainType = assembly.GetTypes().SingleOrDefault(MT => MT.Name == "ModMain");
                                    mainMethod = callMainType.GetMethod("Main");
                                }
                            }
                        }
                        catch (Exception loadError)
                        {
                            messages.Add($"Assembly:{loadError}");
                        }
                    }
                }
            }

            if(messages.Count > 0) Log?.Invoke($"C# Compile [{rootCompileTime.ScriptId}]:{string.Join("\n", messages)}", LogLevel.Error);

            return rootObject =>
            {
                if (!success) return string.Join("\n", messages);

                var root = rootObject as IScriptRootData;
                if (csScriptsAllowed == CsModPermission.SaveGame && !(root is ScriptSaveGameRootData))                       return "C# scripts are only allowed in SaveGameScripts";
                if (csScriptsAllowed == CsModPermission.Admin    && root.E.GetCurrent().Faction.Group != FactionGroup.Admin) return "C# scripts are only allowed on admin structures";

                if (permissionNeeded == CsModPermission.SaveGame && !(root is ScriptSaveGameRootData))                       return "This script is only allowed in SaveGameScripts";
                if (permissionNeeded == CsModPermission.Admin    && root.E.GetCurrent().Faction.Group != FactionGroup.Admin) return "This script is only allowed on admin structures";

                string exceptionMessage = null;
                try
                {
                    using (var output = new StringWriter())
                    {
                        root.ScriptOutput = output;
                        object result = null;

                        if (mainMethod != null)
                        {
                            if(root.CsRoot is CsScriptFunctions csRoot) csRoot.Root = root;
                            result = mainMethod.Invoke(null, new[] { root as IScriptModData });
                        }
                        else
                        {
                            result = csScript
                                .RunAsync(root, ex => { exceptionMessage = $"Exception: {(root.IsElevatedScript ? ex.ToString() : ex.Message)}"; return true; })
                                .ConfigureAwait(true)
                                .GetAwaiter()
                                .GetResult()
                                .ReturnValue;
                        }

                        if (result is Action action)                            action();
                        else if (result is Action<IScriptModData> simpleaction) simpleaction(root);
                        else if (result is Func<IScriptModData, object> func)   output.Write(func(root)?.ToString());
                        else if (result is Task task)                           task.RunSynchronously();
                        else                                                    output.Write(result?.ToString());

                        return exceptionMessage ?? output.ToString();
                    }
                }
                catch (Exception error)
                {
                    exceptionMessage = error.ToString();
                    return root.IsElevatedScript ? error.ToString() : error.Message;
                }
                finally
                {
                    if (!string.IsNullOrEmpty(exceptionMessage)) Log?.Invoke($"C# Run [{root.ScriptId}]:{exceptionMessage}", LogLevel.Error);
                }
            };
        }
    }
}
