using Eleon.Modding;
using EmpyrionScripting.CsCompiler;
using EmpyrionScripting.Interface;
using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Reflection;
using static EmpyrionScripting.CsCompiler.CsCompiler;

namespace EmpyrionScripting
{
    public class SaveGamesScripts
    {
        public class PreCompiledScript : LoadedAssemblyInfo
        {
            public MethodInfo MainMethod { get; set; }
        }

        FileSystemWatcher SaveGameScriptsWatcher { get; set; }
        public ConcurrentDictionary<string, object> SaveGameScripts { get; private set; }
        public static ConcurrentDictionary<string, PreCompiledScript> PreCompiledScriptAssemblies { get; set; } = new ConcurrentDictionary<string, PreCompiledScript>();
        public string MainScriptPath { get; set; }
        public string SaveGameModPath { get; set; }
        public IModApi ModApi { get; }

        public SaveGamesScripts(IModApi modApi)
        {
            ModApi = modApi;
        }

        public void ReadSaveGamesScripts()
        {
            MainScriptPath = Path.Combine(SaveGameModPath, "Scripts");
            Directory.CreateDirectory(MainScriptPath);

            ModApi?.Log($"SaveGameScript: FileSystemWatcher: {MainScriptPath}");

            SaveGameScriptsWatcher = new FileSystemWatcher(MainScriptPath)
            {
                IncludeSubdirectories = true,
            };
            SaveGameScriptsWatcher.Changed += (S, E) => {
                if (!IsScriptFile(Path.GetExtension(E.Name).ToLower())) return;

                var filepath = E.FullPath.NormalizePath();

                if (File.Exists(filepath)) {
                    ModApi?.Log($"SaveGameScript: changed script: {filepath}");
                    SaveGameScripts.AddOrUpdate(filepath, F => File.ReadAllText(F), (F, C) => File.ReadAllText(F));
                }
                else
                {
                    ModApi?.Log($"SaveGameScript: deleted script: {filepath}");
                    SaveGameScripts.TryRemove(filepath, out _);
                }
            };
            SaveGameScriptsWatcher.Created += (S, E) =>
            {
                if (!IsScriptFile(Path.GetExtension(E.Name).ToLower())) return;

                ModApi?.Log($"SaveGameScript: created script: {E.FullPath.NormalizePath()}");
                SaveGameScripts.AddOrUpdate(E.FullPath.NormalizePath(), F => File.ReadAllText(F), (F, C) => File.ReadAllText(F));
            };
            SaveGameScriptsWatcher.Renamed += (S, E) =>
            {
                if (!IsScriptFile(Path.GetExtension(E.Name).ToLower())) return;

                ModApi?.Log($"SaveGameScript: renamed script: {E.OldFullPath.NormalizePath()} -> {E.FullPath.NormalizePath()}");
                SaveGameScripts.TryRemove(E.OldFullPath.NormalizePath(), out _);
                SaveGameScripts.AddOrUpdate(E.FullPath.NormalizePath(), F => File.ReadAllText(F), (F, C) => File.ReadAllText(F));
            };
            SaveGameScriptsWatcher.Deleted += (S, E) =>
            {
                if (!IsScriptFile(Path.GetExtension(E.Name).ToLower())) return;

                ModApi?.Log($"SaveGameScript: deleted script: {E.FullPath.NormalizePath()}");
                SaveGameScripts.TryRemove(E.FullPath.NormalizePath(), out _);
            };

            SaveGameScripts = new ConcurrentDictionary<string, object>(
                                    Directory.GetFiles(MainScriptPath, "*.*", SearchOption.AllDirectories)
                                    .Where(N => IsScriptFile(Path.GetExtension(N).ToLower()))
                                    .ToDictionary(F => F.NormalizePath(), F => Path.GetExtension(F).Equals(".dll", System.StringComparison.InvariantCultureIgnoreCase) ? LoadScriptAssembly(F) : (object)File.ReadAllText(F)));

            SaveGameScriptsWatcher.EnableRaisingEvents = true;

            SaveGameScripts.Keys.ForEach(F => ModApi?.Log($"SaveGameScript: found script: {F}"));
        }

        private PreCompiledScript LoadScriptAssembly(string dll)
        {
            if (PreCompiledScriptAssemblies.TryGetValue(dll, out var loadedAssemblyInfo)) return loadedAssemblyInfo;

            try
            {
                if (!PreCompiledScriptAssemblies.ContainsKey(dll)) loadedAssemblyInfo = LoadCustomAssembly(PreCompiledScriptAssemblies, SaveGameModPath, dll);

                var callMainType = loadedAssemblyInfo.LoadedAssembly.GetTypes().SingleOrDefault(MT => MT.Name == "ModMain");
                loadedAssemblyInfo.MainMethod = callMainType.GetMethod("Main");

                if(loadedAssemblyInfo.MainMethod == null) ModApi?.Log($"LoadScriptAssembly: No Main method in class ModMain {dll} ({dll}) -> {callMainType}");
                else                                      ModApi?.Log($"LoadScriptAssembly: Found Main method in class ModMain {dll} ({dll}) -> {callMainType}");

                return loadedAssemblyInfo;
            }
            catch (FileLoadException error)
            {
                ModApi?.Log($"LoadScriptAssembly: {dll} ({dll}) -> {error}");
            }
            catch (Exception error)
            {
                ModApi?.Log($"LoadScriptAssembly: {dll} ({dll}) -> {error}");
            }

            return null;
        }

        private bool IsScriptFile(string fileext) => fileext == ".cs" || fileext == ".hbs" || fileext == ".dll";
    }
}
