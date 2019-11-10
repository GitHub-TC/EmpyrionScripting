using Eleon.Modding;
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

        public EntityData(bool isPublic)
        {
            _s = isPublic ? null : new Lazy<IStructureData>(() => new StructureData(this));
        }
        public EntityData(IPlayfield playfield, IEntity entity) : this(playfield, entity, false) { }

        public EntityData(IPlayfield playfield, IEntity entity, bool isPublic): this(isPublic)
        {
            this.playfield = new WeakReference<IPlayfield>(playfield);
            this.entity    = new WeakReference<IEntity>(entity);
        }

        public IStructureData S => _s?.Value;
        private readonly Lazy<IStructureData> _s;

        public string[] DeviceNames => Enum.GetNames(typeof(DeviceTypeName));

        public int Id => GetCurrent().Id;
        public virtual string Name => GetCurrent().Name;
        public EntityType EntityType => GetCurrent().Type;

        public Vector3 Pos => GetCurrent().Position;
        public FactionData Faction => GetCurrent().Faction;

        public virtual IEntity GetCurrent() => entity.TryGetTarget(out var e) ? e : null;
        public virtual IPlayfield GetCurrentPlayfield() => playfield.TryGetTarget(out var p) ? p : null;
    }
}