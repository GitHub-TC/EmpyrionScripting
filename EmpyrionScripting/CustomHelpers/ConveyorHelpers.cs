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
        public class ItemMoveInfo
        {
            public int Id { get; set; }
            public EntityData SourceE { get; set; }
            public string Source { get; set; }
            public EntityData DestinationE { get; set; }
            public string Destination { get; set; }
            public int Count { get; set; }
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

                var moveInfos = new List<ItemMoveInfo>();

                var uniqueNames = structure.GetUniqueNames(namesSearch);
                item.Source
                    .ForEach(S => {
                        using(var locked = new DeviceLock(EmpyrionScripting.ModApi.Playfield, S.E.Id, S.Position)) {
                            if (!locked.Success) return;

                            var count = S.Count;
                            count -= S.Container.RemoveItems(S.Id, count);
                            if(count > 0) uniqueNames.Values
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

                if(moveInfos.Count == 0) options.Inverse (output, context as object);
                else                     moveInfos.ForEach(I => options.Template(output, I));
            }
            catch (Exception error)
            {
                output.Write("{{move}} error " + error.ToString());
            }
        }

        private static int MoveItem(ItemsSource S, string N, StructureData structure, int count, int? maxLimit)
        {
            var target = structure.GetCurrent().GetDevice<Eleon.Modding.IContainer>(N);
            if (target == null) return count;

            if(!structure.ContainerSource.TryGetValue(N, out var targetData)) return count;

            using (var locked = new DeviceLock(EmpyrionScripting.ModApi.Playfield, S.E.Id, targetData.Position))
            {
                if (!locked.Success) return count;

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
