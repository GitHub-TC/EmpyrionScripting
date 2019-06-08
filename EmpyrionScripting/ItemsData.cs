using System.Collections.Generic;
using Eleon.Modding;

namespace EmpyrionScripting
{
    public class ItemsData
    {
        public int Id { get; set; }
        public int Count { get; set; }
        public string Name { get; set; }
        public string Key { get; set; }
        public List<IContainer> Source { get; set; }
        public List<string> SourceNames { get; internal set; }

        public ItemsData AddCount(int count, IContainer container, string customName)
        {
            SourceNames .Add(customName);
            Source      .Add(container);
            Count += count;
            return this;
        }
    }
}