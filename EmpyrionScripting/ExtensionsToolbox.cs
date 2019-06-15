using Eleon.Modding;
using System;
using System.Collections.Generic;
using System.IO;

namespace EmpyrionScripting
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
    }
}
