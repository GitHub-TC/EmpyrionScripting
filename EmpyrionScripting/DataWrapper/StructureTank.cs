using Eleon.Modding;
using EmpyrionScripting.Interface;
using System;
using System.Collections.Generic;
using System.Linq;

namespace EmpyrionScripting.DataWrapper
{
    public class StructureTank : IStructureTankWrapper
    {
        private readonly IStructureTank tank;
        private readonly StructureTankType type;

        public Dictionary<int, int> AllowedItems { get; }

        public StructureTank(IStructureTank tank, StructureTankType type)
        {
            this.tank    = tank;
            this.type    = type;
            AllowedItems = EmpyrionScripting.Configuration.Current.StructureTank[type].ToDictionary(I => I.ItemId, I => I.Amount);
        }

        public float Capacity => tank == null ? 0 : tank.Capacity;
        public float Content => tank == null ? 0 : tank.Content;

        public IStructureTank GetCurrent() => tank;

        public int AddItems(int itemId, int count)
        {
            if(AllowedItem(itemId)) tank?.AddContent(count * AllowedItems[itemId]);
            return 0;
        }

        public bool AllowedItem(int itemId) => AllowedItems.ContainsKey(itemId);

        public int ItemsNeededForFill(int itemId, int maxLimit)
        {
            return tank == null || !AllowedItem(itemId) ? 0 : Math.Max(0, (int)Math.Floor((Capacity * (maxLimit / 100f)) - Content) / AllowedItems[itemId]);
        }
    }
}