using Eleon.Modding;
using EmpyrionScripting.CsHelper;
using EmpyrionScripting.Interface;
using EmpyrionScripting.Internal.Interface;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using UnityEngine;

namespace EmpyrionScripting.DataWrapper
{

    public class ScriptRootData : IScriptRootData
    {
        private readonly PlayfieldScriptData _PlayfieldScriptData;

        private readonly ConcurrentDictionary<string, object> _PersistendData;
        private readonly IEntity[] currentEntities;
        private readonly IEntity[] allEntities;
        private readonly IPlayfield playfield;
        private readonly IEntity entity;

        public IEventStore SignalEventStore { get; }

        public ScriptRootData()
        {
            Console = new ConsoleMock(this);
            CsRoot  = new CsScriptFunctions(this);

            _e = new Lazy<IEntityData>(() => new EntityData(playfield, entity));
        }

        public ScriptRootData(
            PlayfieldScriptData playfieldScriptData,
            IEntity[] allEntities,
            IEntity[] currentEntities, 
            IPlayfield playfield, 
            IEntity entity, 
            ConcurrentDictionary<string, object> persistendData, 
            EventStore eventStore) : this()
        {
            _PlayfieldScriptData        = playfieldScriptData;
            _PersistendData             = persistendData;
            this.currentEntities        = currentEntities;
            this.allEntities            = allEntities;
            this.playfield              = playfield;
            this.entity                 = entity;
            SignalEventStore            = eventStore;
            var testGroup = entity?.Faction.Group == FactionGroup.Faction || entity?.Faction.Group == FactionGroup.Player ? entity?.Faction.Id.ToString() : entity?.Faction.Group.ToString();
            IsElevatedScript = this is ScriptSaveGameRootData || EmpyrionScripting.Configuration.Current.ElevatedGroups.Any(f => f == testGroup);

            try  { CultureInfo = playfieldScriptData?.EntityCultureInfo?.GetOrAdd(entity.Id, e => GetCultureInfoForEntity(entity)); }
            catch{ CultureInfo = new EntityCultureInfo();                                                                           }
        }

        public ScriptRootData(ScriptRootData data) : this(data._PlayfieldScriptData, data.allEntities, data.currentEntities, data.playfield, data.entity, data._PersistendData, (EventStore)data.SignalEventStore)
        {
            _p = data._p;
            _e = data._e;
            DisplayType = data.DisplayType;
        }

        EntityCultureInfo GetCultureInfoForEntity(IEntity entity)
        {
            var lcd = entity.Structure.GetDevice<ILcd>("CultureInfo");
            
            if (lcd != null)
            {
                try
                {
                    var text = lcd.GetText();
                    if (string.IsNullOrEmpty(text))
                    {
                        var newCultureInfo = new EntityCultureInfo();
                        lcd.SetText(Newtonsoft.Json.JsonConvert.SerializeObject(newCultureInfo, Newtonsoft.Json.Formatting.Indented));
                        return newCultureInfo;
                    }

                    var ci = Newtonsoft.Json.JsonConvert.DeserializeObject<EntityCultureInfo>(text);
                    ci.CultureInfo = EntityCultureInfo.CreateFormatCulture(System.Globalization.CultureInfo.CreateSpecificCulture(ci.LanguageTag));

                    return ci;
                }
                catch (Exception error)
                {
                    var debugLcd = entity.Structure.GetDevice<ILcd>("CultureInfoDebug");
                    if (debugLcd != null) debugLcd.SetText(error.Message);
                }
            }

            return new EntityCultureInfo();
        }

        public string Version { get; } = EmpyrionScripting.Version;
        public string ScriptingModInfoData { get; } = EmpyrionScripting.ScriptingModInfoData;
        public ConcurrentDictionary<string, string> ScriptingModScriptsInfoData { get; } = EmpyrionScripting.ScriptingModScriptsInfoData;

        public bool IsElevatedScript { get; }
        public ICsScriptFunctions CsRoot { get; }

        public IConsoleMock Console { get; }
        public DateTime DateTimeNow => DateTime.Now;


        public IPlayfieldScriptData GetPlayfieldScriptData() => _PlayfieldScriptData;
        public ConcurrentDictionary<string, object> GetPersistendData() => _PersistendData;
        public ConcurrentDictionary<string, object> CacheData =>
            (ConcurrentDictionary<string, object>)
            (_PersistendData.TryGetValue(E?.Id.ToString() ?? string.Empty, out var cache) 
                ? cache
                : _PersistendData.GetOrAdd(E?.Id.ToString() ?? string.Empty, new ConcurrentDictionary<string, object>())
            );

        public IEnumerable<IEntity> GetAllEntities() => IsElevatedScript ? allEntities : Enumerable.Empty<IEntity>();
        public IEnumerable<IEntity> GetEntities() => currentEntities
                .Where(SafeIsNoProxyCheck)
                .Where(e => IsElevatedScript || e.Faction.Id == E.GetCurrent().Faction.Id)
                .Where(e => IsElevatedScript || Vector3.Distance(e.Position, E.Pos) <= EmpyrionScripting.Configuration.Current.EntityAccessMaxDistance);

        public IPlayfield GetCurrentPlayfield() => playfield;

        public Dictionary<string, string> Ids => EmpyrionScripting.Configuration.Current.Ids;

        public string OreIds => "2248,2249,2250,2251,2252,2253,2254,2269,2270,2284,2293,2297";
        public string IngotIds => "2271,2272,2273,2274,2275,2276,2277,2278,2279,2280,2281,2285,2294,2298";

        public IPlayfieldData P { get => _p == null ? _p = new PlayfieldData(playfield) : _p; set => _p = value; }
        private IPlayfieldData _p;

        public IEntityData E => _e.Value; 
        private readonly Lazy<IEntityData> _e;

        public List<string> LcdTargets { get; set; } = new List<string>();
        public bool FontSizeChanged { get; set; }
        public int FontSize { get => _FontSize; set { _FontSize = value; FontSizeChanged = true; } }
        int _FontSize;
        public bool ColorChanged { get; set; }
        public Color Color { get => _Color; set { _Color = value; ColorChanged = true; } }
        Color _Color;
        public bool BackgroundColorChanged { get; set; }
        public Color BackgroundColor { get => _BackgroundColor; set { _BackgroundColor = value; BackgroundColorChanged = true; } }
        Color _BackgroundColor;
        public ConcurrentDictionary<string, object> Data { get; set; } = new ConcurrentDictionary<string, object>();
        public ScriptLanguage ScriptLanguage { get; set; }
        public string Script { get; set; }
        public TextWriter ScriptOutput { get; set; }
        public IDisplayOutputConfiguration DisplayType { get; set; }
        public string Error { get; set; }
        public string ScriptId { get; set; }

        public int CycleCounter => ((PlayfieldScriptData)GetPlayfieldScriptData()).CycleCounter(ScriptId);
        public virtual bool DeviceLockAllowed => ((PlayfieldScriptData)GetPlayfieldScriptData()).DeviceLockAllowed(ScriptId);

        public IConfigEcfAccess ConfigEcfAccess => EmpyrionScripting.ConfigEcfAccess;

        public bool ScriptWithinMainThread { get; set; }
        public bool ScriptNeedsMainThread { get; set; }
        public int ScriptPriority { get; set; }
        public IScriptInfo ScriptDiagnosticInfo { get; set; }
        public bool Running { get; set; }

        public Func<bool> ScriptLoopTimeLimitReached { get; set; } = () => false;
        public bool TimeLimitReached
        {
            get {
                if (!Running) return false;
                
                if(ScriptLoopTimeLimitReached())
                {
                    Running = false;
                    if (ScriptDiagnosticInfo is ScriptInfo scriptInfo) Interlocked.Increment(ref scriptInfo._TimeLimitReached);
                }

                return !Running;
            }
        }

        public IEntityCultureInfo CultureInfo { get; set; } = new EntityCultureInfo();

        private static bool SafeIsNoProxyCheck(IEntity entity)
        {
            try { return entity != null && entity.Type != EntityType.Proxy && entity.Type != EntityType.Unknown; }
            catch { return false; }
        }

    }
}
