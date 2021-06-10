using Eleon.Modding;
using EmpyrionScripting.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace EmpyrionScripting.DataWrapper
{
    public class EntityData : IEntityData
    {
        private readonly WeakReference<IEntity> entity;
        private readonly WeakReference<IPlayfield> playfield;
        private EntityType LastKnownType;

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

        public int Id => GetCurrent().Id;
        public virtual string Name => GetCurrent().Name;
        public EntityType EntityType
        {
            get {
                try
                {
                    return LastKnownType = GetCurrent() == null ? EntityType.Unknown : GetCurrent().Type;
                }
                catch (Exception error)
                {
                    EmpyrionScripting.Log($"WeakReference<IEntity> LastKnownType:{LastKnownType} but nothing in game? {error}", EmpyrionNetAPIDefinitions.LogLevel.Message);
                    return EntityType.Unknown;
                }
            }
        }
        public bool IsLocal => GetCurrent().IsLocal;
        public bool IsPoi => GetCurrent().IsPoi;
        public bool IsProxy => GetCurrent().IsProxy;

        public Vector3 Pos => GetCurrent().Position;
        public Vector3 Forward => GetCurrent().Forward;
        public float Distance { get; set; }
        public FactionData Faction => GetCurrent().Faction;

        public int BelongsTo => GetCurrent().BelongsTo;
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
                PlayfieldScriptData pfScriptData = null;
                var pfName = GetCurrentPlayfield()?.Name;
                if (string.IsNullOrEmpty(pfName) || EmpyrionScripting.EmpyrionScriptingInstance?.PlayfieldData.TryGetValue(pfName, out pfScriptData) == false) return Array.Empty<IScriptInfo>();

                var entityId = GetCurrent()?.Id;
                if (entityId == 0) return Array.Empty<IScriptInfo>();

                return pfScriptData.ScriptExecQueue.ScriptRunInfo.Values
                    .Where(i => i.EntityId == entityId && (IsElevated || !i.IsElevatedScript))
                    .OrderBy(i => i.ScriptId)
                    .ToArray();
            }
        }
    }
}