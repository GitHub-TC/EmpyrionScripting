using Eleon.Modding;

namespace EmpyrionScripting.Interface
{
    public interface IScriptSaveGameRootData
    {
        string MainScriptPath { get; set; }
        IModApi ModApi { get; set; }
        string ScriptPath { get; set; }
    }
}