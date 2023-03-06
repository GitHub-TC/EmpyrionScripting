using Eleon.Modding;
using EmpyrionNetAPIDefinitions;
using EmpyrionScripting.CsHelper;
using EmpyrionScripting.DataWrapper;
using EmpyrionScripting.Interface;
using EmpyrionScripting.Internal.Interface;
using HandlebarsDotNet;
using Humanizer;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace EmpyrionScripting.CustomHelpers
{
    [HandlebarHelpers]
    public static class ConveyorHelpers
    {
        readonly static object moveLock = new object();
        public static Action<string, LogLevel> Log { get; set; }
        public static Func<IScriptRootData, IPlayfield, IStructure, VectorInt3, IDeviceLock> CreateDeviceLock { get; set; } = (R, P, S, V) => new DeviceLock(R, P, S, V);
        public static Func<IScriptRootData, IPlayfield, IStructure, VectorInt3, IDeviceLock> CreateWeakDeviceLock { get; set; } = (R, P, S, V) => new WeakDeviceLock(R, P, S, V);

        public class ItemMoveInfo : ItemBase, IItemMoveInfo
        {
            public static IList<IItemMoveInfo> Empty = Array.Empty<ItemMoveInfo>();
            public IEntityData SourceE { get; set; }
            public string Source { get; set; }
            public IEntityData DestinationE { get; set; }
            public string Destination { get; set; }
            public int Count { get; set; }
            public string Error { get; set; }
            public int Ammo { get; set; }
            public int Decay { get; set; }
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
                Log($"LockDevice: NoLockAllowed({root.ScriptId}): {root.CycleCounter} % {EmpyrionScripting.Configuration.Current.DeviceLockOnlyAllowedEveryXCycles}", LogLevel.Debug);
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
                using (var locked = CreateWeakDeviceLock(root, root.GetCurrentPlayfield(), S.E?.S.GetCurrent(), position))
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

        [HandlebarTag("trashcontainer")]
        public static void TrashContainerHelper(TextWriter output, object rootObject, dynamic context, object[] arguments)
        {
            if (arguments.Length != 2) throw new HandlebarsException("{{trashcontainer structure containername}} helper must have two arguments: (structure) (containername)");

            var root            = rootObject as IScriptRootData;
            var structure       = arguments[0] as IStructureData;
            var containerName   = arguments[1] as string;

            try
            {
                var containerPos = structure.GetCurrent().GetDevicePositions(containerName).FirstOrDefault();
                var container    = structure.GetCurrent().GetDevice<IContainer>(containerName);

                if (container == null) throw new HandlebarsException("{{trashcontainer}} conatiner not found '" + containerName + "'");

                using var locked = CreateWeakDeviceLock(root, root.GetCurrentPlayfield(), structure.GetCurrent(), containerPos);
                if (!locked.Success)
                {
                    Log($"DeviceIsLocked:{structure.E.Name} -> {containerName}", LogLevel.Debug);
                    return;
                }

                container.SetContent(new List<ItemStack>());
            }
            catch (Exception error)
            {
                if (!CsScriptFunctions.FunctionNeedsMainThread(error, root)) output.Write("{{trashcontainer}} error " + EmpyrionScripting.ErrorFilter(error));
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
                    .ForEach(M => output.Write($"{M.Id}:{M.Source}->{M.Destination}:{M.Error}"));

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
                Log($"Move: NoLockAllowed({root.ScriptId}): {root.CycleCounter} % {EmpyrionScripting.Configuration.Current.DeviceLockOnlyAllowedEveryXCycles}", LogLevel.Debug);
                return ItemMoveInfo.Empty;
            }

            if (root.TimeLimitReached)
            {
                Log($"Move: TimeLimitReached({root.ScriptId})", LogLevel.Debug);
                return ItemMoveInfo.Empty;
            }

            var uniqueNames = structure.AllCustomDeviceNames.GetUniqueNames(namesSearch);
            if(!uniqueNames.Any())
            {
                Log($"NoDevicesFound: {namesSearch}", LogLevel.Debug);
                return ItemMoveInfo.Empty;
            }

            var moveInfos = new List<IItemMoveInfo>();

            lock (moveLock) item.Source
                 .ForEach(S => {
                     using var locked = CreateWeakDeviceLock(root, root.GetCurrentPlayfield(), S.E?.S.GetCurrent(), S.Position);
                     if (!locked.Success)
                     {
                         Log($"DeviceIsLocked (Source): {S.Id} #{S.Count} => {S.CustomName}", LogLevel.Debug);
                         return;
                     }

                     var count = S.Count;
                     if (S.Container == null) count = 0;
                     if (S.IsToken)
                     {
                         var containerItems = S.Container.GetContent();
                         var remainingCount = count;
                         for (int i = containerItems.Count - 1; i >= 0 && remainingCount > 0; i--)
                         {
                             var item = containerItems[i];
                             if(item.CreateId() == S.Id)
                             {
                                 var getCount = item.count;
                                 containerItems.RemoveAt(i);
                                 item.count -= remainingCount;
                                 remainingCount -= getCount;

                                 if(item.count >= 0) containerItems.Insert(i, item);  
                             }
                         }

                         count -= remainingCount;
                         S.Container.SetContent(containerItems); // hier wird ausnahmsweite kein UniqueSlots() benötigt
                     }
                     else count -= S.Container.RemoveItems(S.Id, count);

                     Log($"Move(RemoveItems): {S.CustomName} {S.Id} #{S.Count}->{count}", LogLevel.Debug);

                     ItemMoveInfo currentMoveInfo = null;

                     if (count > 0) uniqueNames
                                 .Where(N => N != S.CustomName)
                                 .ForEach(N => {
                                     var startCount = count;
                                     count = MoveItem(root, S, N, structure, count, maxLimit);
                                     if(startCount != count){
                                         var movedCount = startCount - count;
                                         moveInfos.Add(currentMoveInfo = new ItemMoveInfo() {
                                             Id              = S.Id,
                                             Count           = movedCount,
                                             Ammo            = S.Ammo,
                                             Decay           = S.Decay,
                                             SourceE         = S.E,
                                             Source          = S.CustomName,
                                             DestinationE    = structure.E,
                                             Destination     = N,
                                         });
          
                                         Log($"Move(AddItems): {S.CustomName} {S.Id} #{S.Count}->{N} #{startCount - count}", LogLevel.Debug);
          
                                         // Für diesen Scriptdurchlauf dieses Item aus der Verarbeitung nehmen
                                         S.Count -= movedCount;
                                     };
                                 }, () => root.TimeLimitReached);
          
                     if (count > 0)
                     {
                         var retoureCount = count;
                         if (S.IsToken)
                         {
                             var containerItems = S.Container.GetContent();
                             if (containerItems.Count < 64)
                             {
                                 containerItems.Add(new ItemStack(S.ItemId, count) { ammo = S.Ammo, decay = S.Decay });
                                 S.Container.SetContent(containerItems.UniqueSlots());
                                 count = 0;
                             }
                         }
                         else count = S.Container?.AddItems(S.Id, retoureCount) ?? retoureCount;

                         Log($"Move(retoure): {S.CustomName} {retoureCount} -> {count}", LogLevel.Debug);
                     }

                     if (count > 0)
                     {
                         root.GetPlayfieldScriptData().MoveLostItems.Enqueue(new ItemMoveInfo()
                         {
                             Id         = S.Id,
                             Ammo       = S.Ammo,
                             Decay      = S.Decay,
                             Count      = count,
                             SourceE    = S.E,
                             Source     = S.CustomName,
                         });
                         currentMoveInfo.Error = $"{{move}} error lost #{count} of item {S.Id} in container {S.CustomName} -> add to retry list";
                     }
          
                  }, () => root.TimeLimitReached);

            return moveInfos;
        }

        public static void HandleMoveLostItems(PlayfieldScriptData root)
        {
            var tryCounter = root.MoveLostItems.Count;

            while (tryCounter-- > 0 && root.MoveLostItems.TryDequeue(out var restore))
            {
                try
                {
                    var targetStructure = restore.SourceE.S.GetCurrent();
                    var targetPos       = targetStructure.GetDevicePositions(restore.Source).FirstOrDefault();
                    var targetContainer = targetStructure.GetDevice<IContainer>(restore.Source);

                    if(targetContainer == null)
                    {
                        Log($"HandleMoveLostItems(target container not found): {restore.Source} {restore.Id} #{restore.Count}", LogLevel.Message);
                        root.MoveLostItems.Enqueue(restore);
                        continue;
                    }

                    var isLocked = restore.SourceE.GetCurrentPlayfield().IsStructureDeviceLocked(restore.SourceE.S.GetCurrent().Id, targetPos);
                    if (isLocked)
                    {
                        Log($"HandleMoveLostItems(container is locked): {restore.Source} {restore.Id}", LogLevel.Debug);
                        root.MoveLostItems.Enqueue(restore);
                        continue;
                    }

                    var count     = restore.Count;
                    var stackSize = 0;

                    if (!restore.IsToken)
                    {
                        count = targetContainer.AddItems(restore.Id, restore.Count);

                        try
                        {
                            stackSize = (int)EmpyrionScripting.ConfigEcfAccess.FindAttribute(restore.Id, "StackSize");
                            if (stackSize < count)
                            {
                                Log($"HandleMoveLostItems(split invalid stacks): {restore.Source} {restore.Id} #{restore.Count}->{stackSize}", LogLevel.Message);

                                var countSplit = count;
                                while (countSplit > 0)
                                {
                                    root.MoveLostItems.Enqueue(new ItemMoveInfo()
                                    {
                                        Id      = restore.Id,
                                        Ammo    = restore.Ammo,
                                        Decay   = restore.Decay,
                                        Count   = Math.Min(countSplit, stackSize),
                                        SourceE = restore.SourceE,
                                        Source  = restore.Source,
                                    });
                                    countSplit -= stackSize;
                                }

                                continue;
                            }
                        }
                        catch { /* Fehler ignorieren */ }
                    }

                    // AddItem funktioniert leider nicht (mehr) wenn den der Stack gar nicht in den Container passt
                    if (count > 0 && count == restore.Count && count > stackSize)
                    {
                        var content = targetContainer.GetContent();

                        if (content.Count < 64)
                        {
                            content.Add(new ItemStack(restore.ItemId, restore.Count) { ammo = restore.Ammo, decay = restore.Decay});
                            targetContainer.SetContent(content.UniqueSlots());
                            if(!restore.IsToken) Log($"HandleMoveLostItems(restored set content fallback): {restore.Source} {restore.Id} #{restore.Count}", LogLevel.Message);

                            continue;
                        }
                    }

                    if (count > 0)
                    {
                        root.MoveLostItems.Enqueue(new ItemMoveInfo()
                        {
                            Id      = restore.Id,
                            Ammo    = restore.Ammo,
                            Decay   = restore.Decay,
                            Count   = count,
                            SourceE = restore.SourceE,
                            Source  = restore.Source,
                        });
                        Log($"HandleMoveLostItems(partial restored): {restore.Source} {restore.Id} #{restore.Count} -> {count}", restore.Count == count ? LogLevel.Debug : LogLevel.Message);
                    }
                    else Log($"HandleMoveLostItems(restored): {restore.Source} {restore.Id} #{restore.Count}", LogLevel.Message);
                }
                catch (Exception error)
                {
                    Log($"HandleMoveLostItems(error): {restore.Source} {restore.Id} #{restore.Count} -> {EmpyrionScripting.ErrorFilter(error)}", LogLevel.Message);
                    root.MoveLostItems.Enqueue(restore);
                }

                if (root.ScriptExecQueue.TimeLimitSyncReached()) break;
            }
        }

        private static int MoveItem(IScriptRootData root, IItemsSource S, string N, IStructureData targetStructure, int count, int? maxLimit)
        {
            var target = targetStructure?.GetCurrent()?.GetDevice<IContainer>(N);
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

            var tryMoveCount = maxLimit.HasValue && !S.IsToken
                ? Math.Max(0, Math.Min(count, maxLimit.Value - target.GetTotalItems(S.Id)))
                : count;

            using var locked = CreateWeakDeviceLock(root, root.GetCurrentPlayfield(), targetStructure.GetCurrent(), targetData.Position);
            if (!locked.Success)
            {
                Log($"DeviceIsLocked (Target): {S.Id} #{S.Count} => {targetData.CustomName}", LogLevel.Debug);
                return count;
            }

            if (S.IsToken)
            {
                var itemList = target.GetContent();
                if(itemList.Count < 64)
                {
                    itemList.Add(new ItemStack(S.ItemId, tryMoveCount) { ammo = S.Ammo, decay = S.Decay });
                    target.SetContent(itemList.UniqueSlots());
                    return 0;
                }
                return tryMoveCount;
            }
            else return maxLimit.HasValue
                ? target.AddItems(S.Id, tryMoveCount) + (count - tryMoveCount)
                : target.AddItems(S.Id, tryMoveCount);
        }

        enum GardenerOperation
        {
            Harvest,
            Pickup,
        }

        [HandlebarTag("harvest")]
        public static void HarvestHelper(TextWriter output, object rootObject, HelperOptions options, dynamic context, object[] arguments)
        {
            if (arguments.Length != 6 && arguments.Length != 7) throw new HandlebarsException("{{harvest structure block's target gx gy gz [removeDeadPlants]}} helper must have six argument: (structure) (block's) (target) (gardenerX) (gardenerY) (gardenerZ) (removeDeadPlants) -> " + arguments.Length.ToString());

            PlantsFunction(GardenerOperation.Harvest, output, rootObject, options, (object)context, arguments);
        }

        [HandlebarTag("pickupplants")]
        public static void PickupPlantsHelper(TextWriter output, object rootObject, HelperOptions options, dynamic context, object[] arguments)
        {
            if (arguments.Length != 6 && arguments.Length != 7) throw new HandlebarsException("{{pickupplants structure block's target gx gy gz [removeDeadPlants]}} helper must have six argument: (structure) (block's) (target) (gardenerX) (gardenerY) (gardenerZ) (removeDeadPlants) -> " + arguments.Length.ToString());

            PlantsFunction(GardenerOperation.Pickup, output, rootObject, options, (object)context, arguments);
        }

        [HandlebarTag("replantplants")]
        public static void ReplantPlantsHelper(TextWriter output, object rootObject, HelperOptions options, dynamic context, object[] arguments)
        {
            if (arguments.Length != 2) throw new HandlebarsException("{{replantplants structure target}} helper must have two argument: (structure) (target) -> " + arguments.Length.ToString());

            var root        = rootObject as IScriptRootData;
            var structure   = arguments[0] as IStructureData;
            var namesSearch = arguments.Get<string>(1);

            try
            {
                var cacheId = $"{structure.E.Id}PickupPlants";
                if (!root.GetPersistendData().TryGetValue(cacheId, out var data))
                {
                    options.Inverse(output, context as object);
                    return;
                }

                var uniqueNames = structure.AllCustomDeviceNames.GetUniqueNames(namesSearch).ToList();
                if (!uniqueNames.Any())
                {
                    output.WriteLine($"NoDevicesFound: {namesSearch}");
                    return;
                }

                IContainer container = null;
                VectorInt3 containerPos = VectorInt3.Undef;

                var firstTarget = GetNextContainer(root, uniqueNames, ref container, ref containerPos);
                if (string.IsNullOrEmpty(firstTarget))
                {
                    if (firstTarget == null) output.WriteLine($"Containers '{namesSearch}' are locked");
                    return;
                }

                using var locked = CreateWeakDeviceLock(root, root.GetCurrentPlayfield(), structure.GetCurrent(), containerPos);
                if (!locked.Success)
                {
                    Log($"DeviceIsLocked (harvest): {containerPos}", LogLevel.Debug);
                    return;
                }

                var plantListData = (List<PickupPlantData>)data;
                for (int i = Math.Min(plantListData.Count, EmpyrionScripting.Configuration.Current.ProcessMaxBlocksPerCycle) - 1; i >= 0; i--)
                {
                    var plant = plantListData[i];
                    plantListData.RemoveAt(i);

                    var block = structure.GetCurrent().GetBlock(new VectorInt3(plant.X, plant.Y, plant.Z));
                    block.Get(out var blockType, out _, out _, out _);
                    if (blockType == 0)
                    {
                        var remainingCount = container.RemoveItems(plant.Id, 1);
                        if(remainingCount == 0) block.Set(plant.Id);
                    }

                    options.Template(output, plant);
                }

                if (plantListData.Count == 0) root.GetPersistendData().TryRemove(cacheId, out _);
                else                          root.GetPersistendData().AddOrUpdate(cacheId, plantListData, (n, l) => plantListData);
            }
            catch (Exception error)
            {
                if (!CsScriptFunctions.FunctionNeedsMainThread(error, root)) output.Write("{{replantplants}} error " + EmpyrionScripting.ErrorFilter(error));
            }
        }

        class PickupPlantData {
            public int Id { get; set; }
            public int X { get; set; }
            public int Y { get; set; }
            public int Z { get; set; }
        }

        static void PlantsFunction(GardenerOperation op, TextWriter output, object rootObject, HelperOptions options, dynamic context, object[] arguments)
        {
            var root      = rootObject as IScriptRootData;
            var structure = arguments[0] as IStructureData;

            try
            {
                BlockData[] blocks = null;
                if (arguments.Get(1) is BlockData[] blockArray) blocks = blockArray;
                if (arguments.Get(1) is BlockData blockSingle)  blocks = new[] { blockSingle };

                var namesSearch = arguments.Get<string>(2);
                int.TryParse(arguments.Get(3)?.ToString(), out var gx);
                int.TryParse(arguments.Get(4)?.ToString(), out var gy);
                int.TryParse(arguments.Get(5)?.ToString(), out var gz);
                bool.TryParse(arguments.Get(6)?.ToString(), out var removeDeadPlants);

                if (blocks?.Length == 0)
                {
                    output.WriteLine($"No blocks {arguments.Get(1)} for harvesting");
                    return;
                }

                var uniqueNames = structure.AllCustomDeviceNames.GetUniqueNames(namesSearch).ToList();
                if (!uniqueNames.Any())
                {
                    output.WriteLine($"NoDevicesFound: {namesSearch}");
                    return;
                }

                var gardener = new BlockData(structure.E, new VectorInt3(gx, gy, gz));  
                if(gardener == null || !root.Ids.TryGetValue("Gardeners", out var gardenersList) || !LogicHelpers.In(gardener.Id, gardenersList))
                {
                    output.WriteLine($"No gardener {(gardener != null ? root.ConfigEcfAccess.IdBlockMapping.TryGetValue(gardener.Id, out var foundBlock) ? foundBlock : gardener.Id.ToString() : "?")} found at x:{gx} y:{gy} z:{gz}: {(root.NamedIds.TryGetValue("Gardeners", out var allowedGardenersList) ? allowedGardenersList : "No gardener allowed")}");
                    return;
                }

                IContainer container    = null;
                VectorInt3 containerPos = VectorInt3.Undef;

                var firstTarget = GetNextContainer(root, uniqueNames, ref container, ref containerPos);
                if (string.IsNullOrEmpty(firstTarget))
                {
                    if(firstTarget == null) output.WriteLine($"Containers '{namesSearch}' are locked");
                    return;
                }

                using var locked = CreateWeakDeviceLock(root, root.GetCurrentPlayfield(), structure.GetCurrent(), containerPos);
                if (!locked.Success)
                {
                    Log($"DeviceIsLocked (harvest): {containerPos}", LogLevel.Debug);
                    return;
                }

                foreach (var block in blocks)
                {
                    var amount = EmpyrionScripting.Configuration.Current.GardenerSalary.Amount;

                    if (!root.ConfigEcfAccess.HarvestBlockData.TryGetValue(block.Id, out var harvestInfo)) continue;

                    // Tote Pflanzen stehen lassen oder das 100 fache für das "Aufräumen" nehmen
                    if (op == GardenerOperation.Pickup) amount *= 10;
                    else if (harvestInfo.Name.Contains("PlantDead"))
                    {
                        if (removeDeadPlants) amount *= 100;
                        else                  continue;
                    }

                    var salary = container.GetTotalItems(EmpyrionScripting.Configuration.Current.GardenerSalary.ItemId);
                    if (salary < amount)
                    {
                        output.WriteLine($"Not enougth salary for gardener: {amount} of {(root.ConfigEcfAccess.IdBlockMapping.TryGetValue(EmpyrionScripting.Configuration.Current.GardenerSalary.ItemId, out var salaryItemName) ? salaryItemName : EmpyrionScripting.Configuration.Current.GardenerSalary.ItemId.ToString())}");
                        return;
                    }

                    if (harvestInfo.Name.Contains("PlantDead"))
                    {
                        options.Template(output, harvestInfo);

                        container.RemoveItems(EmpyrionScripting.Configuration.Current.GardenerSalary.ItemId, amount);
                        block.ChangeBlockType(0);

                        continue;
                    }

                    var opId      = op == GardenerOperation.Harvest ? harvestInfo.DropOnHarvestId    : harvestInfo.PickupTargetId;
                    var opCount   = op == GardenerOperation.Harvest ? harvestInfo.DropOnHarvestCount : 1;
                    var opChildId = op == GardenerOperation.Harvest ? harvestInfo.ChildOnHarvestId   : 0;

                    if (opId != 0 && opCount !=  0)
                    {
                        var count = container.AddItems(opId, opCount);
                        if (count > 0)
                        {
                            root.GetPlayfieldScriptData().MoveLostItems.Enqueue(new ItemMoveInfo()
                            {
                                Id      = opId,
                                Ammo    = 0,
                                Decay   = 0,
                                Count   = count,
                                SourceE = structure.E,
                                Source  = firstTarget,
                            });
                            output.WriteLine($"{op} lost #{count} of item {opId} -> add to retry list");
                            options.Inverse(output, context as object);
                        }
                        else
                        {
                            options.Template(output, harvestInfo);
                        }

                        if(op == GardenerOperation.Pickup)
                        {
                            var pickupData = new PickupPlantData
                            {
                                Id = opId,
                                X  = block.Position.x,
                                Y  = block.Position.y,
                                Z  = block.Position.z,
                            }; 

                            var cacheId = $"{structure.E.Id}PickupPlants";
                            root.GetPersistendData().AddOrUpdate(cacheId, new List<PickupPlantData> { pickupData }, (n, data) => { 
                                var pickupListData = (List<PickupPlantData>)data;
                                var found = pickupListData.FirstOrDefault(p => p.X == pickupData.X && p.Y == pickupData.Y && p.Z == pickupData.Z);

                                if (found == null) pickupListData.Add(pickupData);
                                else               found.Id = pickupData.Id;

                                return data; 
                            });
                        }

                        Log($"{structure.E.Name}/{structure.E.Id} PickupPlants:[{op}] x:{block.Position.x} y:{block.Position.y} z:{block.Position.z} ID:{block.Id} -> {opChildId}", LogLevel.Message);

                        container.RemoveItems(EmpyrionScripting.Configuration.Current.GardenerSalary.ItemId, amount);
                        block.ChangeBlockType(opChildId);
                    }
                }

            }
            catch (Exception error)
            {
                if (!CsScriptFunctions.FunctionNeedsMainThread(error, root)) output.Write("{{harvest}} error " + EmpyrionScripting.ErrorFilter(error));
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
                    .ForEach(E => output.Write(E), () => !root.Running);

                if (moveInfos.Count == 0) options.Inverse(output, context as object);
                else                      moveInfos.ForEach(I => options.Template(output, I), () => root.TimeLimitReached);
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
                Log($"Fill: NoLockAllowed({root.ScriptId}): {root.CycleCounter} % {EmpyrionScripting.Configuration.Current.DeviceLockOnlyAllowedEveryXCycles}", LogLevel.Debug);
                return ItemMoveInfo.Empty;
            }

            var specialTransfer = type switch
            {
                StructureTankType.Oxygen    => structure.OxygenTank  ,
                StructureTankType.Fuel      => structure.FuelTank    ,
                StructureTankType.Pentaxid  => structure.PentaxidTank,
                _                           => null,
            };

            if (specialTransfer == null || !specialTransfer.AllowedItem(item.Id)) return ItemMoveInfo.Empty;

            Log($"Fill Total: #{item.Source.Count}", LogLevel.Debug);

            var moveInfos = new List<IItemMoveInfo>();

            lock(moveLock) item.Source
                .ForEach(S => {
                    using var locked = CreateWeakDeviceLock(root, root.GetCurrentPlayfield(), S.E?.S.GetCurrent(), S.Position);
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
                            Id = S.Id,
                            Count = startCount - count,
                            SourceE = S.E,
                            Source = S.CustomName,
                            DestinationE = structure.E,
                            Destination = type.ToString(),
                        });
                    };

                    if (count > 0) count = S.Container.AddItems(S.Id, count);
                    if (count > 0 && currentMoveInfo != null)
                    {
                        root.GetPlayfieldScriptData().MoveLostItems.Enqueue(new ItemMoveInfo()
                        {
                            Id         = S.Id,
                            Count      = count,
                            SourceE    = S.E,
                            Source     = S.CustomName,
                        });
                        currentMoveInfo.Error = $"{{fill}} error lost #{count} of item {S.Id} in container {S.CustomName} -> add to retry list";
                    }

                }, () => root.TimeLimitReached);

            return moveInfos;
        }

        public class ProcessBlockData
        {
            public DateTime Started { get; set; }
            public DateTime LastAccess { get; set; }
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
            if (arguments.Length < 2 || arguments.Length > 4) throw new HandlebarsException("{{deconstruct entity container [CorePrefix] [RemoveItemsIds1,Id2,...]}} helper must have two to four argument: entity (container;container*;*;container) [CorePrefix] [RemoveItemsIds]");

            var root = rootObject as IScriptRootData;
            var E    = arguments[0] as IEntityData;
            var N    = arguments[1]?.ToString();

            try
            {
                // Eigene Struktur nicht "abbauen"
                if (E.Id == root.E.Id) return;

                var commandId = $"{root.ScriptId}-deconstruct";
                if ((!root.GetPlayfieldScriptData().EntityExclusiveAccess.TryGetValue(E.Id, out var accessBy) || accessBy.CommandId != commandId) && !root.GetPlayfieldScriptData().EntityExclusiveAccess.TryAdd(E.Id, new ExclusiveAccess { CommandId = commandId, EntityName = root.E.Name, EntityId = root.E.Id }))
                {
                    if (accessBy.EntityId != root.E.Id) output.WriteLine($"In process by {accessBy.EntityName}");
                    return;
                }

                var list = arguments.Get(3)?.ToString()
                    .Split(new[] { ',', ';' })
                    .Select(T => T.Trim())
                    .Select(T => {
                        var delimiter = T.IndexOf('-', 1);
                        return delimiter > 0
                            ? new Tuple<int, int>(int.TryParse(T.Substring(0, delimiter), out var l1) ? l1 : 0, int.TryParse(T.Substring(delimiter + 1), out var r1) ? r1 : 0)
                            : new Tuple<int, int>(int.TryParse(T, out var l2) ? l2 : 0, int.TryParse(T, out var r2) ? r2 : 0);
                    })
                    .ToArray();

                if(!ConvertBlocks(output, root, options, context as object, arguments,
                    (arguments.Get(2)?.ToString() ?? "Core-Destruct") + $"-{E.Id}", list,
                    DeconstructBlock)) root.GetPlayfieldScriptData().EntityExclusiveAccess.TryRemove(E.Id, out _);
            }
            catch (Exception error)
            {
                if (!CsScriptFunctions.FunctionNeedsMainThread(error, root)) output.Write("{{deconstruct}} error " + EmpyrionScripting.ErrorFilter(error));
            }
        }

        [HandlebarTag("recycle")]
        public static void RecycleHelper(TextWriter output, object rootObject, HelperOptions options, dynamic context, object[] arguments)
        {
            if (arguments.Length < 2 || arguments.Length > 4) throw new HandlebarsException("{{recycle entity container [CorePrefix]}} helper must have two to four argument: entity (container;container*;*;container) [CorePrefix] [RemoveItemsIds]");

            var root = rootObject   as IScriptRootData;
            var E    = arguments[0] as IEntityData;

            try
            {
                // Eigene Struktur nicht "abbauen"
                if (E.Id == root.E.Id) return;

                var commandId = $"{root.ScriptId}-recycle";
                if ((!root.GetPlayfieldScriptData().EntityExclusiveAccess.TryGetValue(E.Id, out var accessBy) || accessBy.CommandId != commandId) && !root.GetPlayfieldScriptData().EntityExclusiveAccess.TryAdd(E.Id, new ExclusiveAccess { CommandId = commandId, EntityName = root.E.Name, EntityId = root.E.Id }))
                {
                    if(accessBy.EntityId != root.E.Id) output.WriteLine($"In process by {accessBy.EntityName}");
                    return;
                }

                if (!ConvertBlocks(output, root, options, context as object, arguments,
                    (arguments.Get(2)?.ToString() ?? "Core-Recycle") + $"-{E.Id}", null,
                    ExtractBlockToRecipe)) root.GetPlayfieldScriptData().EntityExclusiveAccess.TryRemove(E.Id, out _);
            }
            catch (Exception error)
            {
                if (!CsScriptFunctions.FunctionNeedsMainThread(error, root)) output.Write("{{deconstruct}} error " + EmpyrionScripting.ErrorFilter(error));
            }
        }

        public static bool ConvertBlocks(TextWriter output, IScriptRootData root, HelperOptions options, object context, object[] arguments, string coreName,
            Tuple<int, int>[] list, Func<IEntityData, Dictionary<int, double>, int, bool> processBlock)
        { 
            var E    = arguments[0] as IEntityData;
            var N    = arguments[1]?.ToString();

            var minPos = E.S.GetCurrent().MinPos;
            var maxPos      = E.S.GetCurrent().MaxPos;
            var S           = E.S.GetCurrent();
            var corePosList = E.S.GetCurrent().GetDevicePositions(coreName);
            var directId    = root.IsElevatedScript ? (int.TryParse(arguments.Get(2)?.ToString(), out var manualId) ? manualId : 0) : 0;

            // Empyrion hat einen "komischen" y-offest von 128
            minPos = new VectorInt3(minPos.x, 128 + minPos.y, minPos.z);
            maxPos = new VectorInt3(maxPos.x, 128 + maxPos.y, maxPos.z);

            var uniqueNames = root.E.S.AllCustomDeviceNames.GetUniqueNames(N).ToList();

            if (!uniqueNames.Any())
            {
                root.GetPersistendData().TryRemove(root.ScriptId, out _);
                options.Inverse(output, context);
                output.WriteLine($"No target container '{N}' found");
                return false;
            }

            if (directId != E.Id)
            {
                if (corePosList.Count == 0)
                {
                    root.GetPersistendData().TryRemove(root.ScriptId, out _);
                    options.Inverse(output, context);
                    output.WriteLine($"No core '{coreName}' found on {E.Id}");
                    return false;
                }

                var corePos = corePosList.First();
                var core = E.S.GetCurrent().GetBlock(corePos);
                core.Get(out var coreBlockType, out _, out _, out _);

                if (!EmpyrionScripting.Configuration.Current.HarvestCoreTypes.Contains(coreBlockType))
                {
                    root.GetPersistendData().TryRemove(root.ScriptId, out _);
                    options.Inverse(output, context);
                    output.WriteLine($"No core '{coreName}' found on {E.Id} wrong type {coreBlockType}");
                    return false;
                }
            }

            IContainer target = null;
            VectorInt3 targetPos = VectorInt3.Undef;

            var firstTarget = GetNextContainer(root, uniqueNames, ref target, ref targetPos);
            if (string.IsNullOrEmpty(firstTarget))
            {
                root.GetPersistendData().TryRemove(root.ScriptId, out _);
                options.Inverse(output, context);
                if(firstTarget == null) output.WriteLine($"Containers '{N}' are locked");
                return false;
            }

            EmpyrionScripting.Log($"{root.E.Name}/{root.E.Id} Ressource to first conatiner: {firstTarget}", LogLevel.Message);

            var processBlockData = root.GetPersistendData().GetOrAdd(root.ScriptId + E.Id, K => new ProcessBlockData() {
                Started     = DateTime.Now,
                LastAccess  = DateTime.Now,
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

            if (processBlockData.CheckedBlocks < processBlockData.TotalBlocks)
            {
                var ressources = new Dictionary<int, double>();

                lock (processBlockData) ProcessBlockPart(output, root, S, processBlockData, target, targetPos, N, 0, list, (c, i) => processBlock(E, ressources, i));

                var allToLostItemRecover = false;
                var currentContainer = firstTarget;

                var ressourcesWithStackLimit = new List<KeyValuePair<int, double>>();

                if (ressources.Any()) processBlockData.LastAccess = DateTime.Now;

                ressources.ForEach(r =>
                    {
                        try
                        {
                            var stackSize = (int)EmpyrionScripting.ConfigEcfAccess.FindAttribute(r.Key, "StackSize");
                            var count = (int)r.Value;
                            while (count > 0)
                            {
                                ressourcesWithStackLimit.Add(new KeyValuePair<int, double>(r.Key, Math.Min(count, stackSize)));
                                count -= stackSize;
                            }
                        }
                        catch
                        {
                            ressourcesWithStackLimit.Add(r);
                        }
                    }
                );

                ressourcesWithStackLimit.ForEach(R =>
                {
                    var over = allToLostItemRecover ? (int)R.Value : target.AddItems(R.Key, (int)R.Value);

                    if (over > 0 && !allToLostItemRecover)
                    {
                        EmpyrionScripting.Log($"{root.E.Name}/{root.E.Id} Container full: {R.Key} #{over} -> {currentContainer}", LogLevel.Message);

                        currentContainer = GetNextContainer(root, uniqueNames, ref target, ref targetPos);
                        if (string.IsNullOrEmpty(currentContainer))
                        {
                            if (currentContainer == null) EmpyrionScripting.Log($"{root.E.Name}/{root.E.Id} All Container full or blocked", LogLevel.Message);
                            allToLostItemRecover = true;
                        }
                        else
                        {
                            var nextTry = over;
                            over = target.AddItems(R.Key, over);
                            EmpyrionScripting.Log($"{root.E.Name}/{root.E.Id} Ressource to NextContainer: {R.Key} #{nextTry} -> #{over} -> {currentContainer}", LogLevel.Message);
                        }
                    }

                    if (over > 0)
                    {
                        EmpyrionScripting.Log($"{root.E.Name}/{root.E.Id} Ressource to LostItemsRecover: {R.Key} #{over} -> {firstTarget}", LogLevel.Message);

                        root.GetPlayfieldScriptData().MoveLostItems.Enqueue(new ItemMoveInfo()
                        {
                            Id      = R.Key,
                            Count   = over,
                            SourceE = root.E,
                            Source  = firstTarget,
                        });
                    }
                });

                if (processBlockData.CheckedBlocks == processBlockData.TotalBlocks) processBlockData.Finished = DateTime.Now;
            }
            else if ((DateTime.Now - processBlockData.LastAccess).TotalMinutes > 5)
            {
                root.GetPersistendData().TryRemove(root.ScriptId + E.Id, out _);
                options.Inverse(output, processBlockData);
                return false;
            }

            options.Template(output, processBlockData);

            return processBlockData.CheckedBlocks < processBlockData.TotalBlocks;
        }

        private static string GetNextContainer(IScriptRootData root, List<string> uniqueNames, ref IContainer target, ref VectorInt3 targetPos)
        {
            while (uniqueNames.Any())
            {
                var currentTarget = uniqueNames.First();
                uniqueNames.RemoveAt(0);

                try
                {
                    target    = root.E.S.GetCurrent().GetDevice<Eleon.Modding.IContainer>(currentTarget);
                    targetPos = root.E.S.GetCurrent().GetDevicePositions(currentTarget).First();

                    if(target != null)
                    {
                        var locking = CreateWeakDeviceLock(root, root.GetCurrentPlayfield(), root.E.S.GetCurrent(), targetPos);

                        if (locking.Exit)
                        {
                            //EmpyrionScripting.Log($"GetNextContainer:{currentTarget} at pos {targetPos} -> Exit", LogLevel.Debug);
                            return string.Empty;
                        }

                        if (locking.Success)
                        {
                            EmpyrionScripting.Log($"GetNextContainer:{currentTarget} at pos {targetPos}", LogLevel.Debug);
                            return currentTarget;
                        }
                        else
                        {
                            EmpyrionScripting.Log($"GetNextContainer: {currentTarget} at pos {targetPos} -> no free container", LogLevel.Debug);
                        }
                    }
                }
                catch (Exception error)
                {
                    EmpyrionScripting.Log($"GetNextContainer: {currentTarget} at pos {targetPos} -> no container {error}", LogLevel.Debug);
                }
            }

            return null;
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

                // Empyrion hat einen "komischen" y-offest von 128
                minPos = new VectorInt3(minPos.x, 128 + minPos.y, minPos.z);
                maxPos = new VectorInt3(maxPos.x, 128 + maxPos.y, maxPos.z);

                var processBlockData = root.GetPersistendData().GetOrAdd(root.ScriptId + E.Id, K => new ProcessBlockData() {
                    Started     = DateTime.Now,
                    LastAccess  = DateTime.Now,
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

                if (processBlockData.CheckedBlocks < processBlockData.TotalBlocks)
                {
                    lock (processBlockData) ProcessBlockPart(output, root, S, processBlockData, null, VectorInt3.Undef, null, replaceId, list, (C, I) => C.AddItems(I, 1) > 0);
                    if (processBlockData.CheckedBlocks == processBlockData.TotalBlocks) processBlockData.Finished = DateTime.Now;
                }
                else if ((DateTime.Now - processBlockData.LastAccess).TotalMinutes > 5)
                {
                    root.GetPersistendData().TryRemove(root.ScriptId + E.Id, out _);
                    options.Inverse(output, processBlockData);
                }

                options.Template(output, processBlockData);
            }
            catch (Exception error)
            {
                if (!CsScriptFunctions.FunctionNeedsMainThread(error, root)) output.Write("{{replaceblocks}} error " + EmpyrionScripting.ErrorFilter(error));
            }
        }


        public static void ProcessBlockPart(TextWriter output, IScriptRootData root, IStructure S, ProcessBlockData processBlockData,
            IContainer target, VectorInt3 targetPos, string N, int replaceId, Tuple<int, int>[] list,
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

                            var block = S.GetBlock(processBlockData.X, processBlockData.Y, processBlockData.Z);
                            if (block != null)
                            {
                                block.Get(out var blockType, out _, out _, out _);

                                if(list != null     && 
                                   list.Length > 0  && 
                                  !list.Any(L => L.Item1 <= blockType && L.Item2 >= blockType)) blockType = 0;

                                if (blockType > 0 && !EmpyrionScripting.Configuration.Current.HarvestCoreTypes.Contains(blockType))
                                {
                                    if (EmpyrionScripting.Configuration.Current?.DeconstructBlockSubstitution != null &&
                                        EmpyrionScripting.Configuration.Current.DeconstructBlockSubstitution.TryGetValue(blockType, out var substituteTo)) blockType = substituteTo;

                                    if (blockType != 0 && EmpyrionScripting.Configuration.Current.WithinRemoveBlocks(blockType)) blockType = 0;

                                    if (blockType > 0 && N != null)
                                    {
                                        locked = locked ?? CreateWeakDeviceLock(root, root.GetCurrentPlayfield(), root.E.S.GetCurrent(), targetPos);
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
                                    else if(N == null)
                                    {
                                        if (processBlock(target, blockType))
                                        {
                                            processBlockData.CheckedBlocks--;
                                            return;
                                        }
                                    }

                                    if (replaceId >= 0) block.ParentBlock.Set(replaceId);
                                    processBlockData.RemovedBlocks++;

                                    if (processBlockData.RemovedBlocks % EmpyrionScripting.Configuration.Current.ProcessMaxBlocksPerCycle == 0 || root.TimeLimitReached)
                                    {
                                        processBlockData.Z++;
                                        return;
                                    }
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

        private static bool DeconstructBlock(IEntityData E, Dictionary<int, double> ressources, int blockId)
        {
            string blockName = EmpyrionScripting.ConfigEcfAccess.FindAttribute(blockId, "PickupTarget")?.ToString();

            if (string.IsNullOrEmpty(blockName) && EmpyrionScripting.ConfigEcfAccess.FlatConfigBlockById.TryGetValue(blockId, out var blockData))
            {
                blockName = blockData.Values.TryGetValue("Name", out var name) ? name.ToString() : null;
            }

            if (!string.IsNullOrEmpty(blockName) && EmpyrionScripting.ConfigEcfAccess.ParentBlockName.TryGetValue(PlaceAtType(E.EntityType) + blockName, out var parentBlockName1)) blockName = parentBlockName1;
            if (!string.IsNullOrEmpty(blockName) && EmpyrionScripting.ConfigEcfAccess.ParentBlockName.TryGetValue(                            blockName, out var parentBlockName2)) blockName = parentBlockName2;

            var id = blockName != null && EmpyrionScripting.ConfigEcfAccess.BlockIdMapping.TryGetValue(blockName, out var mappedBlockId) ? mappedBlockId : blockId;

            if (EmpyrionScripting.Configuration.Current?.DeconstructBlockSubstitution != null &&
                EmpyrionScripting.Configuration.Current.DeconstructBlockSubstitution.TryGetValue(id, out var substituteTo)) id = substituteTo;

            if (id != 0 && EmpyrionScripting.Configuration.Current.WithinRemoveBlocks(id)) id = 0;

            if (id == 0) return false;

            if (ressources.TryGetValue(id, out var count)) ressources[id] = count + 1;
            else                                           ressources.Add(id, 1);

            return false;
        }

        private static bool ExtractBlockToRecipe(IEntityData E, Dictionary<int, double> ressources, int blockId)
        {
            EmpyrionScripting.ConfigEcfAccess.FlatConfigBlockById.TryGetValue(blockId, out var blockData);

            if (!EmpyrionScripting.ConfigEcfAccess.ResourcesForBlockById.TryGetValue(blockId, out var recipe))
            {
                if(blockData?.Values != null && blockData.Values.ContainsKey("Name")){
                    string parentBlockName = null;
                    if (EmpyrionScripting.ConfigEcfAccess.ParentBlockName.TryGetValue(PlaceAtType(E.EntityType) + blockData.Values["Name"].ToString(), out var parentBlockName1)) parentBlockName = parentBlockName1;
                    if (EmpyrionScripting.ConfigEcfAccess.ParentBlockName.TryGetValue(                            blockData.Values["Name"].ToString(), out var parentBlockName2)) parentBlockName = parentBlockName2;

                    if (parentBlockName != null && EmpyrionScripting.ConfigEcfAccess.ResourcesForBlockById.TryGetValue(EmpyrionScripting.ConfigEcfAccess.BlockIdMapping[parentBlockName], out var parentRecipe)) recipe = parentRecipe;
                }

                if (recipe == null)
                {
                    EmpyrionScripting.Log($"No recipe for {blockId}:{(EmpyrionScripting.ConfigEcfAccess.FlatConfigBlockById.TryGetValue(blockId, out var noRecipeBlock) ? noRecipeBlock.Values["Name"] : "")}", LogLevel.Message);
                    return false;
                }
            }
            EmpyrionScripting.Log($"Recipe for [{blockId}] {blockData?.Values["Name"]}: {recipe.Aggregate("", (r, i) => $"{r}\n{i.Key}:{i.Value}")}", LogLevel.Debug);

            recipe.ForEach(R =>
            {
                if (ressources.TryGetValue(R.Key, out var count)) ressources[R.Key] = count + R.Value;
                else                                              ressources.Add(R.Key, R.Value);
            });

            return false;
        }

        private static string PlaceAtType(EntityType entityType) => entityType switch
        {
            EntityType.BA => "Base",
            EntityType.CV => "MS",
            EntityType.SV => "SS",
            EntityType.HV => "GV",
            _             => string.Empty,
        };
    }
}
