using System.Collections.Generic;
using System.Linq;
using Eleon.Modding;
using EmpyrionScripting.Interface;

namespace EmpyrionScripting.DataWrapper
{
    public class ItemBase : IItemBase
    {
        public int Id { get; set; }
        public int ItemId => Id % ItemTokenAccess.TokenIdSeperator;
        public int TokenId => Id / ItemTokenAccess.TokenIdSeperator;
        public bool IsToken => Id > ItemTokenAccess.TokenIdSeperator;
    }

    public class ItemsSource : ItemBase, IItemsSource
    {
        public IEntityData E { get; set; }
        public IContainer Container { get; set; }
        public VectorInt3 Position { get; set; }
        public string CustomName { get; set; }
        public int Count { get; set; }
        public int Ammo { get; set; }
        public int Decay { get; set; }
    }

    public class ItemsData : ItemBase, IItemsData
    {
        public int Count { get; set; }
        public string Name { get; set; }
        public string Key { get; set; }
        public List<IItemsSource> Source { get; set; }
        public int Ammo { get; set; }
        public int Decay { get; set; }

        public ItemsData AddCount(int count, IItemsSource source)
        {
            if (Source == null) Source = new List<IItemsSource>();
            if (source == null) return this;

            lock (Source)
            {
                Source.Add(source);
                Count += count;
            }
            return this;
        }
    }

    public static class ItemStackExtensions{
        public static List<ItemStack> UniqueSlots(this IEnumerable<ItemStack> items)
        {
            byte index = 0;
            return items.Select(i => new ItemStack(i.id, i.count) { slotIdx = index++, decay = i.decay, ammo = i.ammo }).ToList();
        }
    }

}