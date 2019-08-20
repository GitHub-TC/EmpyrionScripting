using Eleon.Modding;
using System;
using UnityEngine;

namespace EmpyrionScripting.DataWrapper
{
    public class EntityData : IEntityData
    {
        private readonly IEntity entity;

        public EntityData()
        {
            _s = new Lazy<IStructureData>(() => new StructureData(this));
        }

        public EntityData(IEntity entity): this()
        {
            this.entity = entity;
        }

        public IStructureData S => _s.Value;
        private readonly Lazy<IStructureData> _s;

        public string[] DeviceNames => entity.Structure.GetDeviceTypeNames();

        public int Id => entity.Id;
        public virtual string Name => entity.Name;
        public EntityType EntityType => entity.Type;

        public Vector3 Pos => entity.Position;
        public FactionData Faction => entity.Faction;

        public virtual IEntity GetCurrent() => entity;
    }
}