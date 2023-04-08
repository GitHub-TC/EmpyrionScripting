using Eleon.Modding;
using EmpyrionScripting.Interface;
using System;
using System.Linq;
using UnityEngine;

namespace EmpyrionScripting.DataWrapper
{
    public class EntityData : IEntityData
    {
        private readonly WeakReference<IEntity> entity;
        private readonly WeakReference<IPlayfield> playfield;
        private EntityType LastKnownType;
        public string LastKnownName;
        private bool WeakReferenceFailed;

        public EntityData(bool isPublic)
        {
            _s = isPublic ? null : new Lazy<IStructureData>(() => new StructureData(this));
        }
        public EntityData(IPlayfield playfield, IEntity entity) : this(playfield, entity, false) { }

        public EntityData(IPlayfield playfield, IEntity entity, bool isPublic) : this(isPublic)
        {
            this.playfield = new WeakReference<IPlayfield>(playfield);
            this.entity = new WeakReference<IEntity>(entity);
        }

        public IStructureData S => _s?.Value;
        private readonly Lazy<IStructureData> _s;

        public string[] DeviceNames => Enum.GetNames(typeof(DeviceTypeName));

        public int Id => GetCurrent()?.Id ?? 0;
        public virtual string Name => GetCurrent()?.Name;
        public EntityType EntityType
        {
            get {
                try
                {
                    if (WeakReferenceFailed || GetCurrent() == null) return EntityType.Unknown;

                    LastKnownType = GetCurrent().Type;
                    if (string.IsNullOrEmpty(LastKnownName)) LastKnownName = Name;

                    return LastKnownType;
                }
                catch (Exception error)
                {
                    EmpyrionScripting.Log($"WeakReference<IEntity> {LastKnownType}/{LastKnownName} no longer present in the game (perhaps the playfield will be unloaded) {error}", EmpyrionNetAPIDefinitions.LogLevel.Message);
                    WeakReferenceFailed = true;
                    return EntityType.Unknown;
                }
            }
        }
        public bool IsLocal => GetCurrent()?.IsLocal ?? false;
        public bool IsPoi => GetCurrent()?.IsPoi ?? false;
        public bool IsProxy => GetCurrent()?.IsProxy ?? true;

        public Vector3 Pos => GetCurrent()?.Position ?? Vector3.zero;
        public Vector3 Rot => GetCurrent()?.Rotation.eulerAngles ?? Vector3.zero;
        public Vector3 Forward => GetCurrent()?.Forward ?? Vector3.zero;
        public float Distance { get; set; }
        public FactionData Faction => GetCurrent()?.Faction ?? new FactionData();

        public void MoveForward(float speed)
        {
            if (pfScriptData.PersistendData.TryGetValue($"MoveForward{Id}", out var currentSpeed) && (float)currentSpeed == speed) return;
            pfScriptData.PersistendData.AddOrUpdate($"MoveForward{Id}", speed, (k, d) => speed);
            pfScriptData.PersistendData.TryRemove($"MoveStop{Id}", out _);

            GetCurrent()?.MoveForward(speed);
        }

        public void MoveTo(Vector3 direction)
        {
            if (pfScriptData.PersistendData.TryGetValue($"MoveTo{Id}", out var currentDirection) && (Vector3)currentDirection == direction) return;
            pfScriptData.PersistendData.AddOrUpdate($"MoveTo{Id}", direction, (k,d) => direction);
            pfScriptData.PersistendData.TryRemove($"MoveStop{Id}", out _);

            GetCurrent()?.Move(direction);
        }

        public void MoveStop()
        {
            if (pfScriptData.PersistendData.TryGetValue($"MoveStop{Id}", out _)) return;
            pfScriptData.PersistendData.TryAdd($"MoveStop{Id}", Id);
            pfScriptData.PersistendData.TryRemove($"MoveForward{Id}", out _);
            pfScriptData.PersistendData.TryRemove($"MoveTo{Id}", out _);

            var e = GetCurrent();
            if(e == null) return;

            e.MoveForward(0);
            e.Move(Vector3.zero);
            e.MoveStop();

        }

        public int BelongsTo => GetCurrent()?.BelongsTo ?? 0;
        public int DockedTo { get { try { return GetCurrent().DockedTo; } catch { return 0; } } }

        public bool IsElevated { 
            get {
                if (_IsElevated.HasValue) return _IsElevated.Value;

                var testGroup = Faction.Group == FactionGroup.Faction || Faction.Group == FactionGroup.Player ? Faction.Id.ToString() : Faction.Group.ToString();
                _IsElevated = EmpyrionScripting.Configuration.Current.ElevatedGroups.Any(f => f == testGroup);

                return _IsElevated.Value;
            }
        }
        bool? _IsElevated;

        public virtual IEntity GetCurrent() => entity.TryGetTarget(out var e) ? e : null;
        public virtual IPlayfield GetCurrentPlayfield() => playfield.TryGetTarget(out var p) ? p : null;

        public IScriptInfo[] ScriptInfos
        {
            get {
                if(pfScriptData == null) return Array.Empty<IScriptInfo>();

                var entityId = GetCurrent()?.Id;
                if (entityId == 0) return Array.Empty<IScriptInfo>();

                return pfScriptData.ScriptExecQueue.ScriptRunInfo.Values
                    .Where(i => i.EntityId == entityId && (IsElevated || !i.IsElevatedScript))
                    .OrderBy(i => i.ScriptId)
                    .ToArray();
            }
        }

        PlayfieldScriptData pfScriptData
        {
            get
            {
                if(_pfScriptData != null) return _pfScriptData;

                var pfName = GetCurrentPlayfield()?.Name;
                return string.IsNullOrEmpty(pfName) || EmpyrionScripting.EmpyrionScriptingInstance?.PlayfieldData.TryGetValue(pfName, out _pfScriptData) == false
                    ? null
                    : _pfScriptData;
            }
        }
        PlayfieldScriptData _pfScriptData;
    }
}