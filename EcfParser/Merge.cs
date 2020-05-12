using System.Collections.Generic;
using System.Linq;

namespace EcfParser
{
    public static class Merge
    {
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
            add.Blocks.ForEach(B => {
                var found = ecf.Blocks
                    .Where(b => b.Name == B.Name)
                    .Where(b => Equals(b.Attr.FirstOrDefault(a => a.Name == "Id")?.Value  , B.Attr.FirstOrDefault(a => a.Name == "Id")?.Value))
                    .Where(b => Equals(b.Attr.FirstOrDefault(a => a.Name == "Name")?.Value, B.Attr.FirstOrDefault(a => a.Name == "Name")?.Value))
                    .FirstOrDefault();

                if (found != null)
                {
                    found.MergeWith(B);
                }
            });
        }

        public static void MergeWith(this EcfBlock destination, EcfBlock source)
        {
            if (source == null) return;

            destination.Name = source.Name;

            source.Attr?.ForEach(A =>
            {
                if (destination.Attr == null) destination.Attr = new List<EcfAttribute>();

                var foundAttr = destination.Attr.FirstOrDefault(a => a.Name == A.Name);
                if (foundAttr == null) destination.Attr.Add(new EcfAttribute()
                {
                    Name                = A.Name,
                    Value               = A.Value,
                    AddOns   = A.AddOns == null ? null : new Dictionary<string, object>(A.AddOns)
                });
                else
                {
                    foundAttr.Value = A.Value;
                    if (foundAttr.AddOns == null && A.AddOns != null) foundAttr.AddOns = new Dictionary<string, object>(A.AddOns);
                    else A.AddOns?.ToList().ForEach(P =>
                    {
                        if (foundAttr.AddOns.ContainsKey(P.Key)) foundAttr.AddOns[P.Key] = P.Value;
                        else                                                foundAttr.AddOns.Add(P.Key, P.Value);
                    });
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
        }
    }
}
