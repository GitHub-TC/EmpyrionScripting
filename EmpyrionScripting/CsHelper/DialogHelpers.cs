using Eleon.Modding;
using EmpyrionScripting.CustomHelpers;
using EmpyrionScripting.Interface;
using System;
using System.Linq;

namespace EmpyrionScripting.CsHelper
{
    public partial class CsScriptFunctions : ICsScriptFunctions
    {
        public bool ShowDialog(int playerId, DialogConfig dialogConfig, DialogActionHandler actionHandler, int customValue)
        {
            try
            {
                var playerData = Root.IsElevatedScript
                    ? (Root.GetCurrentPlayfield().Players.TryGetValue(playerId, out var plainPlayer) ? new DataWrapper.PlayerData(plainPlayer) : null)
                    : Root.E.S.Players.FirstOrDefault(P => P.Id == playerId);

                if (playerData == null) return false;

                return EmpyrionScripting.ModApi.Application.ShowDialogBox(playerId, dialogConfig, actionHandler, customValue);
            }
            catch (Exception error)
            {
                if (!FunctionNeedsMainThread(error, Root)) Console.WriteLine("{{ShowDialog}} error " + EmpyrionScripting.ErrorFilter(error));
                return false;
            }
        }

        public bool ShowDialog(string signalNames, DialogConfig dialogConfig, DialogActionHandler actionHandler, int customValue)
        {
            try
            {
                var playerId = 0;

                var uniqueNames = Root.SignalEventStore.GetEvents().Keys.GetUniqueNames(signalNames).ToDictionary(N => N);
                var signals = Root.SignalEventStore.GetEvents()
                    .Where(S => uniqueNames.ContainsKey(S.Key))
                    .Select(S => S.Value.LastOrDefault())
                    .Where(S => S != null)
                    .ToArray();

                if (signals.All(S => !S.State))
                {
                    Root.GetPersistendData().TryRemove($"DialogTo{Root.ScriptId}", out var _);
                    return false;
                }
                else playerId = signals.FirstOrDefault(S => S.State)?.TriggeredByEntityId ?? 0;

                var playerData = Root.IsElevatedScript
                    ? (Root.GetCurrentPlayfield().Players.TryGetValue(playerId, out var plainPlayer) ? new DataWrapper.PlayerData(plainPlayer) : null)
                    : Root.E.S.Players.FirstOrDefault(P => P.Id == playerId);

                if (playerData == null) return false;

                var running = (bool)Root.GetPersistendData().GetOrAdd($"DialogTo{Root.ScriptId}", false);
                if (running) return false;

                var success = EmpyrionScripting.ModApi.Application.ShowDialogBox(playerId, dialogConfig, actionHandler, customValue);

                if (success) Root.GetPersistendData().AddOrUpdate($"DialogTo{Root.ScriptId}", true, (key, value) => true);

                return success;
            }
            catch (Exception error)
            {
                if (!FunctionNeedsMainThread(error, Root)) Console.WriteLine("{{ShowDialog}} error " + EmpyrionScripting.ErrorFilter(error));
                return false;
            }
        }

        public bool ShowDialog(string signalNames, Func<IPlayerData, DialogConfig> dialogConfig, DialogActionHandler actionHandler, int customValue)
        {
            try
            {
                var playerId = 0;

                var uniqueNames = Root.SignalEventStore.GetEvents().Keys.GetUniqueNames(signalNames).ToDictionary(N => N);
                var signals = Root.SignalEventStore.GetEvents()
                    .Where(S => uniqueNames.ContainsKey(S.Key))
                    .Select(S => S.Value.LastOrDefault())
                    .Where(S => S != null)
                    .ToArray();

                if (signals.All(S => !S.State))
                {
                    Root.GetPersistendData().TryRemove($"DialogTo{Root.ScriptId}", out var _);
                    return false;
                }
                else playerId = signals.FirstOrDefault(S => S.State)?.TriggeredByEntityId ?? 0;

                var playerData = Root.IsElevatedScript
                    ? (Root.GetCurrentPlayfield().Players.TryGetValue(playerId, out var plainPlayer) ? new DataWrapper.PlayerData(plainPlayer) : null)
                    : Root.E.S.Players.FirstOrDefault(P => P.Id == playerId);

                if (playerData == null) return false;

                var running = (bool)Root.GetPersistendData().GetOrAdd($"DialogTo{Root.ScriptId}", false);
                if (running) return false;

                var success = EmpyrionScripting.ModApi.Application.ShowDialogBox(playerId, dialogConfig(playerData), actionHandler, customValue);

                if (success) Root.GetPersistendData().AddOrUpdate($"DialogTo{Root.ScriptId}", true, (key, value) => true);

                return success;
            }
            catch (Exception error)
            {
                if (!FunctionNeedsMainThread(error, Root)) Console.WriteLine("{{ShowDialog}} error " + EmpyrionScripting.ErrorFilter(error));
                return false;
            }
        }
    }
}
