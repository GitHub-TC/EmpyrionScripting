using Eleon.Modding;
using EmpyrionScripting.Interface;
using System;
using System.Linq;

namespace EmpyrionScripting.DataWrapper
{
    public class StructureTank : IStructureTankWrapper
    {
        private readonly IStructureTank tank;
        private readonly StructureTankType type;

        public StructureTank(IStructureTank tank, StructureTankType type)
        {
            this.tank = tank;
            this.type = type;
        }

        public float Capacity => tank == null ? 0 : tank.Capacity;
        public float Content => tank == null ? 0 : tank.Content;

        public IStructureTank GetCurrent() => tank;

        public int AddItems(int itemId, int count)
        {
            tank?.AddContent(count * EmpyrionScripting.Configuration.Current.StructureTank[type].First(I => I.ItemId == itemId).Amount);
            return 0;
        }

        public bool AllowedItem(int itemId)
        {
            return tank != null && EmpyrionScripting.Configuration.Current.StructureTank[type].Any(I => I.ItemId == itemId);
        }

        public int ItemsNeededForFill(int itemId, int maxLimit)
        {
            return tank == null ? 0 : Math.Max(0, (int)((Capacity * (maxLimit / 100f)) - Content) / EmpyrionScripting.Configuration.Current.StructureTank[type].First(I => I.ItemId == itemId).Amount);
        }
    }
}