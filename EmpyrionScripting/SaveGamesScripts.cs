using Eleon.Modding;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmpyrionScripting
{
    public class SaveGamesScripts
    {
        public const string ScriptExtension = ".hbs";

        FileSystemWatcher SaveGameScriptsWatcher { get; set; }
        public ConcurrentDictionary<string, string> SaveGameScripts { get; private set; }
        public string MainScriptPath { get; set; }
        public string SaveGameModPath { get; set; }
        public IModApi ModApi { get; }

        public SaveGamesScripts(IModApi modApi)
        {
            ModApi = modApi;
            EmpyrionScripting.StopApplicationEvent += (S, E) => SaveGameScriptsWatcher.EnableRaisingEvents = false;
        }

        public void ReadSaveGamesScripts()
        {
            MainScriptPath = Path.Combine(SaveGameModPath, "Scripts");
            Directory.CreateDirectory(MainScriptPath);

            SaveGameScriptsWatcher = new FileSystemWatcher(MainScriptPath, "*" + ScriptExtension)
            {
                IncludeSubdirectories = true,
            };
            SaveGameScriptsWatcher.Changed += (S, E) => {
                ModApi?.Log($"SaveGameScript: changed script: {E.FullPath.NormalizePath()}");
                SaveGameScripts.AddOrUpdate(E.FullPath.NormalizePath(), F => File.ReadAllText(F), (F, C) => File.ReadAllText(F));
            };
            SaveGameScriptsWatcher.Created += (S, E) =>
            {
                ModApi?.Log($"SaveGameScript: created script: {E.FullPath.NormalizePath()}");
                SaveGameScripts.AddOrUpdate(E.FullPath.NormalizePath(), F => File.ReadAllText(F), (F, C) => File.ReadAllText(F));
            };
            SaveGameScriptsWatcher.Renamed += (S, E) =>
            {
                ModApi?.Log($"SaveGameScript: renamed script: {E.OldFullPath.NormalizePath()} -> {E.FullPath.NormalizePath()}");
                SaveGameScripts.TryRemove(E.OldFullPath.NormalizePath(), out _);
                SaveGameScripts.AddOrUpdate(E.FullPath.NormalizePath(), F => File.ReadAllText(F), (F, C) => File.ReadAllText(F));
            };
            SaveGameScriptsWatcher.Deleted += (S, E) =>
            {
                ModApi?.Log($"SaveGameScript: deleted script: {E.FullPath.NormalizePath()}");
                SaveGameScripts.TryRemove(E.FullPath.NormalizePath(), out _);
            };

            SaveGameScripts = new ConcurrentDictionary<string, string>(
                                    Directory.GetFiles(MainScriptPath, "*" + ScriptExtension, SearchOption.AllDirectories)
                                    .ToDictionary(F => F.NormalizePath(), F => File.ReadAllText(F)));

            SaveGameScriptsWatcher.EnableRaisingEvents = true;

            SaveGameScripts.Keys.ForEach(F => ModApi?.Log($"SaveGameScript: found script: {F}"));
        }

    }
}
