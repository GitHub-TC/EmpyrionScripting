using Eleon.Modding;

namespace EmpyrionScripting.DataWrapper
{
    public class ScriptSaveGameRootData : ScriptRootData
    {
        public ScriptSaveGameRootData() :base()
        {
        }
        public ScriptSaveGameRootData(ScriptSaveGameRootData data) : base(data)
        {
            ScriptPath     = data.ScriptPath;
            MainScriptPath = data.MainScriptPath;
        }
        public ScriptSaveGameRootData(IEntity[] currentEntities, IPlayfield playfield, IEntity entity) : base(currentEntities, playfield, entity)
        {
        }

        public string ScriptPath { get; set; }
        public string MainScriptPath { get; set; }
        public IModApi ModApi { get; set; }
    }
}