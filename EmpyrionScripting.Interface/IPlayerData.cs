using Eleon.Modding;
using System.Collections.Generic;
using UnityEngine;

namespace EmpyrionScripting.Interface
{
    public interface IPlayerData : ILimitedPlayerData
    {
        List<ItemStack> Bag { get; }
        float BodyTemp { get; }
        float BodyTempMax { get; }
        double Credits { get; }
        int Died { get; }
        int ExperiencePoints { get; }
        FactionData FactionData { get; }
        FactionRole FactionRole { get; }
        int Food { get; }
        float FoodMax { get; }
        float Health { get; }
        float HealthMax { get; }
        int Kills { get; }
        byte Origin { get; }
        float Oxygen { get; }
        float OxygenMax { get; }
        int Ping { get; }
        float Radiation { get; }
        float RadiationMax { get; }
        float Stamina { get; }
        float StaminaMax { get; }
        string StartPlayfield { get; }
        bool IsPilot { get; }
        IEntityData DrivingEntity { get; }
        IEntityData CurrentStructure { get; }
        string SteamId { get; }
        string SteamOwnerId { get; }
        int HomeBaseId { get; }
        List<ItemStack> Toolbar { get; }
        int UpgradePoints { get; }

        bool Teleport(Vector3 pos);
        bool Teleport(string playfieldName, Vector3 pos, Vector3 rot);
    }
}