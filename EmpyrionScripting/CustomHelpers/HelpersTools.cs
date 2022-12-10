using EmpyrionScripting.Interface;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;

namespace EmpyrionScripting.CustomHelpers
{
    public static class HelpersTools
    {
        public static IEnumerable<string> GetUniqueNames(this IEnumerable<string> sourceNames, string namesSearch)
        {
            return sourceNames.GetUniqueNames(namesSearch, new[] { ';', ',' });
        }

        public static IEnumerable<string> GetUniqueNames(this IEnumerable<string> sourceNames, string namesSearch, char[] delimitter)
        {
            var names = new List<string>();

            namesSearch
                .Split(delimitter, StringSplitOptions.RemoveEmptyEntries)
                .Select(N => N.Trim())
                .ForEach(N =>
                {
                    if (N == "*") names.AddRange(sourceNames);
                    else if (N.EndsWith("*") && N.StartsWith("*"))
                    {
                        var contains = N.Substring(1, N.Length - 2).ToLower();
                        names.AddRange(sourceNames.Where(SN => SN.ToLower().Contains(contains)));
                        names.AddRange(sourceNames.Where(SN => SN.ToLower().Equals(contains, StringComparison.InvariantCultureIgnoreCase)));
                    }
                    else if (N.EndsWith("*"))
                    {
                        var startsWith = N.Substring(0, N.Length - 1).ToLower();
                        names.AddRange(sourceNames.Where(SN => SN.ToLower().StartsWith(startsWith, StringComparison.InvariantCultureIgnoreCase)));
                        names.AddRange(sourceNames.Where(SN => SN.ToLower().Equals(startsWith, StringComparison.InvariantCultureIgnoreCase)));
                    }
                    else if (N.StartsWith("*"))
                    {
                        var endsWith = N.Substring(1).ToLower();
                        names.AddRange(sourceNames.Where(SN => SN.ToLower().EndsWith(endsWith, StringComparison.InvariantCultureIgnoreCase)));
                        names.AddRange(sourceNames.Where(SN => SN.ToLower().Equals(endsWith, StringComparison.InvariantCultureIgnoreCase)));
                    }
                    else
                    {
                        var equal = N.ToLower();
                        names.AddRange(sourceNames.Where(SN => SN.ToLower().Equals(equal, StringComparison.InvariantCultureIgnoreCase)));
                    }
                });

            return names
                .OrderBy(N => N)
                .Distinct()
                .ToArray();
        }

        public static IEnumerable<T> TakeLast<T>(this IEnumerable<T> source, int N)
        {
            return source.Skip(Math.Max(0, source.Count() - N));
        }

        public class FileContent {
            public string[] Lines { get; set; }
            public string Text { get; set; }
            public FileSystemWatcher Watcher { get; set; }
        }

        static readonly ConcurrentDictionary<string, FileContent> FileContentCache = new ConcurrentDictionary<string, FileContent>();

        public static FileContent GetFileContent(string filename)
        {
            try
            {
                if (FileContentCache.TryGetValue(filename, out var filecontent)) return filecontent;
                if (!File.Exists(filename)) return null;

                filecontent = new FileContent()
                {
                    Lines   = File.ReadAllLines(filename),
                    Text    = File.ReadAllText(filename),
                    Watcher = new FileSystemWatcher(Path.GetDirectoryName(filename), Path.GetFileName(filename))
                };
                filecontent.Watcher.Renamed += (S, A) => { filecontent.Watcher.EnableRaisingEvents = false; FileContentCache.TryRemove(filename, out var _); };
                filecontent.Watcher.Changed += (S, A) => { filecontent.Watcher.EnableRaisingEvents = false; FileContentCache.TryRemove(filename, out var _); };
                filecontent.Watcher.Deleted += (S, A) => { filecontent.Watcher.EnableRaisingEvents = false; FileContentCache.TryRemove(filename, out var _); };
                filecontent.Watcher.Created += (S, A) => { filecontent.Watcher.EnableRaisingEvents = false; FileContentCache.TryRemove(filename, out var _); };
                filecontent.Watcher.EnableRaisingEvents = true;

                FileContentCache.AddOrUpdate(filename, filecontent, (S, O) => filecontent);
                return filecontent;
            }
            catch (Exception error)
            {
                EmpyrionScripting.Log($"Filename: {filename} => {error}", EmpyrionNetAPIDefinitions.LogLevel.Message);
                return null;
            }
        }

    }
}
