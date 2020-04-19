using EmpyrionScripting.Interface;

namespace EmpyrionScripting
{
    public class DisplayOutputConfiguration : IDisplayOutputConfiguration
    {
        public int Lines { get; set; }
        public bool AppendAtEnd { get; set; }
    }
}
