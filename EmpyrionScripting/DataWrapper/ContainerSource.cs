using Eleon.Modding;

namespace EmpyrionScripting.DataWrapper
{
    public class ContainerSource
    {
        public IEntityData E { get; set; }
        public IContainer Container { get; set; }
        public string CustomName { get; set; }
        public VectorInt3 Position { get; set; }
    }
}