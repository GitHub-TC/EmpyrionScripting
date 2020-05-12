using System.Collections.Generic;

namespace EcfParser
{
    public class EcfAttribute
    {
        public string Name { get; set; }
        public object Value { get; set; }
        public IDictionary<string, object> AddOns { get; set; }
    }
}
