using Eleon.Modding;
using EmpyrionScripting.Interface;

namespace EmpyrionScripting.DataWrapper
{
    public class ContainerSource : IContainerSource
    {
        public IEntityData E { get; set; }
        public IContainer Container { get; set; }
        public string CustomName { get; set; }
        public VectorInt3 Position { get; set; }
        public float VolumeCapacity => Container.VolumeCapacity;
        public float DecayFactor => Container.DecayFactor;
        public int MaxSlots { get; set; }
    }
}