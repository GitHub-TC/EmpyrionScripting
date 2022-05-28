using System;
using System.Collections.Generic;
using System.Linq;
using Eleon.Modding;

namespace EmpyrionScripting
{
    public static class ItemTokenAccess
    {
        public static Lazy<HashSet<int>> TokenItemIds { get; } = new Lazy<HashSet<int>>(() =>
            EmpyrionScripting.ConfigEcfAccess?.FlatConfigBlockById
                .Where(B => B.Value.Values?.TryGetValue("Class", out var classType) == true && classType.ToString() == "Token")
                .Select(B => B.Key)
                .ToHashSet()
        );

        public static int TokenIdSeperator = 100000;

        public static int ItemId(this int itemId) => itemId % TokenIdSeperator;
        public static int TokenId(this int itemId) => itemId / TokenIdSeperator;
        public static bool IsToken(this int itemId) => itemId >= TokenIdSeperator;

        public static int CreateId(this ItemStack item) 
            => TokenItemIds.Value.Contains(item.id)
            ? item.id + item.ammo * TokenIdSeperator
            : item.id;
    }
}