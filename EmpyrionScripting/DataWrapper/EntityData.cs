using Eleon.Modding;
using UnityEngine;

namespace EmpyrionScripting.DataWrapper
{
    public class EntityData
    {
        private IEntity entity;

        public EntityData()
        {
        }

        public EntityData(IEntity entity)
        {
            this.entity = entity;
        }

        public StructureData S { get => _s == null ? _s = new StructureData(this) : _s; set => _s = value; }
        private StructureData _s;

        public string[] DeviceNames => entity.Structure.GetDeviceTypeNames();

        public int Id => entity.Id;
        public virtual string Name => entity.Name;
        public EntityType EntityType => entity.Type;

        public Vector3 Pos => entity.Position;

        public virtual IEntity GetCurrent() => entity;
    }
}