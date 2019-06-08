using System;
using System.Collections.Generic;
using System.IO;
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
        private const string IdDef = "Id:";
        private const string NameDef = "Name:";

        public Localization Localization { get; set; }
        public Dictionary<int, ItemInfo> ItemInfo { get; set; } = new Dictionary<int, ItemInfo>();


        public ItemInfos(string contentPath, Localization localization)
        {
            try
            {
                Localization = localization;
                ItemInfo = GetAllItems(contentPath).ToDictionary(I => I.Id, I => I);
            }
            catch (Exception error)
            {
                EmpyrionScripting.ModApi.LogError($"ReadAllItemData:{error}");
            }
        }

        public IEnumerable<ItemInfo> GetAllItems(string contentPath)
        {
            var ItemDef = File.ReadAllLines(Path.Combine(contentPath, @"Configuration\Config_Example.ecf"))
                .Where(L => L.Contains(IdDef));

            return ItemDef.Select(L =>
            {
                var IdPos = L.IndexOf(IdDef);
                var IdDelimiter = L.IndexOf(",", IdPos);
                var NamePos = L.IndexOf(NameDef);
                var NameDelimiter = L.IndexOf(",", NamePos);
                if (NameDelimiter == -1) NameDelimiter = L.Length;

                return IdPos >= 0 && NamePos >= 0 && IdDelimiter >= 0
                    ? new ItemInfo()
                    {
                        Id   = int.TryParse(L.Substring(IdPos + IdDef.Length, IdDelimiter - IdPos - IdDef.Length), out int Result) ? Result : 0,
                        Key  = L.Substring(NamePos + NameDef.Length, NameDelimiter - NamePos - NameDef.Length).Trim(),
                        Name = L.Substring(NamePos + NameDef.Length, NameDelimiter - NamePos - NameDef.Length).Trim(),
                    }
                    : null;
            })
            .Where(I => I != null)
            .Select(I =>
            {
                I.Name = Localization.GetName(I.Key, "English");
                return I;
            })
            .ToArray();

        }
    }
}
