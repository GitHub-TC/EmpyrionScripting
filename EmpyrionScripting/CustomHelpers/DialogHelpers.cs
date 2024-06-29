using Eleon;
using EmpyrionScripting.CsHelper;
using EmpyrionScripting.DataWrapper;
using EmpyrionScripting.Interface;
using HandlebarsDotNet;
using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Runtime.Remoting.Channels;
using System.Xml.Linq;
using UnityEngine;

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
                    ? (root.GetCurrentPlayfield().Players.TryGetValue(playerId, out var plainPlayer) ? new PlayerData(root.GetCurrentPlayfield(), plainPlayer) : null)
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
                            ? (root.GetCurrentPlayfield().Players.TryGetValue(playerId, out var plainPlayer) ? new PlayerData(root.GetCurrentPlayfield(), plainPlayer) : null)
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

        [HandlebarTag("dialogbox")]
        public static void DialogBoxHelper(TextWriter output, object rootObject, HelperOptions options, dynamic context, object[] arguments)
        {
            if (arguments.Length <= 1) throw new HandlebarsException("{{dialog player|Id|SignalName}} helper must have at least one argument ((title) (body) (ButtonTexts) came from the {{else}} part): (player | playerId | SignalName) [ButtonIdxForEnter] [ButtonIdxForEsc] [MaxChars] [InitialPlayerInput] [closeOnLinkClick] [DialogData] [Placeholder]");

            var root = rootObject as ScriptRootData;

            var playerId = 0;
            int.TryParse(arguments.Get(0)?.ToString(), out playerId);
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
                    ? (root.GetCurrentPlayfield().Players.TryGetValue(playerId, out var plainPlayer) ? new PlayerData(root.GetCurrentPlayfield(), plainPlayer) : null)
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

                var askData = new StringWriter();
                options.Inverse(askData, playerData);
                var askDataLines = askData.ToString().Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);

                var success = EmpyrionScripting.ModApi.Application.ShowDialogBox(playerId,
                    new Eleon.Modding.DialogConfig()
                    {
                        TitleText               = askDataLines.First(),
                        BodyText                = string.Join("\n", askDataLines.Skip(1).Take(askDataLines.Length - 2)),
                        ButtonTexts             = askDataLines.Last().Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries),
                        ButtonIdxForEnter       = int.TryParse(arguments.Get(1)?.ToString(), out var enterBtn) ? enterBtn : 0,
                        ButtonIdxForEsc         = int.TryParse(arguments.Get(2)?.ToString(), out var escBtn  ) ? escBtn : 0,
                        CloseOnLinkClick        = closeOnLinkClick,
                        MaxChars                = int.TryParse(arguments.Get(3)?.ToString(), out var maxChars) ? maxChars : 0,
                        InitialContent          = arguments.Get(4)?.ToString(),
                        Placeholder             = arguments.Get(7)?.ToString()
                    },
                    (buttonIdx, linkId, content, playerId, customValue) =>
                    {
                        var playerData = root.IsElevatedScript 
                            ? (root.GetCurrentPlayfield().Players.TryGetValue(playerId, out var plainPlayer) ? new PlayerData(root.GetCurrentPlayfield(), plainPlayer) : null)
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
                    int.TryParse(arguments.Get(6)?.ToString(), out var customValue) ? customValue : 0);

                if (success) root.GetPersistendData().AddOrUpdate($"DialogTo{root.ScriptId}", true, (key, value) => true);
            }
            catch (Exception error)
            {
                if (!CsScriptFunctions.FunctionNeedsMainThread(error, root)) output.Write("{{dialog}} error " + EmpyrionScripting.ErrorFilter(error));
                root.GetPersistendData().AddOrUpdate($"DialogTo{root.ScriptId}", false, (key, value) => false);
            }
        }

        [HandlebarTag("chatbysignal")]
        public static void ChatbySignalHelper(TextWriter output, object rootObject, HelperOptions options, dynamic context, object[] arguments)
        {
            if (arguments.Length != 2) throw new HandlebarsException("{{chatbysignal SignalName sender}} helper must have three argument: sender");

            var root = rootObject as ScriptRootData;

            var playerId = 0;
            var uniqueNames = root.SignalEventStore.GetEvents().Keys.GetUniqueNames(arguments.Get(0)?.ToString()).ToDictionary(N => N);
            var signals = root.SignalEventStore.GetEvents()
                .Where(S => uniqueNames.ContainsKey(S.Key))
                .Select(S => S.Value.LastOrDefault())
                .Where(S => S != null)
                .ToArray();

            if (signals.All(S => !S.State))
            {
                root.GetPersistendData().TryRemove($"ChatTo{root.ScriptId}", out var _);
                return;
            }
            else playerId = signals.FirstOrDefault(S => S.State)?.TriggeredByEntityId ?? 0;

            if (playerId == 0) return;

            try
            {
                EmpyrionScripting.Log($"chatbysignal:[{arguments.Get(0)?.ToString()}] {playerId} [{arguments.Get(1)?.ToString()}] '{arguments.Get(2)?.ToString()}'", EmpyrionNetAPIDefinitions.LogLevel.Debug);

                root.GetCurrentPlayfield().Players.TryGetValue(playerId, out var player);

                var msg = new StringWriter();
                options.Template(msg, player != null ? new LimitedPlayerData(player) : null);

                EmpyrionScripting.ModApi.Application.SendChatMessage(new Eleon.MessageData
                {
                    Channel            = MsgChannel.SinglePlayer,
                    RecipientEntityId  = playerId,
                    SenderNameOverride = arguments.Get(1)?.ToString(),
                    SenderType         = Eleon.SenderType.ServerPrio,
                    Text               = msg.ToString(),
                }
                );

                root.GetPersistendData().AddOrUpdate($"ChatTo{root.ScriptId}", true, (key, value) => true);
            }
            catch (Exception error)
            {
                if (!CsScriptFunctions.FunctionNeedsMainThread(error, root)) output.Write("{{chatbysignal}} error " + EmpyrionScripting.ErrorFilter(error));
                root.GetPersistendData().AddOrUpdate($"ChatTo{root.ScriptId}", false, (key, value) => false);
            }
        }

        [HandlebarTag("chat")]
        public static void ChatHelper(TextWriter output, object rootObject, dynamic context, object[] arguments)
        {
            if (arguments.Length != 2) throw new HandlebarsException("{{char sender text}} helper must have two argument: sender text");

            var root = rootObject as ScriptRootData;

            EmpyrionScripting.Log($"chat: Group:{root.E.Faction.Group} Id:{root.E.Faction.Id} '{arguments.Get(1)?.ToString()}'", EmpyrionNetAPIDefinitions.LogLevel.Debug);
            var msg = new Eleon.MessageData
            {
                SenderNameOverride = arguments.Get(0)?.ToString(),
                SenderType         = Eleon.SenderType.ServerPrio,
                Text               = arguments.Get(1)?.ToString(),
            };

            if(root.E.Faction.Group == FactionGroup.Faction)
            {
                msg.Channel          = MsgChannel.Faction;
                msg.RecipientFaction = root.E.Faction;
            }
            else
            {
                msg.Channel           = MsgChannel.SinglePlayer;
                msg.RecipientEntityId = root.E.Faction.Id;
            }

            EmpyrionScripting.ModApi.Application.SendChatMessage(msg);
        }

        [HandlebarTag("chatglobal")]
        public static void ChatGlobalHelper(TextWriter output, object rootObject, dynamic context, object[] arguments)
        {
            if (arguments.Length != 2) throw new HandlebarsException("{{chatglobal sender text}} helper must have two argument: sender text");

            var root = rootObject as ScriptRootData;
            if (!root.IsElevatedScript) throw new HandlebarsException("{{chatglobal}} only allowed in elevated scripts");

            EmpyrionScripting.Log($"chatglobal:[{arguments.Get(0)?.ToString()}] '{arguments.Get(1)?.ToString()}'", EmpyrionNetAPIDefinitions.LogLevel.Debug);
            EmpyrionScripting.ModApi.Application.SendChatMessage(new Eleon.MessageData
                { 
                    Channel            = MsgChannel.Global,
                    SenderNameOverride = arguments.Get(0)?.ToString(),
                    SenderType         = Eleon.SenderType.ServerPrio,
                    Text               = arguments.Get(1)?.ToString(),
                }
            );
        }

        [HandlebarTag("chatserver")]
        public static void ChatServerHelper(TextWriter output, object rootObject, dynamic context, object[] arguments)
        {
            if (arguments.Length != 2) throw new HandlebarsException("{{chatserver sender text}}  helper must have two argument: sender text");

            var root = rootObject as ScriptRootData;
            if (!root.IsElevatedScript) throw new HandlebarsException("{{chatserver}} only allowed in elevated scripts");

            EmpyrionScripting.Log($"chatserver:[{arguments.Get(0)?.ToString()}] '{arguments.Get(1)?.ToString()}'", EmpyrionNetAPIDefinitions.LogLevel.Debug);
            EmpyrionScripting.ModApi.Application.SendChatMessage(new Eleon.MessageData
                { 
                    Channel            = MsgChannel.Server,
                    SenderNameOverride = arguments.Get(0)?.ToString(),
                    SenderType         = Eleon.SenderType.ServerPrio,
                    Text               = arguments.Get(1)?.ToString(),
                }
            );
        }

        [HandlebarTag("chatplayer")]
        public static void ChatPlayerHelper(TextWriter output, object rootObject, dynamic context, object[] arguments)
        {
            if (arguments.Length != 3) throw new HandlebarsException("{{chatplayer playerId sender text}} helper must have three argument: playerId sender text");

            var root = rootObject as ScriptRootData;
            if (!root.IsElevatedScript) throw new HandlebarsException("{{chatplayer}} only allowed in elevated scripts");

            int.TryParse(arguments.Get(0)?.ToString(), out var PlayerId);

            if(PlayerId == 0) return;

            EmpyrionScripting.Log($"chatglobal:{PlayerId} [{arguments.Get(1)?.ToString()}] '{arguments.Get(2)?.ToString()}'", EmpyrionNetAPIDefinitions.LogLevel.Debug);
            EmpyrionScripting.ModApi.Application.SendChatMessage(new Eleon.MessageData
                {
                    Channel            = MsgChannel.SinglePlayer,
                    RecipientEntityId  = PlayerId,
                    SenderNameOverride = arguments.Get(1)?.ToString(),
                    SenderType         = Eleon.SenderType.ServerPrio,
                    Text               = arguments.Get(2)?.ToString(),
                }
            );
        }

        [HandlebarTag("chatfaction")]
        public static void ChatFactionHelper(TextWriter output, object rootObject, dynamic context, object[] arguments)
        {
            if (arguments.Length != 3) throw new HandlebarsException("{{chatfaction factionId sender text}} helper must have three argument: factionId sender text");

            var root = rootObject as ScriptRootData;
            if (!root.IsElevatedScript) throw new HandlebarsException("{{chatfaction}} only allowed in elevated scripts");

            int.TryParse(arguments.Get(0)?.ToString(), out var RecipientFactionId);

            if (RecipientFactionId == 0) return;

            EmpyrionScripting.Log($"chatglobal:{RecipientFactionId} [{arguments.Get(1)?.ToString()}] '{arguments.Get(2)?.ToString()}'", EmpyrionNetAPIDefinitions.LogLevel.Debug);
            EmpyrionScripting.ModApi.Application.SendChatMessage(new Eleon.MessageData
                {
                    Channel            = MsgChannel.Faction,
                    RecipientFaction   = new FactionData { Group = FactionGroup.Faction, Id = RecipientFactionId },
                    SenderNameOverride = arguments.Get(1)?.ToString(),
                    SenderType         = Eleon.SenderType.ServerPrio,
                    Text               = arguments.Get(2)?.ToString(),
                }
            );
        }

    }
}           
