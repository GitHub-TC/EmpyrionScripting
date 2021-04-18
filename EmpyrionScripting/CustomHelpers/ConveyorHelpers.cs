using Eleon.Modding;
using EmpyrionNetAPIDefinitions;
using EmpyrionScripting.CsHelper;
using EmpyrionScripting.DataWrapper;
using EmpyrionScripting.Interface;
using EmpyrionScripting.Internal.Interface;
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
        private static readonly object movelock = new object();

        public static Action<string, LogLevel> Log { get; set; }
        public static Func<IScriptRootData, IPlayfield, IStructure, VectorInt3, IDeviceLock> CreateDeviceLock { get; set; } = (R, P, S, V) => new DeviceLock(R, P,S,V);

        public class ItemMoveInfo : IItemMoveInfo
        {
            public static IList<IItemMoveInfo> Empty = Array.Empty<ItemMoveInfo>();
            public int Id { get; set; }
            public IEntityData SourceE { get; set; }
            public string Source { get; set; }
            public IEntityData DestinationE { get; set; }
            public string Destination { get; set; }
            public int Count { get; set; }
            public string Error { get; set; }
        }

        [HandlebarTag("islocked")]
        public static void IsLockedHelper(TextWriter output, object rootObject, HelperOptions options, dynamic context, object[] arguments)
        {
            if (arguments.Length != 2 && arguments.Length != 4) throw new HandlebarsException("{{islocked structure device|x y z}} helper must have two or four argument: (structure) (device)|(x) (y) (z)");

            var root        = rootObject as IScriptRootData;
            var structure   = arguments[0] as IStructureData;
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
                var isLocked = root.GetCurrentPlayfield().IsStructureDeviceLocked(structure.GetCurrent().Id, position);

                if (isLocked) options.Template(output, context as object);
                else          options.Inverse (output, context as object);
            }
            catch (Exception error)
            {
                if (!CsScriptFunctions.FunctionNeedsMainThread(error, root)) output.Write("{{islocked}} error " + EmpyrionScripting.ErrorFilter(error));
            }
        }

        [HandlebarTag("lockdevice")]
        public static void LockDeviceHelper(TextWriter output, object rootObject, HelperOptions options, dynamic context, object[] arguments)
        {
            if (arguments.Length != 2 && arguments.Length != 4) throw new HandlebarsException("{{lockdevice structure device|x y z}} helper must have two or four argument: @root (structure) (device)|(x) (y) (z)");

            var root                = rootObject as IScriptRootData;
            var S                   = arguments[0] as IStructureData;
            VectorInt3 position;

            if(!root.IsElevatedScript) throw new HandlebarsException("{{lockdevice}} only allowed in elevated scripts");

            if (!root.DeviceLockAllowed)
            {
                Log($"NoLockAllowed: {root.CycleCounter} % {EmpyrionScripting.Configuration.Current.DeviceLockOnlyAllowedEveryXCycles}", LogLevel.Debug);
                return;
            }

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
                using (var locked = CreateDeviceLock(root, root.GetCurrentPlayfield(), S.E?.S.GetCurrent(), position))
                {
                    if (locked.Success) options.Template(output, context as object);
                    else                options.Inverse (output, context as object);
                }
            }
            catch (Exception error)
            {
                if (!CsScriptFunctions.FunctionNeedsMainThread(error, root)) output.Write("{{lockdevice}} error " + EmpyrionScripting.ErrorFilter(error));
            }
        }

        [HandlebarTag("additems")]
        public static void AddItemsHelper(TextWriter output, object rootObject, dynamic context, object[] arguments)
        {
            if (arguments.Length != 3) throw new HandlebarsException("{{additems container itemid count}} helper must have three arguments: (container) (item) (count)");

            var root                = rootObject as IScriptRootData;
            var block               = arguments[0] as BlockData;
            int.TryParse(arguments[1].ToString(), out var itemid);
            int.TryParse(arguments[2].ToString(), out var count);

            if (!root.IsElevatedScript) throw new HandlebarsException("{{additems}} only allowed in elevated scripts");

            try
            {
                var container = block.Device as ContainerData;
                container.GetContainer().AddItems(itemid, count);
            }
            catch (Exception error)
            {
                if (!CsScriptFunctions.FunctionNeedsMainThread(error, root)) output.Write("{{additems}} error " + EmpyrionScripting.ErrorFilter(error));
            }
        }

        [HandlebarTag("removeitems")]
        public static void RemoveItemsHelper(TextWriter output, object rootObject, dynamic context, object[] arguments)
        {
            if (arguments.Length != 3) throw new HandlebarsException("{{removeitems container itemid maxcount}} helper must have three arguments: (container) (item) (maxcount)");

            var root                = rootObject as IScriptRootData;
            var block               = arguments[0] as BlockData;
            int.TryParse(arguments[1].ToString(), out var itemid);
            int.TryParse(arguments[2].ToString(), out var maxcount);

            if (!root.IsElevatedScript) throw new HandlebarsException("{{removeitems}} only allowed in elevated scripts");

            try
            {
                var container = block.Device as ContainerData;
                container.GetContainer().RemoveItems(itemid, maxcount);
            }
            catch (Exception error)
            {
                if (!CsScriptFunctions.FunctionNeedsMainThread(error, root)) output.Write("{{removeitems}} error " + EmpyrionScripting.ErrorFilter(error));
            }
        }

        [HandlebarTag("move")]
        public static void ItemMoveHelper(TextWriter output, object rootObject, HelperOptions options, dynamic context, object[] arguments)
        {
            if (arguments.Length < 3 || arguments.Length > 4) throw new HandlebarsException("{{move item structure names [max]}} helper must have at least three argument: (item) (structure) (name;name*;*;name) [max count targets]");

            var root = rootObject as IScriptRootData;
            try
            {
                var item        = arguments[0] as ItemsData;
                var structure   = arguments[1] as IStructureData;
                var namesSearch = arguments[2] as string;

                int? maxLimit = arguments.Length > 3 && int.TryParse(arguments[3]?.ToString(), out int limit) ? limit : (int?)null;

                var moveInfos = Move(root, item, structure, namesSearch, maxLimit);

                moveInfos
                    .Where(M => M.Error != null)
                    .ForEach(E => output.Write(E));

                if (moveInfos.Count == 0) options.Inverse (output, context as object);
                else                      moveInfos.ForEach(I => options.Template(output, I));
            }
            catch (Exception error)
            {
                if (!CsScriptFunctions.FunctionNeedsMainThread(error, root)) output.Write("{{move}} error " + EmpyrionScripting.ErrorFilter(error));
            }
        }

        public static IList<IItemMoveInfo> Move(IScriptRootData root, IItemsData item, IStructureData structure, string namesSearch, int? maxLimit)
        {
            if (!root.DeviceLockAllowed)
            {
                Log($"NoLockAllowed: {root.CycleCounter} % {EmpyrionScripting.Configuration.Current.DeviceLockOnlyAllowedEveryXCycles}", LogLevel.Debug);
                return ItemMoveInfo.Empty;
            }

            var moveInfos = new List<IItemMoveInfo>();

            var uniqueNames = structure.AllCustomDeviceNames.GetUniqueNames(namesSearch);

            lock (movelock) { 
                if(uniqueNames.Any()){
                    item.Source
                        .ForEach(S => {
                            using(var locked = CreateDeviceLock(root, root.GetCurrentPlayfield(), S.E?.S.GetCurrent(), S.Position)) {
                                if (!locked.Success)
                                {
                                    Log($"DeviceIsLocked (Source): {S.Id} #{S.Count} => {S.CustomName}", LogLevel.Debug);
                                    return;
                                }

                                var count = S.Count;
                                count -= S.Container.RemoveItems(S.Id, count);
                                Log($"Move(RemoveItems): {S.CustomName} {S.Id} #{S.Count}->{count}", LogLevel.Debug);

                                ItemMoveInfo currentMoveInfo = null;

                                if (count > 0) uniqueNames
                                                .Where(N => N != S.CustomName)
                                                .ForEach(N => {
                                                    if (root.ScriptLoopTimeLimitReached()) return;

                                                    var startCount = count;
                                                    count = MoveItem(root, S, N, structure, count, maxLimit);
                                                    if(startCount != count) moveInfos.Add(currentMoveInfo = new ItemMoveInfo() {
                                                        Id              = S.Id,
                                                        Count           = startCount - count,
                                                        SourceE         = S.E,
                                                        Source          = S.CustomName,
                                                        DestinationE    = structure.E,
                                                        Destination     = N,
                                                    });
                                                });

                                if (count > 0) count = S.Container.AddItems(S.Id, count);
                                if (count > 0 && currentMoveInfo != null) currentMoveInfo.Error = $"{{move}} error lost #{count} of item {S.Id} in container {S.CustomName}";
                            }
                        });

                        Log($"Move Total: #{item.Source.Count}", LogLevel.Debug);
                    }
                else
                {
                    Log($"NoDevicesFound: {namesSearch}", LogLevel.Debug);
                    return ItemMoveInfo.Empty;
                }

            }

            return moveInfos;
        }

        private static int MoveItem(IScriptRootData root, IItemsSource S, string N, IStructureData targetStructure, int count, int? maxLimit)
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

            using (var locked = CreateDeviceLock(root, root.GetCurrentPlayfield(), targetStructure.GetCurrent(), targetData.Position))
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
        public static void FillHelper(TextWriter output, object rootObject, HelperOptions options, dynamic context, object[] arguments)
        {
            if (arguments.Length < 3 || arguments.Length > 4) throw new HandlebarsException("{{fill item structure tank [max]}} helper must have at least three argument: (item) (structure) (tank) [max count/percentage targets]");

            var root = rootObject as IScriptRootData;
            try
            {
                if (!(arguments[1] is IStructureData structure) || !(arguments[0] is ItemsData item)) return;

                if (!Enum.TryParse<StructureTankType>(arguments[2]?.ToString(), true, out var type))
                {
                    output.WriteLine($"unknown type {arguments[2]}");
                    return;
                }

                int maxLimit = arguments.Length > 3 && int.TryParse(arguments[3]?.ToString(), out int limit) ? Math.Min(100, Math.Max(0, limit)) : 100;

                var moveInfos = Fill(root, item, structure, type, maxLimit);

                moveInfos
                    .Where(M => M.Error != null)
                    .ForEach(E => output.Write(E));

                if (moveInfos.Count == 0) options.Inverse(output, context as object);
                else                      moveInfos.ForEach(I => options.Template(output, I));
            }
            catch (Exception error)
            {
                if (!CsScriptFunctions.FunctionNeedsMainThread(error, root)) output.Write("{{fill}} error " + EmpyrionScripting.ErrorFilter(error));
            }
        }

        public static IList<IItemMoveInfo> Fill(IScriptRootData root, IItemsData item, IStructureData structure, StructureTankType type, int maxLimit)
        {
            if (!root.DeviceLockAllowed)
            {
                Log($"NoLockAllowed: {root.CycleCounter} % {EmpyrionScripting.Configuration.Current.DeviceLockOnlyAllowedEveryXCycles}", LogLevel.Debug);
                return ItemMoveInfo.Empty;
            }

            IStructureTankWrapper specialTransfer = null;
            switch (type)
            {
                case StructureTankType.Oxygen    : specialTransfer = structure.OxygenTank    ; break;
                case StructureTankType.Fuel      : specialTransfer = structure.FuelTank      ; break;
                case StructureTankType.Pentaxid  : specialTransfer = structure.PentaxidTank  ; break;
            }

            if (specialTransfer == null || !specialTransfer.AllowedItem(item.Id))
            {
                return ItemMoveInfo.Empty;
            }

            var moveInfos = new List<IItemMoveInfo>();

            lock (movelock)
            {
                item.Source
                    .ForEach(S => {
                        using (var locked = CreateDeviceLock(root, root.GetCurrentPlayfield(), S.E?.S.GetCurrent(), S.Position))
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

                            ItemMoveInfo currentMoveInfo = null;

                            if (count > 0)
                            {
                                var startCount = count;
                                count = specialTransfer.AddItems(S.Id, count);
                                if (startCount != count) moveInfos.Add(currentMoveInfo = new ItemMoveInfo()
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
                            if (count > 0 && currentMoveInfo != null) currentMoveInfo.Error = $"{{fill}} error lost #{count} of item {S.Id} in container {S.CustomName}";
                        }
                    });

                Log($"Fill Total: #{item.Source.Count}", LogLevel.Debug);
            }

            return moveInfos;
        }

        public class ProcessBlockData
        {
            public DateTime Started { get; set; }
            public DateTime Finished { get; set; }
            public string Name { get; set; }
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
        public static void DeconstructHelper(TextWriter output, object rootObject, HelperOptions options, dynamic context, object[] arguments)
        {
            if (arguments.Length < 2 || arguments.Length > 4) throw new HandlebarsException("{{deconstruct entity container [CorePrefix] [RemoveItemsIds1,Id2,...]}} helper must have two to four argument: entity container [CorePrefix] [RemoveItemsIds]");

            var root = rootObject as IScriptRootData;
            var E    = arguments[0] as IEntityData;
            var N    = arguments[1]?.ToString();

            try
            {
                var minPos      = E.S.GetCurrent().MinPos;
                var maxPos      = E.S.GetCurrent().MaxPos;
                var S           = E.S.GetCurrent();
                var coreName    = (arguments.Get(2)?.ToString() ?? "Core-Destruct") + $"-{E.Id}";
                var corePosList = E.S.GetCurrent().GetDevicePositions(coreName);
                var directId    = root.IsElevatedScript ? (int.TryParse(arguments.Get(2)?.ToString(), out var manualId) ? manualId : 0) : 0;

                var list        = arguments.Get(3)?.ToString()
                                    .Split(new []{ ',', ';' })
                                    .Select(T => T.Trim())
                                    .Select(T => { 
                                        var delimiter = T.IndexOf('-', 1); 
                                        return delimiter > 0 
                                            ? new Tuple<int,int>(int.TryParse(T.Substring(0, delimiter), out var l1) ? l1 : 0, int.TryParse(T.Substring(delimiter + 1), out var r1) ? r1 : 0)
                                            : new Tuple<int,int>(int.TryParse(T, out var l2) ? l2 : 0, int.TryParse(T, out var r2) ? r2 : 0); 
                                    })
                                    .ToArray();

                var target      = root.E.S.GetCurrent().GetDevice<Eleon.Modding.IContainer>(N);
                if (target == null)
                {
                    root.GetPersistendData().TryRemove(root.ScriptId, out _);
                    options.Inverse(output, context as object);
                    output.WriteLine($"No target container '{N}' found");
                    return;
                }
                var targetPos = root.E.S.GetCurrent().GetDevicePositions(N).First();

                if (directId != E.Id)
                {
                    if (corePosList.Count == 0)
                    {
                        root.GetPersistendData().TryRemove(root.ScriptId, out _);
                        options.Inverse(output, context as object);
                        output.WriteLine($"No core '{coreName}' found on {E.Id}");
                        return;
                    }

                    var corePos = corePosList.First();
                    var core = E.S.GetCurrent().GetBlock(corePos);
                    core.Get(out var coreBlockType, out _, out _, out _);

                    if (coreBlockType != PlayerCoreType)
                    {
                        root.GetPersistendData().TryRemove(root.ScriptId, out _);
                        options.Inverse(output, context as object);
                        output.WriteLine($"No core '{coreName}' found on {E.Id} wrong type {coreBlockType}");
                        return;
                    }
                }

                var processBlockData = root.GetPersistendData().GetOrAdd(root.ScriptId + E.Id, K => new ProcessBlockData() {
                    Started     = DateTime.Now,
                    Name        = E.Name,
                    Id          = E.Id,
                    MinPos      = minPos,
                    MaxPos      = maxPos,
                    X           = minPos.x,
                    Y           = maxPos.y,
                    Z           = minPos.z,
                    TotalBlocks =   (Math.Abs(minPos.x) + Math.Abs(maxPos.x) + 1) *
                                    (Math.Abs(minPos.y) + Math.Abs(maxPos.y) + 1) *
                                    (Math.Abs(minPos.z) + Math.Abs(maxPos.z) + 1)
                }) as ProcessBlockData;

                if(processBlockData.CheckedBlocks < processBlockData.TotalBlocks){
                    lock(processBlockData) ProcessBlockPart(output, root, S, processBlockData, target, targetPos, N, 0, list, (C, I) => C.AddItems(I, 1) > 0);
                    if(processBlockData.CheckedBlocks == processBlockData.TotalBlocks) processBlockData.Finished = DateTime.Now;
                }
                else if((DateTime.Now - processBlockData.Finished).TotalMinutes > 1) root.GetPersistendData().TryRemove(root.ScriptId + E.Id, out _);

                options.Template(output, processBlockData);
            }
            catch (Exception error)
            {
                if (!CsScriptFunctions.FunctionNeedsMainThread(error, root)) output.Write("{{deconstruct}} error " + EmpyrionScripting.ErrorFilter(error));
            }
        }

        [HandlebarTag("recycle")]
        public static void RecycleHelper(TextWriter output, object rootObject, HelperOptions options, dynamic context, object[] arguments)
        {
            if (arguments.Length < 2 || arguments.Length > 3) throw new HandlebarsException("{{recycle entity container [CorePrefix]}} helper must have two to four argument: entity container [CorePrefix] [RemoveItemsIds]");

            var root = rootObject as IScriptRootData;
            var E    = arguments[0] as IEntityData;
            var N    = arguments[1]?.ToString();

            try
            {
                var minPos      = E.S.GetCurrent().MinPos;
                var maxPos      = E.S.GetCurrent().MaxPos;
                var S           = E.S.GetCurrent();
                var coreName    = (arguments.Get(2)?.ToString() ?? "Core-Recycle") + $"-{E.Id}";
                var corePosList = E.S.GetCurrent().GetDevicePositions(coreName);
                var directId    = root.IsElevatedScript ? (int.TryParse(arguments.Get(2)?.ToString(), out var manualId) ? manualId : 0) : 0;

                var target      = root.E.S.GetCurrent().GetDevice<Eleon.Modding.IContainer>(N);
                if (target == null)
                {
                    root.GetPersistendData().TryRemove(root.ScriptId, out _);
                    options.Inverse(output, context as object);
                    output.WriteLine($"No target container '{N}' found");
                    return;
                }
                var targetPos = root.E.S.GetCurrent().GetDevicePositions(N).First();

                if (directId != E.Id)
                {
                    if (corePosList.Count == 0)
                    {
                        root.GetPersistendData().TryRemove(root.ScriptId, out _);
                        options.Inverse(output, context as object);
                        output.WriteLine($"No core '{coreName}' found on {E.Id}");
                        return;
                    }

                    var corePos = corePosList.First();
                    var core = E.S.GetCurrent().GetBlock(corePos);
                    core.Get(out var coreBlockType, out _, out _, out _);

                    if (coreBlockType != PlayerCoreType)
                    {
                        root.GetPersistendData().TryRemove(root.ScriptId, out _);
                        options.Inverse(output, context as object);
                        output.WriteLine($"No core '{coreName}' found on {E.Id} wrong type {coreBlockType}");
                        return;
                    }
                }

                var processBlockData = root.GetPersistendData().GetOrAdd(root.ScriptId + E.Id, K => new ProcessBlockData() {
                    Started     = DateTime.Now,
                    Name        = E.Name,
                    Id          = E.Id,
                    MinPos      = minPos,
                    MaxPos      = maxPos,
                    X           = minPos.x,
                    Y           = maxPos.y,
                    Z           = minPos.z,
                    TotalBlocks =   (Math.Abs(minPos.x) + Math.Abs(maxPos.x) + 1) *
                                    (Math.Abs(minPos.y) + Math.Abs(maxPos.y) + 1) *
                                    (Math.Abs(minPos.z) + Math.Abs(maxPos.z) + 1)
                }) as ProcessBlockData;

                if(processBlockData.CheckedBlocks < processBlockData.TotalBlocks){
                    lock(processBlockData) ProcessBlockPart(output, root, S, processBlockData, target, targetPos, N, 0, null, ExtractBlockToContainer);
                    if(processBlockData.CheckedBlocks == processBlockData.TotalBlocks) processBlockData.Finished = DateTime.Now;
                }
                else if((DateTime.Now - processBlockData.Finished).TotalMinutes > 1) root.GetPersistendData().TryRemove(root.ScriptId + E.Id, out _);

                options.Template(output, processBlockData);
            }
            catch (Exception error)
            {
                if (!CsScriptFunctions.FunctionNeedsMainThread(error, root)) output.Write("{{deconstruct}} error " + EmpyrionScripting.ErrorFilter(error));
            }
        }

        [HandlebarTag("replaceblocks")]
        public static void ReplaceBlocksHelper(TextWriter output, object rootObject, HelperOptions options, dynamic context, object[] arguments)
        {
            if (arguments.Length != 3) throw new HandlebarsException("{{replaceblocks entity RemoveItemsIds1,Id2,... ReplaceId}} helper must have tree argument: entity RemoveItemsIds ReplaceId");

            var root = rootObject as IScriptRootData;
            var E    = arguments[0] as IEntityData;

            try
            {
                if(!root.IsElevatedScript) throw new HandlebarsException("only allowed in elevated scripts");

                var minPos      = E.S.GetCurrent().MinPos;
                var maxPos      = E.S.GetCurrent().MaxPos;
                var S           = E.S.GetCurrent();
                var list        = arguments.Get(1)?.ToString()
                                    .Split(new []{ ',', ';' })
                                    .Select(T => T.Trim())
                                    .Select(T => { 
                                        var delimiter = T.IndexOf('-', 1); 
                                        return delimiter > 0 
                                            ? new Tuple<int,int>(int.TryParse(T.Substring(0, delimiter), out var l1) ? l1 : 0, int.TryParse(T.Substring(delimiter + 1), out var r1) ? r1 : 0)
                                            : new Tuple<int,int>(int.TryParse(T, out var l2) ? l2 : 0, int.TryParse(T, out var r2) ? r2 : 0); 
                                    })
                                    .ToArray();
                int.TryParse(arguments.Get(2)?.ToString(), out var replaceId);

                var processBlockData = root.GetPersistendData().GetOrAdd(root.ScriptId + E.Id, K => new ProcessBlockData() {
                    Started     = DateTime.Now,
                    Name        = E.Name,
                    Id          = E.Id,
                    MinPos      = minPos,
                    MaxPos      = maxPos,
                    X           = minPos.x,
                    Y           = maxPos.y,
                    Z           = minPos.z,
                    TotalBlocks =   (Math.Abs(minPos.x) + Math.Abs(maxPos.x) + 1) *
                                    (Math.Abs(minPos.y) + Math.Abs(maxPos.y) + 1) *
                                    (Math.Abs(minPos.z) + Math.Abs(maxPos.z) + 1)
                }) as ProcessBlockData;

                if(processBlockData.CheckedBlocks < processBlockData.TotalBlocks){
                    lock(processBlockData) ProcessBlockPart(output, root, S, processBlockData, null, VectorInt3.Undef, null, replaceId, list, (C, I) => C.AddItems(I, 1) > 0);
                    if(processBlockData.CheckedBlocks == processBlockData.TotalBlocks) processBlockData.Finished = DateTime.Now;
                }
                else if((DateTime.Now - processBlockData.Finished).TotalMinutes > 1) root.GetPersistendData().TryRemove(root.ScriptId + E.Id, out _);

                options.Template(output, processBlockData);
            }
            catch (Exception error)
            {
                if (!CsScriptFunctions.FunctionNeedsMainThread(error, root)) output.Write("{{replaceblocks}} error " + EmpyrionScripting.ErrorFilter(error));
            }
        }


        static void ProcessBlockPart(TextWriter output, IScriptRootData root, IStructure S, ProcessBlockData processBlockData, 
            IContainer target, VectorInt3 targetPos, string N, int replaceId, Tuple<int,int>[] list,
            Func<IContainer, int, bool> processBlock)
        {
            IDeviceLock locked = null;

            try
            {
                for (; processBlockData.Y >= processBlockData.MinPos.y; processBlockData.Y--)
                {
                    for (; processBlockData.X <= processBlockData.MaxPos.x; processBlockData.X++)
                    {
                        for (; processBlockData.Z <= processBlockData.MaxPos.z; processBlockData.Z++)
                        {
                            processBlockData.CheckedBlocks++;

                            var block = S.GetBlock(processBlockData.X, 128 + processBlockData.Y, processBlockData.Z);
                            if (block != null)
                            {
                                block.Get(out var blockType, out _, out _, out _);

                                if(list != null     && 
                                   list.Length > 0  && 
                                  !list.Any(L => L.Item1 <= blockType && L.Item2 >= blockType)) blockType = 0;

                                if (blockType > 0 && blockType != PlayerCoreType)
                                {
                                    if (EmpyrionScripting.Configuration.Current?.DeconstructBlockSubstitution != null &&
                                        EmpyrionScripting.Configuration.Current.DeconstructBlockSubstitution.TryGetValue(blockType, out var substituteTo)) blockType = substituteTo;

                                    if (blockType > 0 && N != null)
                                    {
                                        locked = locked ?? CreateDeviceLock(root, root.GetCurrentPlayfield(), root.E.S.GetCurrent(), targetPos);
                                        if (!locked.Success)
                                        {
                                            processBlockData.CheckedBlocks--;
                                            output.WriteLine($"Container '{N}' is locked");
                                            return;
                                        }

                                        if (processBlock(target, blockType))
                                        {
                                            processBlockData.CheckedBlocks--;
                                            output.WriteLine($"Container '{N}' is full");
                                            return;
                                        }
                                    }

                                    block.Set(replaceId);
                                    processBlockData.RemovedBlocks++;

                                    if (processBlockData.RemovedBlocks > 100 && processBlockData.RemovedBlocks % 100 == 0 && root.ScriptLoopTimeLimitReached()) return;
                                }
                            }
                        }
                        processBlockData.Z = processBlockData.MinPos.z;
                    }
                    processBlockData.X = processBlockData.MinPos.x;
                }
            }
            finally
            {
                locked?.Dispose();
            }
        }

        private static bool ExtractBlockToContainer(IContainer target, int blockType)
        {
            if (!EmpyrionScripting.ConfigEcfAccess.ResourcesForBlockById.TryGetValue(blockType, out var recipe)) return false;

            var removeRessIfFailed = new List<KeyValuePair<int, int>>();

            var targetContainerFull = recipe.Any(R =>
            {
                var over = target.AddItems(R.Key, R.Value);

                if (over > 0) target.RemoveItems(R.Key, R.Value - over);
                else          removeRessIfFailed.Add(new KeyValuePair<int, int>(R.Key, R.Value));

                return over > 0;
            });

            if (targetContainerFull) removeRessIfFailed.ForEach(R => target.RemoveItems(R.Key, R.Value));

            return targetContainerFull;
        }
    }
}
