using System.Collections.Generic;

namespace EcfParser
{
    public class EcfBlock
    {
        public string Name { get; set; }
        public List<EcfAttribute> Attr { get; set; }
        public IDictionary<string, EcfBlock> Childs { get; set; }

        /// <summary>
        /// Flache Liste der gefunden Attribute und deren primären Werte
        /// </summary>
        public IDictionary<string, object> Values { get; set; }
        /// <summary>
        /// Flache Liste der gefunden Attribute
        /// </summary>
        public IDictionary<string, EcfAttribute> EcfValues { get; set; }

    }
}
