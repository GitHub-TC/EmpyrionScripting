using EmpyrionScripting.DataWrapper;
using EmpyrionScripting.Interface;
using HandlebarsDotNet;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace EmpyrionScripting.CustomHelpers
{
    [HandlebarHelpers]
    public class SignalHelpers
    {
        [HandlebarTag("signalevents")]
        public static void SignalEventsHelper(TextWriter output, object rootObject, HelperOptions options, dynamic context, object[] arguments)
        {
            if (arguments.Length != 1) throw new HandlebarsException("{{signalevents names}} helper must have exactly one argument: (name1;name2...)");

            var root             = rootObject as IScriptRootData;
            var isElevatedScript = rootObject is ScriptSaveGameRootData || root.E.GetCurrent().Faction.Group == FactionGroup.Admin;
            var namesSearch      = arguments[0].ToString();

            try
            {
                var uniqueNames = root.SignalEventStore.GetEvents().Keys.GetUniqueNames(namesSearch).ToDictionary(N => N);

                var signals = root.SignalEventStore.GetEvents().Where(S => uniqueNames.ContainsKey(S.Key)).Select(S => S.Value).ToArray();
                if (signals != null && signals.Length > 0) signals.ForEach(S => options.Template(output, S.OfType<SignalEventBase>().Select(subS => isElevatedScript ? new SignalEventElevated(root.GetCurrentPlayfield(), subS) : (object)new SignalEvent(root.GetCurrentPlayfield(), subS)).Reverse().ToArray()));
                else                                       options.Inverse(output, context as object);
            }
            catch (Exception error)
            {
                output.Write("{{signalevents}} error " + EmpyrionScripting.ErrorFilter(error));
            }
        }

        [HandlebarTag("triggerifsignalgoes")]
        public static void TriggerIfSignalGoesHelper(TextWriter output, object rootObject, HelperOptions options, dynamic context, object[] arguments)
        {
            if (arguments.Length != 2) throw new HandlebarsException("{{triggerifsignalgoes names state}} helper must have exactly two arguments: (name1,name2,...) (boolstate)");

            var root                = rootObject as IScriptRootData;
            var isElevatedScript    = rootObject is ScriptSaveGameRootData || root.E.GetCurrent().Faction.Group == FactionGroup.Admin;
            var namesSearch         = arguments[0].ToString();

            try
            {
                bool.TryParse(arguments[1]?.ToString(), out var searchState);
                var uniqueNames = root.SignalEventStore.GetEvents().Keys.GetUniqueNames(namesSearch).ToDictionary(N => N);

                var signals = root.SignalEventStore.GetEvents()
                    .Where(S => uniqueNames.ContainsKey(S.Key))
                    .Select(S => S.Value)
                    .ToArray();

                if (signals != null && signals.Length > 1)
                {
                    ISignalEventBase found = signals
                        .Where(S => {
                            var currentSignal = S[S.Count - 1];

                            if (currentSignal.State == searchState && 
                               (!root.GetPersistendData().TryGetValue(root.ScriptId + currentSignal.Name, out object lastState) ||
                                (bool)lastState != currentSignal.State)){
                                    root.GetPersistendData().AddOrUpdate(root.ScriptId + currentSignal.Name, searchState, (k, v) => searchState);
                                    return true;
                            }
                            return false;
                            })
                        .Select(S => S[S.Count - 1])
                        .FirstOrDefault();


                    if (found != null) options.Template(output, 
                        isElevatedScript 
                        ? new SignalEventElevated(root.GetCurrentPlayfield(), (SignalEventBase)found) 
                        : (object)new SignalEvent(root.GetCurrentPlayfield(), (SignalEventBase)found)
                        );
                }
                else options.Inverse(output, context as object);
            }
            catch (Exception error)
            {
                output.Write("{{triggerifsignalgoes}} error " + EmpyrionScripting.ErrorFilter(error));
            }
        }

        [HandlebarTag("signals")]
        public static void SignalsHelper(TextWriter output, object root, HelperOptions options, dynamic context, object[] arguments)
        {
            if (arguments.Length != 2) throw new HandlebarsException("{{signals structure names}} helper must have exactly two argument: (structure) (name1;name2...)");

            var structure   = arguments[0] as IStructureData;
            var namesSearch = arguments[1].ToString();

            try
            {
                var uniqueNames = structure.ControlPanelSignals.Select(S => S.Name).GetUniqueNames(namesSearch).ToDictionary(N => N);

                var signals = new List<ISignalData>();

                signals.AddRange(structure.ControlPanelSignals.Where(S => uniqueNames.ContainsKey(S.Name)));
                signals.AddRange(structure.BlockSignals       .Where(S => uniqueNames.ContainsKey(S.Name)));

                if (signals != null && signals.Count > 0) options.Template(output, signals.OrderBy(S => S.Name).ToArray());
                else                                      options.Inverse (output, context as object);
            }
            catch (Exception error)
            {
                output.Write("{{signals}} error " + EmpyrionScripting.ErrorFilter(error));
            }
        }

        [HandlebarTag("getsignal")]
        public static void GetSignalHelper(TextWriter output, object root, HelperOptions options, dynamic context, object[] arguments)
        {
            if (arguments.Length != 2) throw new HandlebarsException("{{getsignal structure name}} helper must have exactly two argument: (structure) name)");

            var structure   = arguments[0] as IStructureData;
            var namesSearch = arguments[1].ToString();

            try
            {
                var signal = structure.GetCurrent().GetSignalState(namesSearch);
                options.Template(output, signal);
            }
            catch (Exception error)
            {
                output.Write("{{getsignal}} error " + EmpyrionScripting.ErrorFilter(error));
            }
        }

        [HandlebarTag("setswitch")]
        public static void SetSwitchHelper(TextWriter output, object root, dynamic context, object[] arguments)
        {
            if (arguments.Length != 3) throw new HandlebarsException("{{setswitch structure name state}} helper must have exactly three argument: (structure) (name) (state)");

            var structure   = arguments[0] as IStructureData;
            var namesSearch = arguments[1].ToString();

            try
            {
                bool.TryParse(arguments[2]?.ToString(), out var state);

                var uniqueSignalNames = structure.BlockSignals.Select(S => S.Name).GetUniqueNames(namesSearch).ToDictionary(N => N);

                var signals = structure.BlockSignals
                    .Where(S => uniqueSignalNames.ContainsKey(S.Name) && S.BlockPos.HasValue)
                    .Select(S => new BlockData(structure.GetCurrent(), S.BlockPos.Value))
                    .ToList();

                var uniqueNames = structure.AllCustomDeviceNames.GetUniqueNames(namesSearch);
                signals.AddRange(uniqueNames
                    .SelectMany(N => structure.GetCurrent().GetDevicePositions(N))
                    .Select(P => new BlockData(structure.GetCurrent(), P))
                    );

                signals.ForEach(S => S.SwitchState = state);
            }
            catch (Exception error)
            {
                output.Write("{{setswitch}} error " + EmpyrionScripting.ErrorFilter(error));
            }
        }

        [HandlebarTag("getswitch")]
        public static void GetSwitchHelper(TextWriter output, object root, HelperOptions options, dynamic context, object[] arguments)
        {
            if (arguments.Length != 2) throw new HandlebarsException("{{getswitch structure name}} helper must have exactly two argument: (structure) (name)");

            var structure   = arguments[0] as IStructureData;
            var namesSearch = arguments[1].ToString();

            try
            {
                var uniqueSignalNames = structure.BlockSignals.Select(S => S.Name).GetUniqueNames(namesSearch).ToDictionary(N => N);

                var signals = structure.BlockSignals
                    .Where(S => uniqueSignalNames.ContainsKey(S.Name) && S.BlockPos.HasValue)
                    .Select(S => new BlockData(structure.GetCurrent(), S.BlockPos.Value))
                    .ToList();

                var uniqueNames = structure.AllCustomDeviceNames.GetUniqueNames(namesSearch);
                signals.AddRange(uniqueNames
                    .SelectMany(N => structure.GetCurrent().GetDevicePositions(N))
                    .Select(P => new BlockData(structure.GetCurrent(), P))
                    );

                if (signals.Any()) options.Template(output, signals.First());
                else               options.Inverse(output, context as object);
            }
            catch (Exception error)
            {
                output.Write("{{getswitch}} error " + EmpyrionScripting.ErrorFilter(error));
            }
        }

        [HandlebarTag("getswitches")]
        public static void GetSwitchesHelper(TextWriter output, object root, HelperOptions options, dynamic context, object[] arguments)
        {
            if (arguments.Length != 2) throw new HandlebarsException("{{getswitches structure name}} helper must have exactly two argument: (structure) (name1;name2;...)");

            var structure = arguments[0] as IStructureData;
            var namesSearch = arguments[1].ToString();

            try
            {
                var uniqueSignalNames = structure.BlockSignals.Select(S => S.Name).GetUniqueNames(namesSearch).ToDictionary(N => N);

                var signals = structure.BlockSignals
                    .Where(S => uniqueSignalNames.ContainsKey(S.Name) && S.BlockPos.HasValue)
                    .Select(S => new BlockData(structure.GetCurrent(), S.BlockPos.Value))
                    .ToList();

                var uniqueNames = structure.AllCustomDeviceNames.GetUniqueNames(namesSearch);
                signals.AddRange(uniqueNames
                    .SelectMany(N => structure.GetCurrent().GetDevicePositions(N))
                    .Select(P => new BlockData(structure.GetCurrent(), P))
                    );

                if (signals.Any()) options.Template(output, signals);
                else               options.Inverse(output, context as object);
            }
            catch (Exception error)
            {
                output.Write("{{getswitches}} error " + EmpyrionScripting.ErrorFilter(error));
            }
        }

        public class StopWatchData
        {
            public SignalEventBase Start { get; set; }
            public SignalEventBase Stop { get; set; }
            public TimeSpan TimeTaken { get; set; }
            public TimeSpan? BestTimeTaken { get; set; }
        }

        public class StopWatchRankingData
        {
            public int Pos { get; set; }
            public string Name { get; set; }
            public SignalEventBase Start { get; set; }
            public SignalEventBase Stop { get; set; }
            public TimeSpan TimeTaken { get; set; }
            public TimeSpan? BestTimeTaken { get; set; }
        }

        [HandlebarTag("stopwatch")]
        public static void StopWatchHelper(TextWriter output, object rootObject, HelperOptions options, dynamic context, object[] arguments)
        {
            if (arguments.Length != 2 && arguments.Length != 3) throw new HandlebarsException("{{stopwatch startsignal stopsignal [resetsignal]}} helper must have at least three argument: startsignal stopsignal [resetsignal]");

            var root                = rootObject as IScriptRootData;
            var isElevatedScript    = rootObject is ScriptSaveGameRootData || root.E.GetCurrent().Faction.Group == FactionGroup.Admin;
            var startSignal         = arguments[0].ToString();
            var stopSignal          = arguments[1].ToString();
            var resetSignal         = arguments.Length == 3 ? arguments[2].ToString() : null;

            try
            {
                var stopWatchData = root.GetPersistendData().GetOrAdd($"ST{root.ScriptId}", _ => new ConcurrentDictionary<int, StopWatchData>()) as ConcurrentDictionary<int, StopWatchData>;

                root.SignalEventStore.GetEvents().Keys
                    .GetUniqueNames(startSignal)
                    .ForEach(N => {
                        if (root.SignalEventStore.GetEvents().TryGetValue(N, out var start) && start.Count > 0)
                        {
                            start
                                .Where(S => S.State)
                                .OfType<SignalEventBase>()
                                .ForEach(S => stopWatchData.AddOrUpdate(S.TriggeredByEntityId, 
                                _ => new StopWatchData() {   Start = isElevatedScript ? new SignalEventElevated(root.GetCurrentPlayfield(), S) : (SignalEventBase)new SignalEvent(root.GetCurrentPlayfield(), S) }, 
                                (_, s) =>                { s.Start = isElevatedScript ? new SignalEventElevated(root.GetCurrentPlayfield(), S) : (SignalEventBase)new SignalEvent(root.GetCurrentPlayfield(), S); s.Stop = null; return s; }));
                            start.Clear();
                        }
                    });

                root.SignalEventStore.GetEvents().Keys
                    .GetUniqueNames(stopSignal)
                    .ForEach(N =>
                    {
                        if (root.SignalEventStore.GetEvents().TryGetValue(N, out var stop) && stop.Count > 0)
                        {
                            stop
                                .Where(S => S.State)
                                .OfType<SignalEventBase>()
                                .ForEach(S => stopWatchData.AddOrUpdate(S.TriggeredByEntityId, 
                                _ => new StopWatchData() {   Stop = isElevatedScript ? new SignalEventElevated(root.GetCurrentPlayfield(), S) : (SignalEventBase)new SignalEvent(root.GetCurrentPlayfield(), S) }, 
                                (_, s) => {
                                    s.Stop          = isElevatedScript ? new SignalEventElevated(root.GetCurrentPlayfield(), S) : (SignalEventBase)new SignalEvent(root.GetCurrentPlayfield(), S);
                                    if(s.Start != null) { 
                                        s.TimeTaken     = s.Stop.TimeStamp - s.Start.TimeStamp;
                                        s.BestTimeTaken = !s.BestTimeTaken.HasValue || s.BestTimeTaken > s.TimeTaken ? s.TimeTaken : s.BestTimeTaken;
                                    }
                                    return s;
                                }));
                            stop.Clear();
                        }
                    });

                if (resetSignal != null)
                {
                    root.SignalEventStore.GetEvents().Keys
                        .GetUniqueNames(resetSignal)
                        .ForEach(N =>
                        {
                            if (root.SignalEventStore.GetEvents().TryGetValue(N, out var reset) && reset.Count > 0)
                            {
                                if (reset.Any(S => S.State)) stopWatchData.Clear();
                                reset.Clear();
                            }
                        });
                }

                var pos = 1;
                var ranking = stopWatchData.Values
                    .Where(S => S.Start != null)
                    .Select(S => new StopWatchRankingData() {
                        Name            = S.Start == null || !root.GetCurrentPlayfield().Entities.TryGetValue(S.Start.TriggeredByEntityId, out var entity) ? null : entity.Name,
                        Start           = S.Start,
                        Stop            = S.Stop,
                        TimeTaken       = (S.Stop == null ? DateTime.Now : S.Stop.TimeStamp) - S.Start.TimeStamp,
                        BestTimeTaken   = S.BestTimeTaken,
                    })
                    .Where(S => S.TimeTaken.TotalMilliseconds > 0)
                    .OrderBy(S => S.TimeTaken)
                    .Select(S => { S.Pos = pos++; return S; })
                    .ToArray();

                if(ranking.Length > 0) ranking.ForEach(S => options.Template(output, S));
                else                   options.Inverse(output, context as object);
            }
            catch (Exception error)
            {
                output.Write("{{stopwatch}} error " + EmpyrionScripting.ErrorFilter(error));
            }
        }

    }
}
