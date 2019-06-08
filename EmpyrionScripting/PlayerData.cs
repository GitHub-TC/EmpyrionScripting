using Eleon.Modding;

namespace EmpyrionScripting
{
    public class PlayerData
    {
        private IPlayer p;

        public PlayerData(IPlayer p)
        {
            this.p = p;
        }

        public string Name => p.Name;
    }
}