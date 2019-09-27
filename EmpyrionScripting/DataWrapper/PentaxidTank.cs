using Eleon.Modding;
using System;
using System.Linq;

namespace EmpyrionScripting.DataWrapper
{
    public class PentaxidTank : IStructureTankWrapper
    {
        const int PentaxidItemId = 2294;
        private readonly IContainer tank;

        public PentaxidTank(IContainer tank)
        {
            this.tank = tank;
        }

        public float Capacity => tank == null ? 0 : tank.VolumeCapacity;

        public float Content => tank == null ? 0 : tank.GetTotalItems(PentaxidItemId) * EmpyrionScripting.Configuration.Current.StructureTank[StructureTankType.Pentaxid].First(I => I.ItemId == PentaxidItemId).Amount;

        public int ItemsNeededForFill(int itemId, int maxLimit)
        {
            return Math.Max(0, (int)((Capacity * (maxLimit / 100f)) - Content) / EmpyrionScripting.Configuration.Current.StructureTank[StructureTankType.Pentaxid].First(I => I.ItemId == itemId).Amount);
        }
        public bool AllowedItem(int itemId)
        {
            return tank != null && EmpyrionScripting.Configuration.Current.StructureTank[StructureTankType.Pentaxid].Any(I => I.ItemId == itemId);
        }
        public int AddItems(int itemId, int count)
        {
            return tank.AddItems(itemId, count);
        }
    }
}
