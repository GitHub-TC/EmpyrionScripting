using Eleon.Modding;
using System;
using UnityEngine;

namespace EmpyrionScripting.DataWrapper
{
    public class EntityData : IEntityData
    {
        private readonly WeakReference<IEntity> entity;

        public EntityData(bool isPublic)
        {
            _s = isPublic ? null : new Lazy<IStructureData>(() => new StructureData(this));
        }
        public EntityData(IEntity entity) : this(entity, false) { }

        public EntityData(IEntity entity, bool isPublic): this(isPublic)
        {
            this.entity = new WeakReference<IEntity>(entity);
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
    }
}