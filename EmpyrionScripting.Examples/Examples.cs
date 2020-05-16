using EmpyrionScripting.Interface;
using System;
using System.Collections.Generic;
using System.Linq;

namespace EmpyrionScripting.Examples
{
    public class Examples
    {
        public string Example1(IScriptModData g)
        {
            var E = g.E;
            return
            $"Name: {E.Name}" +
            $"Id: {E.Id}" +
            $"DeviceNames: " +
            string.Join(",", E.DeviceNames) +
            $"EntityType: {E.EntityType}" +
            $"Faction: Id:{E.Faction.Id} Group:{E.Faction.Group}"
            ;
        }
        public string Example2(IScriptModData g)
        {
            var E = g.E;

            return
            $"GetCurrentPlayfield: {E.GetCurrentPlayfield().Name}\n" +
            $"GetCurrent: {E.GetCurrent()}\n"
            ;

        }

        public string Example3(IScriptModData g)
        {
            var S = g.E.S;

            return
            "Player StructureData\n" +
            $"GetCurrentPlayfield: {S.DockedE}\n" +
            $"AllCustomDeviceNames: {S.AllCustomDeviceNames}\n" +
            $"ControlPanelSignals: {S.ControlPanelSignals}\n" +
            $"BlockSignals: {S.BlockSignals}\n" +
            $"ContainerSource: {S.ContainerSource}\n" +
            $"DamageLevel: {S.DamageLevel}\n" +
            $"GetDeviceTypeNames: {S.GetDeviceTypeNames}\n" +
            $"IsOfflineProtectable: {S.IsOfflineProtectable}\n" +
            $"IsPowerd: {S.IsPowerd}\n" +
            $"IsReady: {S.IsReady}\n" +
            $"E: {S.E}\n" +
            $"Items: {S.Items}\n" +
            $"OxygenTank: {S.OxygenTank}\n" +
            $"FuelTank: {S.FuelTank}\n" +
            $"PentaxidTank: {S.PentaxidTank}\n" +
            $"Passengers: {S.Passengers}\n" +
            $"Pilot: {S.Pilot}\n"
            ;
        }

        public string Example4(IScriptModData g)
        {
            var I = g.E.S.Items.FirstOrDefault();
            var s = I?.Source?.FirstOrDefault();

            return
            "Player ItemsData\n" +
            $"Id: {I?.Id}\n" +
            $"Key: {I?.Key}\n" +
            $"Name: {I?.Name}\n" +
            $"Count: {I?.Count}\n" +
            $"Source: {I?.Source?.FirstOrDefault()}\n" +
            $"CustomName: {s?.CustomName}\n" +
            $"Id: {s?.Id}\n" +
            $"Count: {s?.Count}\n" +
            $"E: {s?.E}\n" +
            $"Position: {s?.Position}\n" +
            ""
            ;
        }

        public string Example5(IScriptModData g)
        {
            var p = g.E.S.Pilot;

            return
            "Player PlayerData\n" +
            $"Id: {p.Id}\n" +
            $"Name: {p.Name}\n" +
            $"SteamId: {p.SteamId}\n" +
            $"Ping: {p.Ping}\n" +
            "\n" +
            $"BodyTemp: {p.BodyTemp}\n" +
            $"BodyTempMax: {p.BodyTempMax}\n" +
            $"Food: {p.Food}\n" +
            $"FoodMax: {p.FoodMax}\n" +
            $"Health: {p.Health}\n" +
            $"HealthMax: {p.HealthMax}\n" +
            $"Radiation: {p.Radiation}\n" +
            $"RadiationMax: {p.RadiationMax}\n" +
            $"Stamina: {p.Stamina}\n" +
            $"StaminaMax: {p.StaminaMax}\n" +
            $"Oxygen: {p.Oxygen}\n" +
            $"OxygenMax: {p.OxygenMax}\n" +
            "\n" +
            $"Bag: {p.Bag?.Count()}\n" +
            $"Toolbar: {p.Toolbar?.Count()}\n" +
            "\n" +
            $"Credits: {p.Credits}\n" +
            $"Died: {p.Died}\n" +
            $"ExperiencePoints: {p.ExperiencePoints}\n" +
            $"UpgradePoints: {p.UpgradePoints}\n" +
            $"FactionData: {p.FactionData}\n" +
            $"FactionRole: {p.FactionRole}\n" +
            $"Kills: {p.Kills}\n" +
            $"Origin: {p.Origin}\n" +
            $"StartPlayfield: {p.StartPlayfield}\n" +
            ""
            ;
        }


        public string Example6(IScriptModData g)
        {
            var f = g.E.S.FuelTank;

            return
            "Player StructureTank\n" +
            $"Capacity: {f.Capacity}\n" +
            $"Content: {f.Content}\n" +
            $"AllowedItem: {f.AllowedItem(2266)}\n" +
            $"ItemsNeededForFill: {f.ItemsNeededForFill(2266, 100)}\n" +
            ""
            ;
        }

        public string Example7(IScriptModData g)
        {
            var E = g.E;

            Console.WriteLine(
                $"BlockSignals: {E.S.BlockSignals.Count()}\n" +
                GetNames(E.S.BlockSignals) +
                "");

            Console.WriteLine(
                $"ControlPanelSignals: " +
                $"{E.S.ControlPanelSignals.Count()}\n" +
                GetNames(E.S.ControlPanelSignals) +
                "");

            string GetNames(IEnumerable<ISignalData> s)
            {
                return string.Join(";", s.Select(i => i.Name));
            }

            return null;
        }

    }
}
