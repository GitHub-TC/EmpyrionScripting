using Eleon.Modding;

namespace EmpyrionScripting
{
    public class ScriptSaveGameRootData : ScriptRootData
    {
        public ScriptSaveGameRootData(ScriptSaveGameRootData data) : base(data)
        {
            ScriptPath     = data.ScriptPath;
            MainScriptPath = data.MainScriptPath;
        }

        public ScriptSaveGameRootData(IPlayfield playfield, IEntity entity) : base(playfield, entity)
        {
        }

        public string ScriptPath { get; set; }
        public string MainScriptPath { get; set; }
    }
}