using Eleon.Modding;
using EmpyrionNetAPIDefinitions;
using EmpyrionScripting.DataWrapper;
using HandlebarsDotNet;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace EmpyrionScripting.CustomHelpers
{
    [HandlebarHelpers]
    public static class ConveyorHelpers
    {
        private static object movelock = new object();

        public static Action<string, LogLevel> Log { get; set; }

        public class ItemMoveInfo
        {
            public int Id { get; set; }
            public EntityData SourceE { get; set; }
            public string Source { get; set; }
            public EntityData DestinationE { get; set; }
            public string Destination { get; set; }
            public int Count { get; set; }
        }

        [HandlebarTag("islocked")]
        public static void IsLockedHelper(TextWriter output, HelperOptions options, dynamic context, object[] arguments)
        {
            if (arguments.Length != 2 && arguments.Length != 4) throw new HandlebarsException("{{islocked structure device|x y z}} helper must have two or four argument: (structure) (device)|(x) (y) (z)");

            var structure = arguments[0] as StructureData;
            VectorInt3 position = new VectorInt3();

            if (arguments.Length == 2)
            {
                var block = arguments[1] as BlockData;
                position  = block?.Position ?? new VectorInt3();
            }
            else
            {
                int.TryParse(arguments[1].ToString(), out var x);
                int.TryParse(arguments[2].ToString(), out var y);
                int.TryParse(arguments[3].ToString(), out var z);

                position = new VectorInt3(x, y, z);
            }

            try
            {
                var isLocked = EmpyrionScripting.ModApi.Playfield.IsStructureDeviceLocked(structure.GetCurrent().Id, position);

                if (isLocked) options.Template(output, context as object);
                else          options.Inverse (output, context as object);
            }
            catch (Exception error)
            {
                output.Write("{{islocked}} error " + EmpyrionScripting.ErrorFilter(error));
            }
        }

        [HandlebarTag("move")]
        public static void ItemMoveHelper(TextWriter output, HelperOptions options, dynamic context, object[] arguments)
        {
            if (arguments.Length < 3) throw new HandlebarsException("{{move item structure names [max]}} helper must have at least three argument: (item) (structure) (name;name*;*;name) [max count targets]");

            try
            {
                var item        = arguments[0] as ItemsData;
                var structure   = arguments[1] as StructureData;
                var namesSearch = arguments[2] as string;

                int? maxLimit = arguments.Length > 3 && int.TryParse(arguments[3]?.ToString(), out int limit) ? limit : (int?)null;

                if (ScriptExecQueue.Iteration % EmpyrionScripting.Configuration.Current.DeviceLockOnlyAllowedEveryXCycles != 0)
                {
                    Log($"NoLockAllowed: {ScriptExecQueue.Iteration} % {EmpyrionScripting.Configuration.Current.DeviceLockOnlyAllowedEveryXCycles}", LogLevel.Debug);
                    return;
                }

                var moveInfos = new List<ItemMoveInfo>();

                var uniqueNames = structure.AllCustomDeviceNames.GetUniqueNames(namesSearch);

                lock (movelock) { 
                    if(uniqueNames.Any()){
                        item.Source
                            .ForEach(S => {
                                using(var locked = new DeviceLock(EmpyrionScripting.ModApi.Playfield, S.E.S.GetCurrent(), S.Position)) {
                                    if (!locked.Success)
                                    {
                                        Log($"DeviceIsLocked (Source): {S.Id} #{S.Count} => {S.CustomName}", LogLevel.Debug);
                                        return;
                                    }

                                    var count = S.Count;
                                    count -= S.Container.RemoveItems(S.Id, count);
                                    Log($"Move(RemoveItems): {S.CustomName} {S.Id} #{S.Count}->{count}", LogLevel.Debug);

                                    if (count > 0) uniqueNames
                                                    .Where(N => N != S.CustomName)
                                                    .ForEach(N => {
                                                        var startCount = count;
                                                        count = MoveItem(S, N, structure, count, maxLimit);
                                                        if(startCount != count) moveInfos.Add(new ItemMoveInfo() {
                                                            Id              = S.Id,
                                                            Count           = startCount - count,
                                                            SourceE         = S.E,
                                                            Source          = S.CustomName,
                                                            DestinationE    = structure.E,
                                                            Destination     = N,
                                                        });
                                                    });

                                    if (count > 0) count = S.Container.AddItems(S.Id, count);
                                    if (count > 0) output.Write($"{{move}} error lost #{count} of item {S.Id} in container {S.CustomName}");
                                }
                            });

                            Log($"Move Total: #{item.Source.Count}", LogLevel.Debug);
                        }
                    else
                    {
                        Log($"NoDevicesFound: {namesSearch}", LogLevel.Debug);
                        return;
                    }

                }

                if (moveInfos.Count == 0) options.Inverse (output, context as object);
                else                      moveInfos.ForEach(I => options.Template(output, I));
            }
            catch (Exception error)
            {
                output.Write("{{move}} error " + EmpyrionScripting.ErrorFilter(error));
            }
        }

        private static int MoveItem(ItemsSource S, string N, StructureData targetStructure, int count, int? maxLimit)
        {
            var target = targetStructure?.GetCurrent()?.GetDevice<Eleon.Modding.IContainer>(N);
            if (target == null)
            {
                Log($"TargetNoFound: {S.Id} #{S.Count} => {N}", LogLevel.Debug);
                return count;
            }

            if (!targetStructure.ContainerSource.TryGetValue(N, out var targetData))
            {
                Log($"TargetDataNoFound: {S.Id} #{S.Count} => {N}", LogLevel.Debug);
                return count;
            }

            using (var locked = new DeviceLock(EmpyrionScripting.ModApi.Playfield, targetStructure.GetCurrent(), targetData.Position))
            {
                if (!locked.Success)
                {
                    Log($"DeviceIsLocked (Target): {S.Id} #{S.Count} => {targetData.CustomName}", LogLevel.Debug);
                    return count;
                }

                if (maxLimit.HasValue)
                {
                    var stock = target.GetTotalItems(S.Id);
                    var transfer = Math.Min(count, maxLimit.Value - stock);
                    return target.AddItems(S.Id, transfer) + (count - transfer);
                }
                else return target.AddItems(S.Id, count);
            }
        }

    }
}
