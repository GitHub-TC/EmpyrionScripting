using Eleon.Modding;
using UnityEngine;

namespace EmpyrionScripting
{
    public class EntityData
    {
        private IEntity entity;

        public EntityData(IEntity entity)
        {
            this.entity = entity;
        }

        /// <summary>
        /// not accessible from the LCD macros
        /// </summary>
        /// <returns></returns>
        public IEntity GetCurrent() { return entity; }

        public int Id => entity.Id;
        public string Name => entity.Name;

        public Vector3 Pos => entity.Position;
    }
}