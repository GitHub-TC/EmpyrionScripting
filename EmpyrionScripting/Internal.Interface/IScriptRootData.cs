namespace EmpyrionScripting.Internal.Interface
{

    public interface IScriptRootData : IScriptRootModData
    {
        IPlayfieldScriptData GetPlayfieldScriptData();
    }
}