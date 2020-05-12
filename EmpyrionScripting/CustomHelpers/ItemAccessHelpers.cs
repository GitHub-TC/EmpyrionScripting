using EcfParser;
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
    public static class ItemAccessHelpers
    {
        [HandlebarTag("configattr")]
        public static void ConfigAttrHelper(TextWriter output, object rootObject, dynamic context, object[] arguments)
        {
            if (arguments.Length != 2) throw new HandlebarsException("{{configattr id attrname}} helper must have exactly two argument: (id) (attrname)");

            var root    = rootObject as IScriptModData;
            int.TryParse(arguments[0]?.ToString(), out var id);
            var name    = arguments[1]?.ToString();

            try{ output.Write(root.ConfigEcfAccess.FindAttribute(id, name)); }
            catch (Exception error) { output.Write("{{configattr}} error " + EmpyrionScripting.ErrorFilter(error)); }
        }

        [HandlebarTag("configbyid")]
        public static void ConfigByIdHelper(TextWriter output, object rootObject, HelperOptions options, dynamic context, object[] arguments)
        {
            if (arguments.Length != 1) throw new HandlebarsException("{{configbyid id}} helper must have exactly one argument: (id)");

            var root = rootObject as IScriptModData;
            int.TryParse(arguments[0]?.ToString(), out var id);

            try {
                if (root.ConfigEcfAccess.FlatConfigBlockById.TryGetValue(id, out var config)) options.Template(output, config);
                else                                                                          options.Inverse (output, (object)context);
            }
            catch (Exception error) { output.Write("{{configattr}} error " + EmpyrionScripting.ErrorFilter(error)); }
        }

        [HandlebarTag("configbyname")]
        public static void ConfigByNameHelper(TextWriter output, object rootObject, HelperOptions options, dynamic context, object[] arguments)
        {
            if (arguments.Length != 1) throw new HandlebarsException("{{configbyname name}} helper must have exactly one argument: (name)");

            var root = rootObject as IScriptModData;

            try {
                if (root.ConfigEcfAccess.FlatConfigBlockByName.TryGetValue(arguments[0]?.ToString(), out var config)) options.Template(output, config);
                else                                                                                                  options.Inverse (output, (object)context);
            }
            catch (Exception error) { output.Write("{{configattr}} error " + EmpyrionScripting.ErrorFilter(error)); }
        }

        public static object FindAttribute(this IConfigEcfAccess ecf, int id, string name) =>
            ecf.FlatConfigBlockById.TryGetValue(id, out var found)
                ? found.Attr.FirstOrDefault(A => A.Name == name)?.Value
                : null;

        [HandlebarTag("items")]
        public static void ItemsBlockHelper(TextWriter output, object root, HelperOptions options, dynamic context, object[] arguments)
        {
            if (arguments.Length != 2) throw new HandlebarsException("{{items structure names}} helper must have exactly two argument: (structure) (name;name*;*;name)");

            var structure   = arguments[0] as IStructureData;
            var namesSearch = arguments[1]?.ToString();

            try
            {
                var allItems = Items(structure, namesSearch);

                if (allItems.Length > 0) allItems.ForEach(I => options.Template(output, I));
                else                     options.Inverse(output, context as object);
            }
            catch (Exception error)
            {
                output.Write("{{items}} error " + EmpyrionScripting.ErrorFilter(error));
            }
        }

        [HandlebarTag("getitems")]
        public static void GetItemsBlockHelper(TextWriter output, object root, HelperOptions options, dynamic context, object[] arguments)
        {
            if (arguments.Length != 2) throw new HandlebarsException("{{getitems structure names}} helper must have exactly two argument: (structure) (name;name*;*;name)");

            var structure = arguments[0] as IStructureData;
            var namesSearch = arguments[1]?.ToString();

            try
            {
                var allItems = Items(structure, namesSearch);

                if (allItems.Length > 0) options.Template(output, allItems);
                else                     options.Inverse(output, context as object);
            }
            catch (Exception error)
            {
                output.Write("{{getitems}} error " + EmpyrionScripting.ErrorFilter(error));
            }
        }

        public static ItemsData[] Items(IStructureData structure, string names)
        {
            var uniqueNames = structure.AllCustomDeviceNames.GetUniqueNames(names);

            var allItems = new ConcurrentDictionary<int, ItemsData>();
            structure.Items
                .SelectMany(I => I.Source.Where(S => S.CustomName != null && uniqueNames.Contains(S.CustomName)))
                .ForEach(I =>
                {
                    ItemInfo details = null;
                    EmpyrionScripting.ItemInfos?.ItemInfo.TryGetValue(I.Id, out details);
                    allItems.AddOrUpdate(I.Id,
                    new ItemsData()
                    {
                        Source  = new[] { I }.ToList(),
                        Id      = I.Id,
                        Count   = I.Count,
                        Key     = details == null ? I.Id.ToString() : details.Key,
                        Name    = details == null ? I.Id.ToString() : details.Name,
                    },
                    (K, U) => U.AddCount(I.Count, I));
                });

            return allItems.Values.OrderBy(I => I.Id).ToArray();
        }

        [HandlebarTag("itemlist")]
        public static void ItemListBlockHelper(TextWriter output, object root, HelperOptions options, dynamic context, object[] arguments)
        {
            if (arguments.Length != 2) throw new HandlebarsException("{{itemlist list ids}} helper must have exactly two argument: (list) (id1;id2;id3)");

            var items = arguments[0] as ItemsData[];
            var ids   = (arguments[1] as string)
                            .Split(';', ',')
                            .Select(N => int.TryParse(N, out int i) ? i : 0)
                            .Where(i => i != 0)
                            .ToArray();

            try
            {
                var list = items.ToDictionary(I => I.Id, I => I);
                ids.Where(i => !list.ContainsKey(i)).ForEach(i => {
                    EmpyrionScripting.ItemInfos.ItemInfo.TryGetValue(i, out ItemInfo details);
                    list.Add(i, new ItemsData()
                    {
                        Id    = i,
                        Count = 0,
                        Key   = details == null ? i.ToString() : details.Key,
                        Name  = details == null ? i.ToString() : details.Name,
                    });
                });

                if (list.Count > 0) list.Values
                                        .Where(i => ids.Contains(i.Id))
                                        .OrderBy(I => I.Id)
                                        .ForEach(I => options.Template(output, I));
                else                options.Inverse (output, context as object);
            }
            catch (Exception error)
            {
                output.Write("{{itemlist}} error " + EmpyrionScripting.ErrorFilter(error));
            }
        }

    }
}
