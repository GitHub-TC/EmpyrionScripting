using System.Collections.Generic;

namespace EmpyrionScripting.Interface
{
    public interface IItemsData : IItemBase
    {
        int Count { get; set; }
        string Key { get; set; }
        string Name { get; set; }
        List<IItemsSource> Source { get; set; }
    }
}