using Eleon.Modding;

namespace EmpyrionScripting.DataWrapper
{
    public class StructureTank : IStructureTankWrapper
    {
        private IStructureTank tank;

        public StructureTank(IStructureTank tank)
        {
            this.tank = tank;
        }

        public float Capacity => tank.Capacity;
        public float Content => tank.Content;

        public IStructureTank GetCurrent() => tank;
    }
}