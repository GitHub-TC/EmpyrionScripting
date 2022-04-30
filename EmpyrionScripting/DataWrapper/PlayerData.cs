using Eleon.Modding;
using EmpyrionNetAPIDefinitions;
using EmpyrionScripting.Interface;
using System;
using System.Collections.Generic;
using UnityEngine;

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

        public IPlayer GetCurrent() => p;
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

        public bool Teleport(Vector3 pos) => SafeGet("Teleport", p, () => EmpyrionScripting.ModApi.Network.SendToDedicatedServer("EmpyrionScripting", SerializeHelper.ObjectToByteArray(
                new TeleportPlayerData { 
                    PlayerId = p.Id,
                    Position = pos,
                }),
                currentPlayfield.Name) || p.Teleport(pos)
        );
        public bool Teleport(string playfieldName, Vector3 pos, Vector3 rot) => SafeGet("Teleport", p, () => p.Teleport(playfieldName, pos, rot));

    }

    [Serializable]
    public class TeleportPlayerData
    {
        public int PlayerId { get; set; }
        public Vector3 Position { get; set; }
        public override string ToString() => $"PlayerId:{PlayerId} Position:{Position}";
    }

    public class PlayerCommandsDediHelper{
        public IModApi ModApi { get; }
        public static Action<string, LogLevel> Log { get; set; }

        public PlayerCommandsDediHelper(IModApi modApi)
        {
            ModApi = modApi;

            try
            {
                if (!ModApi.Network.RegisterReceiverForPlayfieldPackets(CommandCallback)) Log("RegisterReceiverForPlayfieldPackets failed", LogLevel.Error);
            }
            catch (Exception error)
            {
                Log($"PlayerCommandsDediHelper: {error}", LogLevel.Error);
            }
        }

        private void CommandCallback(string sender, string playfieldName, byte[] data)
        {
            Log($"EmpyrionScripting:CommandCallback from {playfieldName} -> {sender}", LogLevel.Message);

            if (sender != "EmpyrionScripting") return;

            try
            {
                var teleportData = SerializeHelper.ByteArrayToObject<TeleportPlayerData>(data);
                Log($"EmpyrionScripting:Teleport call from {playfieldName} -> {teleportData}", LogLevel.Message);

                //var playerData = ModApi.Application.GetPlayerDataFor(teleportData.PlayerId);
                //playerData.Value.t
            }
            catch (Exception error)
            {
                Log($"EmpyrionScripting:Teleport {error}", LogLevel.Error);
            }
        }
    }
}