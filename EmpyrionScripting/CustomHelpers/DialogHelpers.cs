using EmpyrionNetAPITools.Extensions;
using EmpyrionScripting.CsHelper;
using EmpyrionScripting.DataWrapper;
using EmpyrionScripting.Interface;
using HandlebarsDotNet;
using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;

namespace EmpyrionScripting.CustomHelpers
{
    [HandlebarHelpers]
    public class DialogHelpers
    {

        [HandlebarTag("dialog")]
        public static void DialogHelper(TextWriter output, object rootObject, HelperOptions options, dynamic context, object[] arguments)
        {
            if (arguments.Length <= 3) throw new HandlebarsException("{{dialog player|Id|SignalName title body}} helper must have at least three argument: (player | playerId | SignalName) (title) (body) [ButtonTexts] [ButtonIdxForEnter] [ButtonIdxForEsc] [MaxChars] [InitialPlayerInput] [closeOnLinkClick] [DialogData] [Placeholder]");

            var root = rootObject as ScriptRootData;

            int.TryParse(arguments.Get(0)?.ToString(), out var playerId);
            if(arguments.Get(0) is IPlayerData player) playerId = player.Id;

            var withSignalTrigger = playerId == 0;

            if (withSignalTrigger)
            {
                var uniqueNames = root.SignalEventStore.GetEvents().Keys.GetUniqueNames(arguments.Get(0)?.ToString()).ToDictionary(N => N);
                var signals = root.SignalEventStore.GetEvents()
                    .Where(S => uniqueNames.ContainsKey(S.Key))
                    .Select(S => S.Value.LastOrDefault())
                    .Where(S => S != null)
                    .ToArray();

                if (signals.All(S => !S.State))
                {
                    root.GetPersistendData().TryRemove($"DialogTo{root.ScriptId}", out var _);
                    return;
                }
                else playerId = signals.FirstOrDefault(S => S.State)?.TriggeredByEntityId ?? 0;
            }

            if (playerId == 0) return;

            try
            {
                var playerData = root.IsElevatedScript
                    ? (root.GetCurrentPlayfield().Players.TryGetValue(playerId, out var plainPlayer) ? new PlayerData(plainPlayer) : null)
                    : root.E.S.Players.FirstOrDefault(P => P.Id == playerId);

                if (playerData == null) return;

                if (root.GetPersistendData().TryRemove($"DialogTo{root.ScriptId}LinkResult", out var linkResults))
                {
                    while (((ConcurrentQueue<DialogResult>)linkResults).TryDequeue(out var linkResult)) options.Template(output, linkResult);
                }

                if (root.GetPersistendData().TryRemove($"DialogTo{root.ScriptId}Result", out var result))
                {
                    if (!withSignalTrigger) root.GetPersistendData().TryRemove($"DialogTo{root.ScriptId}", out var _);
                    options.Template(output, result);
                    return;
                }

                var running = (bool)root.GetPersistendData().GetOrAdd($"DialogTo{root.ScriptId}", false);
                if (running) return;

                var closeOnLinkClick = !bool.TryParse(arguments.Get(8)?.ToString(), out var closeOnLink) || closeOnLink;

                var success = EmpyrionScripting.ModApi.Application.ShowDialogBox(playerId,
                    new Eleon.Modding.DialogConfig()
                    {
                        TitleText               = arguments.Get(1)?.ToString(),
                        BodyText                = arguments.Get(2)?.ToString(),
                        ButtonTexts             = arguments.Get(3) is string[] btn ? btn : arguments.Get(3)?.ToString()?.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries),
                        ButtonIdxForEnter       = int.TryParse(arguments.Get(4)?.ToString(), out var enterBtn) ? enterBtn : 0,
                        ButtonIdxForEsc         = int.TryParse(arguments.Get(5)?.ToString(), out var escBtn  ) ? escBtn : 0,
                        CloseOnLinkClick        = closeOnLinkClick,
                        MaxChars                = int.TryParse(arguments.Get(6)?.ToString(), out var maxChars) ? maxChars : 0,
                        InitialContent          = arguments.Get(7)?.ToString(),
                        Placeholder             = arguments.Get(10)?.ToString()
                    },
                    (buttonIdx, linkId, content, playerId, customValue) =>
                    {
                        var playerData = root.IsElevatedScript 
                            ? (root.GetCurrentPlayfield().Players.TryGetValue(playerId, out var plainPlayer) ? new PlayerData(plainPlayer) : null)
                            :  root.E.S.Players.FirstOrDefault(P => P.Id == playerId);

                        if (closeOnLinkClick || buttonIdx == -1)
                        {
                            root.GetPersistendData().AddOrUpdate($"DialogTo{root.ScriptId}Result",
                                new DialogResult { ButtonIdx = buttonIdx, Link = linkId, PlayerInput = content, Player = playerData, DialogData = customValue },
                                (key, value) => new DialogResult { ButtonIdx = buttonIdx, Link = linkId, PlayerInput = content, Player = playerData, DialogData = customValue });
                        }
                        else
                        {
                            root.GetPersistendData().AddOrUpdate($"DialogTo{root.ScriptId}LinkResult",
                                new ConcurrentQueue<DialogResult>(new[] { new DialogResult { ButtonIdx = buttonIdx, Link = linkId, PlayerInput = content, Player = playerData, DialogData = customValue } }),
                                (key, value) =>
                                {
                                    ((ConcurrentQueue<DialogResult>)value).Enqueue(
                                        new DialogResult
                                            {
                                                ButtonIdx   = buttonIdx,
                                                Link        = linkId,
                                                PlayerInput = content,
                                                Player      = playerData,
                                                DialogData  = customValue
                                            });
                                    return value;
                                });
                        }
                    },
                    int.TryParse(arguments.Get(9)?.ToString(), out var customValue) ? customValue : 0);

                if (success) root.GetPersistendData().AddOrUpdate($"DialogTo{root.ScriptId}", true, (key, value) => true);
            }
            catch (Exception error)
            {
                if (!CsScriptFunctions.FunctionNeedsMainThread(error, root)) output.Write("{{dialog}} error " + EmpyrionScripting.ErrorFilter(error));
                root.GetPersistendData().AddOrUpdate($"DialogTo{root.ScriptId}", false, (key, value) => false);
            }
        }
    }
}           
