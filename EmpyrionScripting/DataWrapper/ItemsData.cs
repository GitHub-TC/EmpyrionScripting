﻿using System.Collections.Generic;
using Eleon.Modding;
using EmpyrionScripting.Interface;

namespace EmpyrionScripting.DataWrapper
{
    public class ItemsSource : IItemsSource
    {
        public IEntityData E { get; set; }
        public IContainer Container { get; set; }
        public VectorInt3 Position { get; set; }
        public string CustomName { get; set; }
        public int Id { get; set; }
        public int Count { get; set; }
    }

    public class ItemsData : IItemsData
    {
        public int Id { get; set; }
        public int Count { get; set; }
        public string Name { get; set; }
        public string Key { get; set; }
        public List<IItemsSource> Source { get; set; }

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
}