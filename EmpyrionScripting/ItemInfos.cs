using System;
using System.Collections.Generic;
using System.Linq;

namespace EmpyrionScripting
{
    public class ItemInfo
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Key { get; set; }
    }

    public class ItemInfos
    {
        public Localization Localization { get; set; }
        public Dictionary<int, ItemInfo> ItemInfo { get; set; } = new Dictionary<int, ItemInfo>();

        public ItemInfos(ConfigEcfAccess configAccess, Localization localization)
        {
            try
            {
                Localization = localization;
                ItemInfo = GetAllItems(configAccess).ToDictionary(I => I.Id, I => I);
            }
            catch (Exception error)
            {
                EmpyrionScripting.Log($"ReadAllItemData:{error}", EmpyrionNetAPIDefinitions.LogLevel.Error);
            }
        }

        public IEnumerable<ItemInfo> GetAllItems(ConfigEcfAccess configAccess) =>
            configAccess.ConfigBlockById
                .Select(I => 
                    new ItemInfo()
                      {
                          Id   = I.Key,
                          Key  = I.Value.Attr.FirstOrDefault(A => A.Name == "Id")?.AddOns?.FirstOrDefault(A => A.Key == "Name").Value?.ToString(),
                      }
                )
                .Select(I =>
                 {
                     I.Name = Localization.GetName(I.Key, "English");
                     return I;
                 })
                .ToArray();

    }
}
