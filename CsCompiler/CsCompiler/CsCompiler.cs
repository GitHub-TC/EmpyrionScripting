using Eleon.Modding;
using EmpyrionNetAPIDefinitions;
using EmpyrionNetAPITools;
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
        public IModApi ModApi { get; }
        public Assembly MainAssembly { get; }

        public event EventHandler ConfigurationChanged;

        static IEnumerable<string> IgnoreErrorsForDlls { get; } = new[] { "RoslynCsCompiler.resources.dll", "System.Runtime.Loader.dll", "Microsoft.VisualStudio.TestPlatform.TestFramework.resources.dll" };

        public ConcurrentDictionary<string, LoadedAssemblyInfo> CustomAssemblies { get; set; } = new ConcurrentDictionary<string, LoadedAssemblyInfo>();
        public ConcurrentDictionary<string, LoadedAssemblyInfo> MainAssemblies   { get; set; } = new ConcurrentDictionary<string, LoadedAssemblyInfo>();

        public class LoadedAssemblyInfo
        {
            public string FullAssemblyDllName { get; set; }
            public Assembly LoadedAssembly { get; set; }
        }

        public CsCompiler(string saveGameModPath, Eleon.Modding.IModApi modApi, Assembly mainAssembly)
        {
            SaveGameModPath = saveGameModPath;
            ModApi = modApi;
            MainAssembly = mainAssembly;
            LoadConfiguration();

            AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;
        }

        private Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            int nameEndPos = args.Name.IndexOf(',');
            var dllPath = Path.Combine(Path.GetDirectoryName(typeof(CsCompiler).Assembly.Location), nameEndPos == -1 ? args.Name : args.Name.Substring(0, nameEndPos) + ".dll");

            try
            {
                return Assembly.LoadFrom(dllPath);
            }
            catch (Exception error)
            {
                if (IgnoreErrorsForDlls.Contains(Path.GetFileName(dllPath))) return GetType().Assembly;

                if (Log == null) Console.WriteLine($"CurrentDomain_AssemblyResolve: ({dllPath}) {sender}:{args.Name} for {args.RequestingAssembly?.FullName} -> {error}");
                else             Log($"CurrentDomain_AssemblyResolve: ({dllPath}) {sender}:{args.Name} for {args.RequestingAssembly?.FullName} -> {error}", LogLevel.Error);
                return null;
            }
        }

        public Action<IScriptRootModData, List<string>> ScriptErrorTracking { get; set; } = (m, l) => { };
        public static Action<string, LogLevel> Log { get; set; }

        private void LoadConfiguration()
        {
            if(ModApi != null) ConfigurationManager<CsCompilerConfiguration>.Log = ModApi.Log;

            Configuration = new ConfigurationManager<CsCompilerConfiguration>()
            {
                ConfigFilename = Path.Combine(SaveGameModPath, "CsCompilerConfiguration.json")
            };
            Configuration.ConfigFileLoaded += (o, a) =>
            {
                try
                {
                    CheckCustomAssemblies();
                    CheckMainAssemblies(Configuration?.Current?.AssemblyReferences.Concat(DefaultConfiguration?.Current?.AssemblyReferences));
                    ConfigurationChanged?.Invoke(this, EventArgs.Empty);
                }
                catch (Exception error)
                {
                    Log?.Invoke($"CsCompilerConfiguration.ConfigFileLoaded: {Configuration.ConfigFilename} -> {error}", LogLevel.Error);
                }
            };

            Configuration.Load();
            if(Configuration.LoadException == null || !File.Exists(Configuration.ConfigFilename)) Configuration.Save();

            DefaultConfiguration = new ConfigurationManager<CsCompilerConfiguration>()
            {
                ConfigFilename = Path.Combine(Path.GetDirectoryName(MainAssembly.Location), "DefaultCsCompilerConfiguration.json")
            };
            DefaultConfiguration.ConfigFileLoaded += (o, a) =>
            {
                try
                {
                    CheckMainAssemblies(Configuration?.Current?.AssemblyReferences.Concat(DefaultConfiguration?.Current?.AssemblyReferences));
                    ConfigurationChanged?.Invoke(this, EventArgs.Empty);
                }
                catch (Exception error)
                {
                    Log?.Invoke($"DefaultCsCompilerConfiguration.ConfigFileLoaded: {DefaultConfiguration.ConfigFilename} -> {error}", LogLevel.Error);
                }
            };
            DefaultConfiguration.Load();

            Log?.Invoke($"GAC:{typeof(object).Assembly.GlobalAssemblyCache} Mono:{IsRunningOnMono}", LogLevel.Message);
        }

        private static bool IsRunningOnMono
        {
            get
            {
                try{ return !(Type.GetType("Mono.Runtime") is null);                 }
                catch { return false; }
            }
        }

        private void CheckCustomAssemblies()
        {
            var processed = CustomAssemblies.Select(dll => dll.Key).ToList();

            Configuration?.Current?.CustomAssemblies?.ForEach(dll => {
                string dllPath = dll;
                try
                {
                    dllPath = Path.Combine(SaveGameModPath, dll).NormalizePath();
                    if (!CustomAssemblies.ContainsKey(dllPath)) LoadCustomAssembly(CustomAssemblies, SaveGameModPath, dllPath);
                    else                                        processed.Remove(dllPath);
                }
                catch (Exception error)
                {
                    Log?.Invoke($"CheckCustomAssemblies: {dll} ({dllPath}) -> {error}", LogLevel.Error);
                }
            });

            processed.ForEach(dll => CustomAssemblies.TryRemove(dll, out var customAssembly));
        }

        public static void LoadCustomAssembly(ConcurrentDictionary<string, LoadedAssemblyInfo> customAssemblies, string saveGameModPath, string dll)
        {
            string dllPath = dll;
            try
            {
                dllPath = Path.Combine(saveGameModPath, dll).NormalizePath();
                Log?.Invoke($"CustomAssemblyLoad: {dll} ({dllPath})", LogLevel.Message);
                var loadedAssembly = Assembly.LoadFile(dllPath);
                LoadedAssemblyInfo current = null;
                customAssemblies.AddOrUpdate(dllPath,
                    current = new LoadedAssemblyInfo() { FullAssemblyDllName = dllPath, LoadedAssembly = loadedAssembly }, 
                    (d, a) => { current = a;  a.LoadedAssembly = loadedAssembly; return a; });
            }
            catch (Exception error)
            {
                Log?.Invoke($"AssemblyLoad: {dll} ({dllPath}) -> {error}", LogLevel.Error);
            }
        }

        private void CheckMainAssemblies(IEnumerable<string> assemblyReferences)
        {
            var processed = MainAssemblies.Select(dll => dll.Key).ToList();

            assemblyReferences.ForEach(dll => {
                try
                {
                    if (!MainAssemblies.ContainsKey(dll)) LoadMainAssembly(dll);
                    else                                  processed.Remove(dll);
                }
                catch (Exception error)
                {
                    Log?.Invoke($"CheckMainAssemblies: {dll} -> {error}", LogLevel.Error);
                }
            });

            processed.ForEach(dll => MainAssemblies.TryRemove(dll, out var mainAssembly));
        }

        private void LoadMainAssembly(string dll)
        {
            string dllPath = null;

            try
            {
                Log?.Invoke($"MainAssemblyLoad: {dll} ({dllPath})", LogLevel.Message);
                Assembly loadedAssembly = null;
                try
                {
                    // direkte Angabe
                    dllPath = dll.EndsWith(".dll", StringComparison.InvariantCultureIgnoreCase) ? dll : dll + ".dll";
                    loadedAssembly = Assembly.LoadFrom(dllPath);
                }
                catch
                {
                    try
                    {
                        // DLL im Modverzeichnis
                        dllPath = Path.Combine(Path.GetDirectoryName(MainAssembly.Location), Path.GetFileName(dllPath));
                        loadedAssembly = Assembly.LoadFrom(dllPath);
                    }
                    catch
                    {
                        // DLL im Empyrionverzeichnis
                        dllPath = Path.Combine(Path.GetDirectoryName(typeof(IModApi).Assembly.Location), Path.GetFileName(dllPath));
                        loadedAssembly = Assembly.LoadFrom(dllPath);
                    }
                }

                LoadedAssemblyInfo current = null;
                MainAssemblies.AddOrUpdate(dllPath,
                    current = new LoadedAssemblyInfo() { FullAssemblyDllName = dllPath, LoadedAssembly = loadedAssembly },
                    (d, a) => { current = a; a.LoadedAssembly = loadedAssembly; return a; });
            }
            catch (Exception error)
            {
                Log?.Invoke($"MainAssemblyLoad: {dll} ({dllPath}) -> {error}", LogLevel.Error);
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

        public Func<object, string> GetExec<T>(CsModPermission csScriptsAllowed, T rootObjectCompileTime, string script) where T : IScriptRootModData
        {
            if (csScriptsAllowed == CsModPermission.None) return o => "Sorry the C# scripting is deactivate on this server.";

            var messages = new List<string>();
            bool success = true;
            MethodInfo mainMethod = null;

            Script<object> csScript = null;
            CsModPermission permissionNeeded;
            var rootCompileTime = rootObjectCompileTime as IScriptModData;

            using (var loader = new InteractiveAssemblyLoader())
            {
                ScriptOptions options;
                try
                {
                    options = ScriptOptions.Default;
                }
                catch (Exception optionsError)
                {
                    Log($"GetExec:ScriptOptions:Init: {optionsError}", LogLevel.Error);
                    throw;
                }

                try
                {
                    options = options
                            .WithAllowUnsafe(false)
                            .WithEmitDebugInformation(Configuration.Current.DebugMode)
                            .WithCheckOverflow(true)
                            .WithOptimizationLevel(Configuration.Current.DebugMode ? OptimizationLevel.Debug : OptimizationLevel.Release)

                            .WithImports(DefaultConfiguration.Current.Usings)
                            .AddImports(Configuration.Current.Usings)

                            .WithReferences(DefaultConfiguration.Current.AssemblyReferences)
                            .AddReferences(Configuration.Current.AssemblyReferences)
                            .WithReferences(MainAssembly, typeof(IScriptModData).Assembly, typeof(IMod).Assembly, typeof(IModApi).Assembly)
                            .AddReferences(MainAssemblies.Values.Select(A => A.LoadedAssembly))
                            .AddReferences(CustomAssemblies.Values.Select(A => A.LoadedAssembly));
                }
                catch (Exception optionsError)
                {
                    Log($"GetExec:ScriptOptions:Configure: {optionsError}", LogLevel.Error);
                    throw;
                }

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
                            else
                            {
                                result.Diagnostics
                                    .Where(diag => diag.Id != "CS7022")
                                    .ForEach(diag =>
                                {
                                    messages.Add(diag.ToString());
                                });
                            }
                        }
                        catch (Exception loadError)
                        {
                            messages.Add($"Assembly:{loadError}");
                        }
                    }
                }
            }

            if (messages.Count > 0)
            {
                Log?.Invoke($"C# Compile [{rootCompileTime.ScriptId}]:{string.Join("\n", messages)}", LogLevel.Error);

                ScriptErrorTracking(rootObjectCompileTime, messages);
            }

            return rootObject =>
            {
                if (!success) return string.Join("\n", messages);

                var root = rootObject as IScriptRootModData;
                if (csScriptsAllowed == CsModPermission.SaveGame && !(root is IScriptSaveGameRootData))                      return "C# scripts are only allowed in SaveGameScripts";
                if (csScriptsAllowed == CsModPermission.Admin    && root.E.GetCurrent().Faction.Group != FactionGroup.Admin) return "C# scripts are only allowed on admin structures";

                if (permissionNeeded == CsModPermission.SaveGame && !(root is IScriptSaveGameRootData))                      return "This script is only allowed in SaveGameScripts";
                if (permissionNeeded == CsModPermission.Admin    && root.E.GetCurrent().Faction.Group != FactionGroup.Admin) return "This script is only allowed on admin structures";

                string exceptionMessage = null;
                using (var output = new StringWriter())
                {
                    root.ScriptOutput = output;

                    try
                    {
                        object result = null;

                        if (mainMethod != null)
                        {
                            if(root.CsRoot is ICsScriptRootFunctions csRoot) csRoot.ScriptRoot = root;
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

                        return exceptionMessage == null ? output.ToString() : $"{exceptionMessage}\n\nScript output up to exception:\n{output}";
                    }
                    catch (Exception error)
                    {
                        exceptionMessage = error.ToString();
                        return root.IsElevatedScript ? error.ToString() : error.Message;
                    }
                    finally
                    {
                        if (!string.IsNullOrEmpty(exceptionMessage))
                        {
                            Log?.Invoke($"C# Run [{root.ScriptId}]:{exceptionMessage}\n{output}", LogLevel.Error);

                            ScriptErrorTracking(rootObjectCompileTime, new []{ exceptionMessage }.ToList());
                        }
                    }
                }
            };
        }

    }
}
