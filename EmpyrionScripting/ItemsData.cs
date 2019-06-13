using System.Collections.Generic;
using Eleon.Modding;

namespace EmpyrionScripting
{
    public class ItemsSource
    {
        public EntityData E { get; set; }
        public IContainer Container { get; set; }
        public VectorInt3 Position { get; set; }
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
            if(Source == null) Source = new List<ItemsSource>();
            if(source == null) return this;

            lock (Source)
            {
                Source.Add(source);
                Count += count;
            }
            return this;
        }
    }
}