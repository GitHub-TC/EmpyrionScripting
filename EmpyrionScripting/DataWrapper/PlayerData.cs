using Eleon.Modding;
using System.Collections.Generic;

namespace EmpyrionScripting.DataWrapper
{
    public class LimitedPlayerData
    {
        protected IPlayer p;

        public LimitedPlayerData(IPlayer p)
        {
            this.p = p;
        }

        public string Name => p?.Name;
    }

    public class PlayerData : LimitedPlayerData
    {
        public PlayerData(IPlayer p) : base(p)
        {
        }

        public int Food => p == null ? 0 : (int)p.Food;
        public int Ping => p == null ? 0 : p.Ping;
        public int UpgradePoints => p == null ? 0 : p.UpgradePoints;
        public int ExperiencePoints => p == null ? 0 : p.ExperiencePoints;
        public double Credits => p == null ? 0 : p.Credits;
        public int Died => p == null ? 0 : p.Died;
        public int Kills => p == null ? 0 : p.Kills;
        public float BodyTempMax => p == null ? 0 : p.BodyTempMax;
        public float BodyTemp => p == null ? 0 : p.BodyTemp;
        public float RadiationMax => p == null ? 0 : p.RadiationMax;
        public float Radiation => p == null ? 0 : p.Radiation;
        public float FoodMax => p == null ? 0 : p.FoodMax;
        public List<ItemStack> Toolbar => p?.Toolbar;
        public List<ItemStack> Bag => p?.Bag;
        public float Stamina => p == null ? 0 : p.Stamina;
        public float OxygenMax => p == null ? 0 : p.OxygenMax;
        public float Oxygen => p == null ? 0 : p.Oxygen;
        public float HealthMax => p == null ? 0 : p.HealthMax;
        public float Health => p == null ? 0 : p.Health;
        public FactionRole FactionRole => p == null ? 0 : p.FactionRole;
        public FactionData FactionData => p == null ? new FactionData() : FactionData;
        public byte Origin => p == null ? (byte)0 : p.Origin;
        public string StartPlayfield => p?.StartPlayfield;
        public string SteamId => p?.SteamId;
        public float StaminaMax => p == null ? 0 : p.StaminaMax;
    }
}