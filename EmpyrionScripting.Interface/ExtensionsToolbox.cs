using Eleon.Modding;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace EmpyrionScripting.Interface
{
    public static class ExtensionsToolbox
    {
        public static string NormalizePath(this string path)
        {
            return Path.GetFullPath(new Uri(path).LocalPath)
                       .TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
                       .ToUpperInvariant();
        }

        public static IEnumerable<VectorInt3> Values(this IDevicePosList list)
        {
            for (int i = 0; i < list.Count; i++) yield return list.GetAt(i);
        }

        public static object Get(this object[] args, int index)
        {
            return args.Length > index ? args[index] : null;
        }

        public static T Get<T>(this object[] args, int index) where T : class
        {
            return args.Length > index ? args[index] as T : default;
        }

        public static IEnumerable<T> ForEach<T>(this IEnumerable<T> data, Action<T> action, Func<bool> terminate)
        {
            if (data == null || terminate()) return Enumerable.Empty<T>();

            foreach (var item in data)
            {
                action(item);
                if (terminate()) return Enumerable.Empty<T>();
            }

            return data;
        }
    }
}
