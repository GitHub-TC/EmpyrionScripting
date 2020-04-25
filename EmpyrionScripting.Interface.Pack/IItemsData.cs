using System.Collections.Generic;

namespace EmpyrionScripting.Interface
{
    public interface IItemsData
    {
        int Count { get; set; }
        int Id { get; set; }
        string Key { get; set; }
        string Name { get; set; }
        List<IItemsSource> Source { get; set; }
    }
}