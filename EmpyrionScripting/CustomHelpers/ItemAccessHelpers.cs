using EmpyrionScripting.CsHelper;
using EmpyrionScripting.DataWrapper;
using EmpyrionScripting.Interface;
using EmpyrionScripting.Internal.Interface;
using HandlebarsDotNet;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;

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
            catch (Exception error) {
                if (!CsScriptFunctions.FunctionNeedsMainThread(error, root)) output.Write("{{configattr}} error " + EmpyrionScripting.ErrorFilter(error)); 
            }
        }

        [HandlebarTag("configattrbyname")]
        public static void ConfigAttrByNameHelper(TextWriter output, object rootObject, dynamic context, object[] arguments)
        {
            if (arguments.Length != 2) throw new HandlebarsException("{{configattrbyname name attrname}} helper must have exactly two argument: (name) (attrname)");

            var root = rootObject as IScriptModData;

            try {
                root.ConfigEcfAccess.FlatConfigBlockByName.TryGetValue(arguments[0]?.ToString(), out var config);
                var id = (int)config.Attr.FirstOrDefault(A => A.Name == "Id").Value;

                output.Write(root.ConfigEcfAccess.FindAttribute(id, arguments[1]?.ToString())); 
            }
            catch (Exception error)
            {
                if (!CsScriptFunctions.FunctionNeedsMainThread(error, root)) output.Write("{{configattrbyname}} error " + EmpyrionScripting.ErrorFilter(error));
            }
        }

        [HandlebarTag("configid")]
        public static void ConfigIdHelper(TextWriter output, object rootObject, dynamic context, object[] arguments)
        {
            if (arguments.Length != 1) throw new HandlebarsException("{{configid name}} helper must have exactly one argument: (name) ");

            var root = rootObject as IScriptModData;

            try
            {
                root.ConfigEcfAccess.FlatConfigBlockByName.TryGetValue(arguments[0]?.ToString(), out var config);
                output.Write(config.Attr?.FirstOrDefault(A => A.Name == "Id")?.Value);
            }
            catch (Exception error)
            {
                if (!CsScriptFunctions.FunctionNeedsMainThread(error, root)) output.Write("{{configattrbyname}} error " + EmpyrionScripting.ErrorFilter(error));
            }
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
            catch (Exception error) {
                if (!CsScriptFunctions.FunctionNeedsMainThread(error, root)) output.Write("{{configattr}} error " + EmpyrionScripting.ErrorFilter(error)); 
            }
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
            catch (Exception error) {
                if (!CsScriptFunctions.FunctionNeedsMainThread(error, root)) output.Write("{{configattr}} error " + EmpyrionScripting.ErrorFilter(error)); 
            }
        }

        [HandlebarTag("resourcesforid")]
        public static void ResourcesForBlockByIdHelper(TextWriter output, object rootObject, HelperOptions options, dynamic context, object[] arguments)
        {
            if (arguments.Length != 1) throw new HandlebarsException("{{resourcesforid id}} helper must have exactly one argument: (id)");

            var root = rootObject as IScriptModData;
            int.TryParse(arguments[0]?.ToString(), out var id);

            try {
                if (root.ConfigEcfAccess.ResourcesForBlockById.TryGetValue(id, out var recipe)) options.Template(output, recipe);
                else                                                                            options.Inverse (output, (object)context);
            }
            catch (Exception error) {
                if (!CsScriptFunctions.FunctionNeedsMainThread(error, root)) output.Write("{{resourcesforid}} error " + EmpyrionScripting.ErrorFilter(error)); 
            }
        }


        public static object FindAttribute(this IConfigEcfAccess ecf, int id, string name) =>
            ecf.FlatConfigBlockById.TryGetValue(id, out var found)
                ? (found.Values.TryGetValue(name, out var data) ? data : null)
                : null;

        [HandlebarTag("items")]
        public static void ItemsBlockHelper(TextWriter output, object rootObject, HelperOptions options, dynamic context, object[] arguments)
        {
            if (arguments.Length != 2) throw new HandlebarsException("{{items structure names}} helper must have exactly two argument: (structure) (name;name*;*;name)");

            var root        = rootObject as IScriptRootData;
            var structure   = arguments[0] as IStructureData;
            var namesSearch = arguments[1]?.ToString();

            try
            {
                var allItems = Items(root, structure, namesSearch);

                if (allItems.Length > 0) allItems.ForEach(item => options.Template(output, item), () => root.TimeLimitReached);
                else                     options.Inverse(output, context as object);
            }
            catch (Exception error)
            {
                if (!CsScriptFunctions.FunctionNeedsMainThread(error, root)) output.Write("{{items}} error " + EmpyrionScripting.ErrorFilter(error));
            }
        }

        [HandlebarTag("getitems")]
        public static void GetItemsBlockHelper(TextWriter output, object rootObject, HelperOptions options, dynamic context, object[] arguments)
        {
            if (arguments.Length != 2) throw new HandlebarsException("{{getitems structure names}} helper must have exactly two argument: (structure) (name;name*;*;name)");

            var root        = rootObject as IScriptRootData;
            var structure   = arguments[0] as IStructureData;
            var namesSearch = arguments[1]?.ToString();

            try
            {
                var allItems = Items(root, structure, namesSearch);

                if (allItems.Length > 0) options.Template(output, allItems);
                else                     options.Inverse(output, context as object);
            }
            catch (Exception error)
            {
                if (!CsScriptFunctions.FunctionNeedsMainThread(error, root)) output.Write("{{getitems}} error " + EmpyrionScripting.ErrorFilter(error));
            }
        }

        public static ItemsData[] Items(IScriptRootData root, IStructureData structure, string names)
        {
            if (root.TimeLimitReached) return Array.Empty<ItemsData>();

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
        public static void ItemListBlockHelper(TextWriter output, object rootObject, HelperOptions options, dynamic context, object[] arguments)
        {
            if (arguments.Length != 2) throw new HandlebarsException("{{itemlist list ids}} helper must have exactly two argument: (list) (id1;id2;id3)");

            var root  = rootObject as IScriptRootData;
            var items = arguments[0] as ItemsData[];
            var idsList = (arguments[1] as string)
                            .Split(new[] { ';', ',' }, StringSplitOptions.RemoveEmptyEntries)
                            .Select(I => I.Trim())
                            .Where(I => !string.IsNullOrEmpty(I))
                            .ToArray();

            try
            {
                var ids = new List<int>();

                idsList.ForEach(I => {
                    var delimiter = I.IndexOf('-', I.StartsWith("-") ? 1 : 0);
                    if(delimiter > 0){
                        if (int.TryParse(I.Substring(0, delimiter), out int fromId) && int.TryParse(I.Substring(delimiter + 1), out var toId)) ids.AddRange(Enumerable.Range(fromId, toId - fromId + 1));
                    }
                    else if(int.TryParse(I, out var id)) ids.Add(id);
                });

                if (root.TimeLimitReached) return;

                var list = items.ToDictionary(I => I.Id, I => I);
                ids.Where(i => !list.ContainsKey(i)).ForEach(i => {
                    ItemInfo details = null;
                    EmpyrionScripting.ItemInfos?.ItemInfo?.TryGetValue(i, out details);
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
                if (!CsScriptFunctions.FunctionNeedsMainThread(error, root)) output.Write("{{itemlist}} error " + EmpyrionScripting.ErrorFilter(error));
            }
        }

    }
}
