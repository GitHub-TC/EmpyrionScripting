using System.Reflection;

namespace EmpyrionScripting.CsCompiler
{
    public interface ILoadedAssemblyInfo
    {
        string FullAssemblyDllName { get; set; }
        Assembly LoadedAssembly { get; set; }
    }

    public class LoadedAssemblyInfo : ILoadedAssemblyInfo
    {
        public string FullAssemblyDllName { get; set; }
        public Assembly LoadedAssembly { get; set; }
    }

}