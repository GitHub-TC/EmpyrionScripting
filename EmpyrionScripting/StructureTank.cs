using Eleon.Modding;

namespace EmpyrionScripting
{
    public class StructureTank
    {
        private IStructureTank tank;

        public StructureTank(IStructureTank fuelTank)
        {
            tank = fuelTank;
        }

        public float Capacity => tank.Capacity;
        public float Content => tank.Content;

        public IStructureTank GetCurrent() => tank;
    }
}