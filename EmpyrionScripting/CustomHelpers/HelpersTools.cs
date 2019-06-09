using HandlebarsDotNet;
using System;
using System.Collections.Generic;
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
                        Handlebars.RegisterHelper(
                            ((HandlebarTagAttribute)Attribute.GetCustomAttribute(M, typeof(HandlebarTagAttribute))).Tag,
                            (HandlebarsHelper)Delegate.CreateDelegate(typeof(HandlebarsHelper), M));
                    }
                    catch { }

                    try
                    {
                        Handlebars.RegisterHelper(
                            ((HandlebarTagAttribute)Attribute.GetCustomAttribute(M, typeof(HandlebarTagAttribute))).Tag,
                            (HandlebarsBlockHelper)Delegate.CreateDelegate(typeof(HandlebarsBlockHelper), M));
                    }
                    catch { }
                })
            );
        }

        public static Dictionary<string, string> GetUniqueNames(this StructureData structure, string namesSearch)
        {
            var names = new List<string>();

            namesSearch
                .Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(N => N.Trim())
                .ForEach(N =>
                {
                    if (N == "*") names.AddRange(structure.AllCustomDeviceNames);
                    else if (N.EndsWith("*"))
                    {
                        var startsWith = N.Substring(0, N.Length - 1);
                        names.AddRange(structure.AllCustomDeviceNames.Where(SN => SN.StartsWith(startsWith)));
                    }
                    else names.AddRange(structure.AllCustomDeviceNames.Where(SN => SN == N));
                });

            return names.GroupBy(N => N).ToDictionary(N => N.Key, N => N.Key);
        }

        public static IEnumerable<T> TakeLast<T>(this IEnumerable<T> source, int N)
        {
            return source.Skip(Math.Max(0, source.Count() - N));
        }
    }
}
