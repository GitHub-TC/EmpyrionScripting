using System.Collections.Generic;

namespace EcfParser
{
    public class EcfBlock
    {
        public string Name { get; set; }
        public List<EcfAttribute> Attr { get; set; }
        public IDictionary<string, EcfBlock> Childs { get; set; }
    }
}
