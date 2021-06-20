using Eleon.Modding;
using EmpyrionScripting.Interface;
using System;
using System.Collections.Generic;

namespace EmpyrionScripting.DataWrapper
{
    public class LimitedPlayerData : ILimitedPlayerData
    {
        protected IPlayer p;

        public LimitedPlayerData(IPlayer p)
        {
            this.p = p;
        }

        protected T SafeGet<T>(string func, object check, Func<T> getFunction)
        {
            try
            {
                return check == null ? default : getFunction();
            }
            catch (Exception error)
            {
                EmpyrionScripting.Log($"ReadFailed({func}): {error}", EmpyrionNetAPIDefinitions.LogLevel.Debug);
                return default;
            }
        }


        public int Id => SafeGet("Id", p, () => p.Id);
        public string Name => p?.Name;
    }

    public class PlayerData : LimitedPlayerData, IPlayerData
    {
        private readonly IPlayfield currentPlayfield;

        public PlayerData(IPlayfield playfield, IPlayer p) : base(p)
        {
            currentPlayfield = playfield;
        }

        public int Food => SafeGet("Food", p, () => (int)p.Food);
        public int Ping => SafeGet("Ping", p, () => p.Ping);
        public int UpgradePoints => SafeGet("UpgradePoints", p, () => p.UpgradePoints);
        public int ExperiencePoints => SafeGet("ExperiencePoints", p, () => p.ExperiencePoints);
        public double Credits => SafeGet("Credits", p, () => p.Credits);
        public int Died => SafeGet("Died", p, () => p.Died);
        public int Kills => SafeGet("Kills", p, () => p.Kills);
        public float BodyTempMax => SafeGet("BodyTempMax", p, () => p.BodyTempMax);
        public float BodyTemp => SafeGet("BodyTemp", p, () => p.BodyTemp);
        public float RadiationMax => SafeGet("RadiationMax", p, () => p.RadiationMax);
        public float Radiation => SafeGet("Radiation", p, () => p.Radiation);
        public float FoodMax => SafeGet("FoodMax", p, () => p.FoodMax);
        public List<ItemStack> Toolbar => SafeGet("Toolbar", p, () => p?.Toolbar);
        public List<ItemStack> Bag => SafeGet("Bag", p, () => p?.Bag);
        public float Stamina => SafeGet("Stamina", p, () => p.Stamina);
        public float OxygenMax => SafeGet("OxygenMax", p, () => p.OxygenMax);
        public float Oxygen => SafeGet("Oxygen", p, () => p.Oxygen);
        public float HealthMax => SafeGet("HealthMax", p, () => p.HealthMax);
        public float Health => SafeGet("Health", p, () => p.Health);
        public FactionRole FactionRole => SafeGet("FactionRole", p, () => p.FactionRole);
        public FactionData FactionData => SafeGet("FactionData", p, () => p.FactionData);
        public byte Origin => SafeGet("Origin", p, () => p.Origin);
        public string StartPlayfield => SafeGet("StartPlayfield", p, () => p?.StartPlayfield);
        public string SteamId => SafeGet("SteamId", p, () => p?.SteamId);
        public float StaminaMax => SafeGet("StaminaMax", p, () => p.StaminaMax);

        public bool IsPilot => SafeGet("IsPilot", p, () => p.IsPilot);

        public IEntityData DrivingEntity => SafeGet("DrivingEntity", p, () => new EntityData(currentPlayfield, p.DrivingEntity));

        public IEntityData CurrentStructure => SafeGet("CurrentStructure", p, () => new EntityData(currentPlayfield, p.CurrentStructure.Entity));

        public string SteamOwnerId => SafeGet("SteamOwnerId", p, () => p.SteamOwnerId);

        public int HomeBaseId => SafeGet("HomeBaseId", p, () => p.HomeBaseId);
    }
}