using System.Collections.Generic;
using Eleon.Modding;

namespace EmpyrionScripting
{
    public class ItemsSource
    {
        public IContainer Container { get; set; }
        public string CustomName { get; set; }
        public int Id { get; set; }
        public int Count { get; set; }
    }

    public class ItemsData
    {
        public int Id { get; set; }
        public int Count { get; set; }
        public string Name { get; set; }
        public string Key { get; set; }
        public List<ItemsSource> Source { get; set; }

        public ItemsData AddCount(int count, ItemsSource source)
        {
            Source.Add(source);
            Count += count;
            return this;
        }
    }
}