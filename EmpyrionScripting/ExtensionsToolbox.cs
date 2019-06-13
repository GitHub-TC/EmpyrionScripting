using System;
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
    }
}
