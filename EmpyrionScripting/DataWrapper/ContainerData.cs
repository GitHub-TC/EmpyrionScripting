using Eleon.Modding;

namespace EmpyrionScripting.DataWrapper
{
    public class ContainerData
    {
        private IContainer _container;

        public ContainerData(IContainer container)
        {
            _container = container;
        }

        public float VolumeCapacity => _container.VolumeCapacity;
        public float DecayFactor => _container.DecayFactor;
    }
}
