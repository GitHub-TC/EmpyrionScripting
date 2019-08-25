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
        private const int PlayerCoreType = 558;
        private static object movelock = new object();

        public static Action<string, LogLevel> Log { get; set; }
        public static Func<IPlayfield, IStructure, VectorInt3, IDeviceLock> CreateDeviceLock { get; set; } = (P, S, V) => new DeviceLock(P,S,V);

        public class ItemMoveInfo
        {
            public int Id { get; set; }
            public IEntityData SourceE { get; set; }
            public string Source { get; set; }
            public IEntityData DestinationE { get; set; }
            public string Destination { get; set; }
            public int Count { get; set; }
        }

        [HandlebarTag("islocked")]
        public static void IsLockedHelper(TextWriter output, HelperOptions options, dynamic context, object[] arguments)
        {
            if (arguments.Length != 2 && arguments.Length != 4) throw new HandlebarsException("{{islocked structure device|x y z}} helper must have two or four argument: (structure) (device)|(x) (y) (z)");

            var structure = arguments[0] as IStructureData;
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
                var structure   = arguments[1] as IStructureData;
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
                                using(var locked = CreateDeviceLock(EmpyrionScripting.ModApi?.Playfield, S.E?.S.GetCurrent(), S.Position)) {
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

        private static int MoveItem(ItemsSource S, string N, IStructureData targetStructure, int count, int? maxLimit)
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

            using (var locked = CreateDeviceLock(EmpyrionScripting.ModApi?.Playfield, targetStructure.GetCurrent(), targetData.Position))
            {
                if (!locked.Success)
                {
                    Log($"DeviceIsLocked (Target): {S.Id} #{S.Count} => {targetData.CustomName}", LogLevel.Debug);
                    return count;
                }

                if (maxLimit.HasValue)
                {
                    var stock = target.GetTotalItems(S.Id);
                    var transfer = Math.Max(0, Math.Min(count, maxLimit.Value - stock));
                    return target.AddItems(S.Id, transfer) + (count - transfer);
                }
                else return target.AddItems(S.Id, count);
            }
        }

        [HandlebarTag("deconstruct")]
        public static void DeconstructHelper(TextWriter output, dynamic context, object[] arguments)
        {
            if (arguments.Length < 3) throw new HandlebarsException("{{deconstruct @root entity container}} helper must have three argument: @root entity container");

            var root = arguments[0] as IScriptRootData;
            var E    = arguments[1] as IEntityData;
            var N    = arguments[2]?.ToString();
            IDeviceLock locked = null;

            try
            {
                if (ScriptExecQueue.Iteration % EmpyrionScripting.Configuration.Current.DeviceLockOnlyAllowedEveryXCycles != 0)
                {
                    Log($"NoLockAllowed: {ScriptExecQueue.Iteration} % {EmpyrionScripting.Configuration.Current.DeviceLockOnlyAllowedEveryXCycles}", LogLevel.Debug);
                    return;
                }

                var minPos      = E.S.GetCurrent().MinPos;
                var maxPos      = E.S.GetCurrent().MaxPos;
                var S           = E.S.GetCurrent();
                var coreName    = $"Core-Destruct-{E.Id}";
                var corePosList = E.S.GetCurrent().GetDevicePositions(coreName);

                var maxMilliSeconds = EmpyrionScripting.Configuration.Current.InGameScriptsIntervallMS * EmpyrionScripting.Configuration.Current.DeviceLockOnlyAllowedEveryXCycles;

                var target      = root.E.S.GetCurrent().GetDevice<Eleon.Modding.IContainer>(N);
                if (target == null)
                {
                    output.Write($"No target container '{N}' found");
                    return;
                }
                var targetPos = root.E.S.GetCurrent().GetDevicePositions(N).First();

                var removed     = 0;
                var count       = 0;
                var poscount    = 0;

                if(corePosList.Count == 0)
                {
                    output.Write($"No core '{coreName}' found on {E.Id}");
                    return;
                }

                var corePos = corePosList.First();
                var core = E.S.GetCurrent().GetBlock(corePos);
                core.Get(out var coreBlockType, out _ , out _, out _);

                if (coreBlockType != PlayerCoreType)
                {
                    output.Write($"No core '{coreName}' found on {E.Id} wrong type {coreBlockType}");
                    return;
                }

                var startTime = DateTime.Now;
                output.WriteLine($"Deconstruct:{E.Name} Id:{E.Id}\nminPos:{minPos} maxPos:{maxPos}");

                for (int y = maxPos.y; y >= minPos.y; y--)
                {

                    for (int x = minPos.x; x <= maxPos.x; x++)
                    {
                        for (int z = minPos.z; z <= maxPos.z; z++)
                        {
                            var block = S.GetBlock(x, 128 + y, z);
                            if (block != null)
                            {
                                poscount++;
                                block.Get(out var blockType, out _, out _, out _);
                                if (blockType > 0 && blockType != PlayerCoreType)
                                {
                                    locked = locked ?? CreateDeviceLock(EmpyrionScripting.ModApi?.Playfield, root.E.S.GetCurrent(), targetPos);
                                    if (!locked.Success) return;

                                    count++;
                                    if (target.AddItems(blockType, 1) > 0)
                                    {
                                        output.WriteLine($"Container '{N}' is full");
                                        return;
                                    }
                                    block.Set(0);
                                    removed++;

                                    if (removed > 100 && removed % 100 == 0 && (DateTime.Now - startTime).TotalMilliseconds > maxMilliSeconds) return;
                                }
                            }
                        }
                    }
                }

                output.WriteLine($"Removed:{removed} Count:{poscount}/{count}");
            }
            catch (Exception error)
            {
                output.Write("{{deconstruct}} error " + EmpyrionScripting.ErrorFilter(error));
            }
            finally
            {
                locked?.Dispose();
            }
        }



    }
}
