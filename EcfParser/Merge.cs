using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
                    .Where(b => Equals(b.Attributes.FirstOrDefault(a => a.Name == "Id")?.Value  , B.Attributes.FirstOrDefault(a => a.Name == "Id")?.Value))
                    .Where(b => Equals(b.Attributes.FirstOrDefault(a => a.Name == "Name")?.Value, B.Attributes.FirstOrDefault(a => a.Name == "Name")?.Value))
                    .FirstOrDefault();

                if (found != null)
                {
                    B.Attributes.ForEach(A => {
                        var foundAttr = found.Attributes.FirstOrDefault(a => a.Name == A.Name);
                        if (foundAttr == null) found.Attributes.Add(A);
                        else
                        {
                            foundAttr.Value = A.Value;
                            if (foundAttr.AdditionalPayload == null) foundAttr.AdditionalPayload = A.AdditionalPayload;
                            else A.AdditionalPayload?.ToList().ForEach(P => {
                                if (foundAttr.AdditionalPayload.ContainsKey(P.Key)) foundAttr.AdditionalPayload[P.Key] = P.Value; 
                                else                                                foundAttr.AdditionalPayload.Add(P.Key, P.Value);
                            });
                        }
                    });
                }
            });
        }
    }
}
