using Eleon.Modding;
using EmpyrionScripting.Interface;
using System.Collections.Concurrent;
using System.Reflection;

namespace EmpyrionScripting.DataWrapper
{
    public class ScriptSaveGameRootData : ScriptRootData, IScriptSaveGameRootData
    {
        public ScriptSaveGameRootData() : base()
        {
        }
        public ScriptSaveGameRootData(ScriptSaveGameRootData data) : base(data)
        {
            MainMethod     = data.MainMethod;
            ModApi         = data.ModApi;
            ScriptPath     = data.ScriptPath;
            MainScriptPath = data.MainScriptPath;
        }
        public ScriptSaveGameRootData(
            PlayfieldScriptData playfieldScriptData,
            IEntity[] allEntities,
            IEntity[] currentEntities,
            IPlayfield playfield,
            IEntity entity,
            ConcurrentDictionary<string, object> persistendData,
            EventStore eventStore) : base(playfieldScriptData, allEntities, currentEntities, playfield, entity, persistendData, eventStore)
        {
        }

        public MethodInfo MainMethod { get; set; }
        public string ScriptPath { get; set; }
        public string MainScriptPath { get; set; }
        public IModApi ModApi { get; set; }

        public override bool DeviceLockAllowed => true;

    }
}