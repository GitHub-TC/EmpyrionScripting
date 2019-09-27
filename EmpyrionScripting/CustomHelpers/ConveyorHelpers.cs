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
            VectorInt3 position;

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

        [HandlebarTag("lockdevice")]
        public static void LockDeviceHelper(TextWriter output, HelperOptions options, dynamic context, object[] arguments)
        {
            if (arguments.Length != 3 && arguments.Length != 5) throw new HandlebarsException("{{lockdevice @root structure device|x y z}} helper must have three or five argument: @root (structure) (device)|(x) (y) (z)");

            var root                = arguments[0] as IScriptRootData;
            var isElevatedScript    = arguments[0] is ScriptSaveGameRootData || root.E.GetCurrent().Faction.Group == FactionGroup.Admin;
            var S                   = arguments[1] as IStructureData;
            VectorInt3 position;

            if(!isElevatedScript) throw new HandlebarsException("{{lockdevice}} only allowed in elevated scripts");

            if (arguments.Length == 3)
            {
                var block = arguments[2] as BlockData;
                position  = block?.Position ?? new VectorInt3();
            }
            else
            {
                int.TryParse(arguments[2].ToString(), out var x);
                int.TryParse(arguments[3].ToString(), out var y);
                int.TryParse(arguments[4].ToString(), out var z);

                position = new VectorInt3(x, y, z);
            }

            try
            {
                using (var locked = CreateDeviceLock(EmpyrionScripting.ModApi?.Playfield, S.E?.S.GetCurrent(), position))
                {
                    if (locked.Success) options.Template(output, context as object);
                    else                options.Inverse (output, context as object);
                }
            }
            catch (Exception error)
            {
                output.Write("{{lockdevice}} error " + EmpyrionScripting.ErrorFilter(error));
            }
        }

        [HandlebarTag("additems")]
        public static void AddItemsHelper(TextWriter output, dynamic context, object[] arguments)
        {
            if (arguments.Length != 4) throw new HandlebarsException("{{additems @root container itemid count}} helper must have four arguments: @root (container) (item) (count)");

            var root                = arguments[0] as IScriptRootData;
            var isElevatedScript    = arguments[0] is ScriptSaveGameRootData || root.E.GetCurrent().Faction.Group == FactionGroup.Admin;
            var block               = arguments[1] as BlockData;
            int.TryParse(arguments[2].ToString(), out var itemid);
            int.TryParse(arguments[3].ToString(), out var count);

            if (!isElevatedScript) throw new HandlebarsException("{{additems}} only allowed in elevated scripts");

            try
            {
                var container = block.Device as ContainerData;
                container.GetContainer().AddItems(itemid, count);
            }
            catch (Exception error)
            {
                output.Write("{{additems}} error " + EmpyrionScripting.ErrorFilter(error));
            }
        }

        [HandlebarTag("removeitems")]
        public static void RemoveItemsHelper(TextWriter output, dynamic context, object[] arguments)
        {
            if (arguments.Length != 4) throw new HandlebarsException("{{removeitems @root container itemid maxcount}} helper must have four arguments: @root (container) (item) (maxcount)");

            var root = arguments[0] as IScriptRootData;
            var isElevatedScript = arguments[0] is ScriptSaveGameRootData || root.E.GetCurrent().Faction.Group == FactionGroup.Admin;
            var block = arguments[1] as BlockData;
            int.TryParse(arguments[2].ToString(), out var itemid);
            int.TryParse(arguments[3].ToString(), out var maxcount);

            if (!isElevatedScript) throw new HandlebarsException("{{removeitems}} only allowed in elevated scripts");

            try
            {
                var container = block.Device as ContainerData;
                container.GetContainer().RemoveItems(itemid, maxcount);
            }
            catch (Exception error)
            {
                output.Write("{{removeitems}} error " + EmpyrionScripting.ErrorFilter(error));
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

        [HandlebarTag("fill")]
        public static void FillHelper(TextWriter output, HelperOptions options, dynamic context, object[] arguments)
        {
            if (arguments.Length < 3) throw new HandlebarsException("{{fill item structure tank [max]}} helper must have at least two argument: (item) (structure) (tank) [max count/percentage targets]");

            try
            {
                var structure   = arguments[1] as IStructureData;
                var item        = arguments[0] as ItemsData;

                if (!Enum.TryParse<StructureTankType>(arguments[2]?.ToString(), true, out var type))
                {
                    output.WriteLine($"unknown type {arguments[2]}");
                    return;
                }

                int maxLimit = arguments.Length > 3 && int.TryParse(arguments[3]?.ToString(), out int limit) ? Math.Min(100, Math.Max(0, limit)) : 100;

                if (ScriptExecQueue.Iteration % EmpyrionScripting.Configuration.Current.DeviceLockOnlyAllowedEveryXCycles != 0)
                {
                    Log($"NoLockAllowed: {ScriptExecQueue.Iteration} % {EmpyrionScripting.Configuration.Current.DeviceLockOnlyAllowedEveryXCycles}", LogLevel.Debug);
                    return;
                }

                IStructureTankWrapper specialTransfer = null;
                switch (type)
                {
                    case StructureTankType.Oxygen    : specialTransfer = structure.OxygenTank    ; break;
                    case StructureTankType.Fuel      : specialTransfer = structure.FuelTank      ; break;
                    case StructureTankType.Pentaxid  : specialTransfer = structure.PentaxidTank  ; break;
                }

                if (!specialTransfer.AllowedItem(item.Id))
                {
                    options.Inverse(output, context as object);
                    return;
                }

                var moveInfos = new List<ItemMoveInfo>();

                lock (movelock)
                {
                    item.Source
                        .ForEach(S => {
                            using (var locked = CreateDeviceLock(EmpyrionScripting.ModApi?.Playfield, S.E?.S.GetCurrent(), S.Position))
                            {
                                if (!locked.Success)
                                {
                                    Log($"DeviceIsLocked (Source): {S.Id} #{S.Count} => {S.CustomName}", LogLevel.Debug);
                                    return;
                                }

                                var count = specialTransfer.ItemsNeededForFill(S.Id, maxLimit);
                                if (count > 0)
                                {
                                    count -= S.Container.RemoveItems(S.Id, count);
                                    Log($"Move(RemoveItems): {S.CustomName} {S.Id} #{S.Count}->{count}", LogLevel.Debug);
                                }

                                if (count > 0)
                                {
                                    var startCount = count;
                                    count = specialTransfer.AddItems(S.Id, count);
                                    if (startCount != count) moveInfos.Add(new ItemMoveInfo()
                                    {
                                        Id              = S.Id,
                                        Count           = startCount - count,
                                        SourceE         = S.E,
                                        Source          = S.CustomName,
                                        DestinationE    = structure.E,
                                        Destination     = type.ToString(),
                                    });
                                };

                                if (count > 0) count = S.Container.AddItems(S.Id, count);
                                if (count > 0) output.Write($"{{fill}} error lost #{count} of item {S.Id} in container {S.CustomName}");
                            }
                        });

                    Log($"Fill Total: #{item.Source.Count}", LogLevel.Debug);
                }

                if (moveInfos.Count == 0) options.Inverse(output, context as object);
                else                      moveInfos.ForEach(I => options.Template(output, I));
            }
            catch (Exception error)
            {
                output.Write("{{fill}} error " + EmpyrionScripting.ErrorFilter(error));
            }
        }


        public class DeconstructData
        {
            public int Id { get; set; }
            public int X { get; set; }
            public int Y { get; set; }
            public int Z { get; set; }
            public int TotalBlocks { get; set; }
            public int CheckedBlocks { get; set; }
            public int RemovedBlocks { get; set; }
            public VectorInt3 MinPos { get; set; }
            public VectorInt3 MaxPos { get; set; }
        }

        [HandlebarTag("deconstruct")]
        public static void DeconstructHelper(TextWriter output, HelperOptions options, dynamic context, object[] arguments)
        {
            if (arguments.Length < 3) throw new HandlebarsException("{{deconstruct @root entity container}} helper must have three argument: @root entity container");

            var root = arguments[0] as IScriptRootData;
            var E    = arguments[1] as IEntityData;
            var N    = arguments[2]?.ToString();

            try
            {
                var minPos      = E.S.GetCurrent().MinPos;
                var maxPos      = E.S.GetCurrent().MaxPos;
                var S           = E.S.GetCurrent();
                var coreName    = $"Core-Destruct-{E.Id}";
                var corePosList = E.S.GetCurrent().GetDevicePositions(coreName);

                var target      = root.E.S.GetCurrent().GetDevice<Eleon.Modding.IContainer>(N);
                if (target == null)
                {
                    root.GetPersistendData().TryRemove(root.ScriptId, out _);
                    options.Inverse(output, context as object);
                    output.WriteLine($"No target container '{N}' found");
                    return;
                }
                var targetPos = root.E.S.GetCurrent().GetDevicePositions(N).First();

                if(corePosList.Count == 0)
                {
                    root.GetPersistendData().TryRemove(root.ScriptId, out _);
                    options.Inverse(output, context as object);
                    output.WriteLine($"No core '{coreName}' found on {E.Id}");
                    return;
                }

                var corePos = corePosList.First();
                var core = E.S.GetCurrent().GetBlock(corePos);
                core.Get(out var coreBlockType, out _ , out _, out _);

                if (coreBlockType != PlayerCoreType)
                {
                    root.GetPersistendData().TryRemove(root.ScriptId, out _);
                    options.Inverse(output, context as object);
                    output.WriteLine($"No core '{coreName}' found on {E.Id} wrong type {coreBlockType}");
                    return;
                }

                var deconstructData = root.GetPersistendData().GetOrAdd(root.ScriptId, K => new DeconstructData() {
                    Id          = E.Id,
                    MinPos      = minPos,
                    MaxPos      = maxPos,
                    X           = minPos.x,
                    Y           = maxPos.y,
                    Z           = minPos.z,
                    TotalBlocks =   (Math.Abs(minPos.x) + Math.Abs(maxPos.x) + 1) *
                                    (Math.Abs(minPos.y) + Math.Abs(maxPos.y) + 1) *
                                    (Math.Abs(minPos.z) + Math.Abs(maxPos.z) + 1)
                }) as DeconstructData;

                if(deconstructData.Id != E.Id)                                       root.GetPersistendData().TryRemove(root.ScriptId, out _);
                else if(deconstructData.CheckedBlocks < deconstructData.TotalBlocks) lock(deconstructData) DeconstructPart(output, root, S, deconstructData, target, targetPos, N);

                options.Template(output, deconstructData);
            }
            catch (Exception error)
            {
                output.Write("{{deconstruct}} error " + EmpyrionScripting.ErrorFilter(error));
            }
        }

        static void DeconstructPart(TextWriter output, IScriptRootData root, IStructure S, DeconstructData deconstructData, IContainer target, VectorInt3 targetPos, string N)
        {
            IDeviceLock locked = null;

            var startTime = DateTime.Now;
            var maxMilliSeconds = EmpyrionScripting.Configuration.Current.InGameScriptsIntervallMS;

            try
            {
                for (; deconstructData.Y >= deconstructData.MinPos.y; deconstructData.Y--)
                {
                    for (; deconstructData.X <= deconstructData.MaxPos.x; deconstructData.X++)
                    {
                        for (; deconstructData.Z <= deconstructData.MaxPos.z; deconstructData.Z++)
                        {
                            deconstructData.CheckedBlocks++;

                            var block = S.GetBlock(deconstructData.X, 128 + deconstructData.Y, deconstructData.Z);
                            if (block != null)
                            {
                                block.Get(out var blockType, out _, out _, out _);
                                if (blockType > 0 && blockType != PlayerCoreType)
                                {
                                    locked = locked ?? CreateDeviceLock(EmpyrionScripting.ModApi?.Playfield, root.E.S.GetCurrent(), targetPos);
                                    if (!locked.Success)
                                    {
                                        deconstructData.CheckedBlocks--;
                                        output.WriteLine($"Container '{N}' is locked");
                                        return;
                                    }

                                    if (target.AddItems(blockType, 1) > 0)
                                    {
                                        deconstructData.CheckedBlocks--;
                                        output.WriteLine($"Container '{N}' is full");
                                        return;
                                    }
                                    block.Set(0);
                                    deconstructData.RemovedBlocks++;

                                    if (deconstructData.RemovedBlocks > 100 && deconstructData.RemovedBlocks % 100 == 0 && (DateTime.Now - startTime).TotalMilliseconds > maxMilliSeconds) return;
                                }
                            }
                        }
                        deconstructData.Z = deconstructData.MinPos.z;
                    }
                    deconstructData.X = deconstructData.MinPos.x;
                }
            }
            finally
            {
                locked?.Dispose();
            }
        }

    }
}
