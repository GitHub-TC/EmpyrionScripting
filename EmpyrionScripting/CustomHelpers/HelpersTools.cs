using HandlebarsDotNet;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace EmpyrionScripting.CustomHelpers
{
    public class HandlebarHelpersAttribute : Attribute { }

    public class HandlebarTagAttribute : Attribute
    {
        public HandlebarTagAttribute(string tag)
        {
            Tag = tag;
        }

        public string Tag { get; }
    }

    public static class HelpersTools
    {
        public static void ScanHandlebarHelpers()
        {
            var helperTypes = typeof(HelpersTools).Assembly.GetTypes()
                .Where(T => T.GetCustomAttributes(typeof(HandlebarHelpersAttribute), true).Length > 0);

            helperTypes.ForEach(T =>
                T.GetMethods(BindingFlags.DeclaredOnly | BindingFlags.Static | BindingFlags.Public)
                .ForEach(M =>
                {
                    try
                    {
                        if (Attribute.GetCustomAttribute(M, typeof(HandlebarTagAttribute)) is HandlebarTagAttribute A)
                            Handlebars.RegisterHelper(A.Tag, (HandlebarsHelper)Delegate.CreateDelegate(typeof(HandlebarsHelper), M));
                    }
                    catch { }

                    try
                    {
                        if (Attribute.GetCustomAttribute(M, typeof(HandlebarTagAttribute)) is HandlebarTagAttribute A)
                            Handlebars.RegisterHelper(A.Tag, (HandlebarsBlockHelper)Delegate.CreateDelegate(typeof(HandlebarsBlockHelper), M));
                    }
                    catch { }
                })
            );
        }

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
                    else if (N.EndsWith("*"))
                    {
                        var startsWith = N.Substring(0, N.Length - 1);
                        names.AddRange(sourceNames.Where(SN => SN.StartsWith(startsWith)));
                    }
                    else names.AddRange(sourceNames.Where(SN => SN == N));
                });

            return names
                .OrderBy(N => N)
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
