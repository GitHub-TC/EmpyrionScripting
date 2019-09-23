using Eleon.Modding;

namespace EmpyrionScripting.DataWrapper
{
    public class PentaxidTank : IStructureTankWrapper
    {
        const int PentaxidItemId = 2294;
        private readonly IContainer tank;

        public PentaxidTank(IContainer tank)
        {
            this.tank = tank;
        }

        public float Capacity => tank == null ? 0 : tank.VolumeCapacity;

        public float Content => tank == null ? 0 : tank.GetTotalItems(PentaxidItemId);
    }
}
