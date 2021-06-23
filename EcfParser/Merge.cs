using System;
using System.Collections.Generic;
using System.Linq;

namespace EcfParser
{
    public static class Merge
    {
        public static IDictionary<T, EcfBlock> EcfBlocksToDictionary<T>(this IEnumerable<EcfBlock> blocks, Func<EcfBlock, bool> blockSelector, Func<EcfBlock, T> keySelector)
            => blocks
                .Where(B => blockSelector(B))
                .Aggregate(new Dictionary<T, EcfBlock>(), (result, b) => {
                    var key = keySelector(b);
                    if (!result.ContainsKey(key)) result.Add(key, b); return result; });

        public static EcfFile Ecf(params EcfFile[] files)
        {
            EcfFile result = null;
            foreach (var ecf in files)
            {
                if (result == null) result = ecf;
                else                result.MergeWith(ecf);
            }

            return result;
        }

        public static void MergeWith(this EcfFile ecf, EcfFile add)
        {
            if(ecf.Blocks == null) ecf.Blocks = new List<EcfBlock>();

            add?.Blocks?.ForEach(B => {
                var found = ecf.Blocks
                    .Where(b => b.Name == B.Name)
                    .Where(b => Equals(b.Values?.FirstOrDefault(a => a.Key == "Id").Value  , B.Values?.FirstOrDefault(a => a.Key == "Id").Value))
                    .Where(b => Equals(b.Values?.FirstOrDefault(a => a.Key == "Name").Value, B.Values?.FirstOrDefault(a => a.Key == "Name").Value))
                    .FirstOrDefault();

                if (found == null) ecf.Blocks.Add(B); 
                else               found.MergeWith(B);
            });
        }

        public static void MergeWith(this EcfBlock destination, EcfBlock source)
        {
            if (source == null) return;

            destination.Name = source.Name;

            source.Attr?.ForEach(A =>
            {
                if (destination.Attr      == null) destination.Attr      = new List<EcfAttribute>();
                if (destination.Values    == null) destination.Values    = new Dictionary<string, object>();
                if (destination.EcfValues == null) destination.EcfValues = new Dictionary<string, EcfAttribute>();

                var foundAttr = destination.Attr.FirstOrDefault(a => a.Name == A.Name);
                if (foundAttr == null)
                {
                    destination.Attr.Add(foundAttr = new EcfAttribute()
                    {
                        Name   = A.Name,
                        Value  = A.Value,
                        AddOns = A.AddOns == null ? null : new Dictionary<string, object>(A.AddOns)
                    });

                    if (A.Name != null && !destination.EcfValues.ContainsKey(A.Name)) destination.EcfValues.Add(A.Name, foundAttr);
                    if (A.Name != null && !destination.Values   .ContainsKey(A.Name)) destination.Values   .Add(A.Name, foundAttr.Value);
                }
                else
                {
                    MergeEcfAttribute(foundAttr, A);

                    if (A.Name != null && !destination.EcfValues.ContainsKey(A.Name)) destination.EcfValues.Add(A.Name, foundAttr);
                    if (A.Name != null && !destination.Values.ContainsKey(A.Name)) destination.Values.Add(A.Name, foundAttr.Value);
                }
            });

            if (destination.Childs == null && source.Childs != null) destination.Childs = source.Childs
                     .ToDictionary(B => B.Key, B => { var block = new EcfBlock(); block.MergeWith(B.Value); return block; });
            else source.Childs?
                    .ToList()
                    .ForEach(B =>
                    {
                        if (destination.Childs.TryGetValue(B.Key, out var block)) block.MergeWith(B.Value);
                        else
                        {
                            var newBlock = new EcfBlock(); newBlock.MergeWith(B.Value);
                            destination.Childs.Add(B.Key, newBlock);
                        }
                    });

            destination.Childs?.Values
                .ToList()
                .ForEach(B => { 
                    B.EcfValues?.Where(A => !destination.EcfValues.ContainsKey(A.Key))
                    .ToList()
                    .ForEach(A =>
                    {
                        if (destination.EcfValues == null) destination.EcfValues = new Dictionary<string, EcfAttribute>();
                        if (destination.Values    == null) destination.Values    = new Dictionary<string, object>();

                        destination.EcfValues.Add(A.Key, A.Value);
                        destination.Values   .Add(A.Key, A.Value.Value);
                    });
                });

            if (source.EcfValues != null && destination.EcfValues == null) destination.EcfValues = new Dictionary<string, EcfAttribute>(source.EcfValues);
            else if (source.EcfValues != null) foreach (var item in source.EcfValues)
                {
                    if (destination.EcfValues.TryGetValue(item.Key, out var attr)) MergeEcfAttribute(attr, item.Value);
                    else                                                           destination.EcfValues.Add(item.Key, item.Value);
                }

            if (source.Values != null && destination.Values == null) destination.Values = new Dictionary<string, object>(source.Values);
            else if (source.Values != null) foreach (var item in source.Values)
                {
                    if (destination.Values.ContainsKey(item.Key)) destination.Values[item.Key] = item.Value;
                    else                                          destination.Values.Add(item.Key, item.Value);
                }

        }

        private static void MergeEcfAttribute(EcfAttribute dest, EcfAttribute source)
        {
            dest.Value = source.Value;
            if (dest.AddOns == null && source.AddOns != null) dest.AddOns = new Dictionary<string, object>(source.AddOns);
            else source.AddOns?.ToList().ForEach(P =>
            {
                if (dest.AddOns.ContainsKey(P.Key)) dest.AddOns[P.Key] = P.Value;
                else dest.AddOns.Add(P.Key, P.Value);
            });
        }
    }
}
