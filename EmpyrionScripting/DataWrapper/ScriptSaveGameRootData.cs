using Eleon.Modding;
using System.Collections.Concurrent;

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
        public ScriptSaveGameRootData(IEntity[] currentEntities, IPlayfield playfield, IEntity entity, ConcurrentDictionary<string, object> persistendData) : base(currentEntities, playfield, entity, true, persistendData)
        {
        }

        public string ScriptPath { get; set; }
        public string MainScriptPath { get; set; }
        public IModApi ModApi { get; set; }
    }
}