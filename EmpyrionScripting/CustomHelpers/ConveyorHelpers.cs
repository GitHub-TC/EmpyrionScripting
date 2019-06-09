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
            if (arguments.Length != 3) throw new HandlebarsException("{{move item structure names}} helper must have exactly three argument: (item) (structure) (name;name*;*;name)");

            try
            {
                var item        = arguments[0] as ItemsData;
                var structure   = arguments[1] as StructureData;
                var namesSearch = arguments[2] as string;

                var moveInfos = new List<ItemMoveInfo>();

                var uniqueNames = structure.GetUniqueNames(namesSearch);
                item.Source
                    .ForEach(S => {
                        var count = S.Count;
                        count -= S.Container.RemoveItems(S.Id, count);
                        if(count > 0) uniqueNames.Values
                                        .Where(N => N != S.CustomName)
                                        .ForEach(N => {
                                            var startCount = count;
                                            count = MoveItem(S, N, structure, count);
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
                    });

                if(moveInfos.Count == 0) options.Inverse (output, context as object);
                else                     moveInfos.ForEach(I => options.Template(output, I));
            }
            catch (Exception error)
            {
                output.Write("{{move}} error " + error.Message);
            }
        }

        private static int MoveItem(ItemsSource S, string N, StructureData structure, int count)
        {
            var target = structure.GetCurrent().GetDevice<Eleon.Modding.IContainer>(N);
            return target == null ? count : target.AddItems(S.Id, count);
        }

    }
}
