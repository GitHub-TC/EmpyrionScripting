using System.Collections.Generic;

namespace EcfParser
{
    public class EcfFile
    {
        public int Version { get; set; }
        public List<EcfBlock> Blocks { get; set; }
    }
}
