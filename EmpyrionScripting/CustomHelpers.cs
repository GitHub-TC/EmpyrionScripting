using Eleon.Modding;
using HandlebarsDotNet;
using System;
using System.Collections.Concurrent;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;

namespace EmpyrionScripting
{
    public static class CustomHelpers
    {
        public static readonly HandlebarsBlockHelper TestBlockHelper = (TextWriter output, HelperOptions options, dynamic context, object[] arguments) => {
            if (arguments.Length != 3) throw new HandlebarsException("{{test}} helper must have exactly three argument: (testvalue) 'eq'|'le'|'leq'|'ge'|'geq'|'in' (compareto)");

            try
            {
                var left    = arguments[0];
                var op      = HandlebarsUtils.IsUndefinedBindingResult(arguments[1]) 
                                ? arguments[1].GetType().GetField("Value").GetValue(arguments[1])
                                : arguments[1] as string;
                var right   = arguments[2];

                var renderTemplate = false;

                switch (op)
                {
                    case "eq" : renderTemplate = Compare(left, right) == 0; break;
                    case "le" : renderTemplate = Compare(left, right) <  0; break;
                    case "leq": renderTemplate = Compare(left, right) <= 0; break;
                    case "ge" : renderTemplate = Compare(left, right) >  0; break;
                    case "geq": renderTemplate = Compare(left, right) >= 0; break;
                    case "in" : renderTemplate = In(left, right); break;
                }

                if (renderTemplate) options.Template(output, context as object);
                else                options.Inverse (output, context as object);
            }
            catch (Exception error)
            {
                throw new HandlebarsException($"{{test}} [{arguments?.Aggregate(string.Empty, (s, a) => s + $"{a}")}]:{error.Message}");
            }
        };

        private static bool In(object left, object right)
        {
            var list = right.ToString().Split(',').Select(T => T.Trim());
            var Converter = TypeDescriptor.GetConverter(left.GetType());

            return list.Any(I =>
            {
                var rangeDelimiter = I.IndexOf('-', 1);
                return rangeDelimiter == -1
                    ? ((IComparable)left).CompareTo((IComparable)Converter.ConvertFromString(I)) == 0
                    : ((IComparable)left).CompareTo((IComparable)Converter.ConvertFromString(I.Substring(0, rangeDelimiter))) >= 0 &&
                      ((IComparable)left).CompareTo((IComparable)Converter.ConvertFromString(I.Substring(rangeDelimiter + 1))) <= 0;
            });
        }

        private static int Compare(object left, object right)
        {
            var r = TypeDescriptor.GetConverter(left.GetType()).ConvertFromString(right.ToString());
            return ((IComparable)left).CompareTo((IComparable)r);
        }

        public static readonly HandlebarsHelper DateTimeHelper = (TextWriter output, dynamic context, object[] arguments) =>
        {
            var format = arguments.Length > 0 ? arguments[0] as string : null;
            var add    = arguments.Length > 1 ? arguments[1] as string : null;

            try
            {
                var current = string.IsNullOrEmpty(add)
                        ? DateTime.Now
                        : DateTime.UtcNow.AddHours(int.Parse(add));

                if (string.IsNullOrEmpty(format)) output.Write(current);
                else                              output.Write(current.ToString(format));
            }
            catch (Exception error)
            {
                output.Write("{{datetime}} error " + error.Message);
            }
        };

        public static readonly HandlebarsHelper I18NHelper = (TextWriter output, dynamic context, object[] arguments) =>
        {
            if (arguments.Length != 2) throw new HandlebarsException("{{i18n}} helper must have exactly two argument: (key|id) (language)");

            var data     = arguments.Length > 0 ? arguments[0] as string : null;
            var language = arguments.Length > 1 ? arguments[1] as string : null;

            try
            {
                if (int.TryParse(data, out int itemId) &&
                   EmpyrionScripting.ItemInfos.ItemInfo.TryGetValue(itemId, out ItemInfo details)) data = details.Key;
                
                output.Write(EmpyrionScripting.Localization.GetName(data, language));
            }
            catch (Exception error)
            {
                output.Write("{{i18n}} error " + error.Message);
            }
        };

        public static readonly HandlebarsHelper ColorHelper = (TextWriter output, dynamic context, object[] arguments) =>
        {
            if (arguments.Length != 2) throw new HandlebarsException("{{color}} helper must have exactly two argument: '@root (rgb hex)'");

            try
            {
                var root = arguments[0] as ScriptRootData;
                int.TryParse(arguments[1] as string, NumberStyles.HexNumber, null, out int color);
                root.Color = new Color((color & 0xff0000) >> 16, (color & 0x00ff00) >> 8, color & 0x0000ff);
            }
            catch (Exception error)
            {
                output.Write("{{color}} error " + error.Message);
            }
        };

        public static readonly HandlebarsHelper BGColorHelper = (TextWriter output, dynamic context, object[] arguments) =>
        {
            if (arguments.Length != 1) throw new HandlebarsException("{{bgcolor}} helper must have exactly one argument: '@root (rgb hex)'");

            try
            {
                var root = arguments[0] as ScriptRootData;
                int.TryParse(arguments[1] as string, NumberStyles.HexNumber, null, out int color);
                root.BackgroundColor = new Color((color & 0xff0000) >> 16, (color & 0x00ff00) >> 8, color & 0x0000ff);
            }
            catch (Exception error)
            {
                output.Write("{{bgcolor}} error " + error.Message);
            }
        };

        public static readonly HandlebarsHelper FontSizeHelper = (TextWriter output, dynamic context, object[] arguments) =>
        {
            if (arguments.Length != 2) throw new HandlebarsException("{{fontsize}} helper must have exactly one argument: @root (number)");

            try
            {
                var root = arguments[0] as ScriptRootData;
                int.TryParse(arguments[1] as string, out int fontSize);
                root.FontSize = fontSize;
            }
            catch (Exception error)
            {
                output.Write("{{fontsize}} error " + error.Message);
            }
        };

        public static readonly HandlebarsBlockHelper IntervallBlockHelper = (TextWriter output, HelperOptions options, dynamic context, object[] arguments) => 
        {
            if (arguments.Length != 1) throw new HandlebarsException("{{intervall seconds}} helper must have exactly one argument: (value)");

            double.TryParse(arguments[0] as string, out double intervall);

            try
            {
                if((TimeSpan.FromTicks(DateTime.Now.Ticks).TotalSeconds % (2 * intervall)) < intervall) options.Template(output, context as object);
                else                                                                                    options.Inverse (output, context as object);
            }
            catch (Exception error)
            {
                output.Write("{{intervall}} error " + error.Message);
            }
        };

        public static readonly HandlebarsBlockHelper DeviceBlockHelper = (TextWriter output, HelperOptions options, dynamic context, object[] arguments) =>
        {
            if (arguments.Length != 2) throw new HandlebarsException("{{device entity names}} helper must have exactly two argument: (entity) (name;name*;name)");

            var entity =  arguments[0] as EntityData;
            var names  = (arguments[1] as string).Split(';');

            try
            {
                var devices = names.Select(N => entity.GetCurrent().Structure.GetDevice<IDevice>(N)).ToArray();
                if(devices.Length > 0)  options.Template(output, devices);
                else                    options.Inverse (output, context as object);
            }
            catch (Exception error)
            {
                output.Write("{{device}} error " + error.Message);
            }
        };

        public static readonly HandlebarsBlockHelper ItemsBlockHelper = (TextWriter output, HelperOptions options, dynamic context, object[] arguments) =>
        {
            if (arguments.Length != 2) throw new HandlebarsException("{{items entity names}} helper must have exactly two argument: (entity) (name;name*;name)");

            var entity = arguments[0] as EntityData;
            var names = (arguments[1] as string).Split(';').Select(N => N.Trim());

            try
            {
                var container = names.Select(N =>
                {
                    try{ return entity.GetCurrent().Structure.GetDevice<Eleon.Modding.IContainer>(N); }
                    catch { return null; }
                }).Where(C => C != null).ToArray();

                var allItems = new ConcurrentDictionary<int, ItemsData>();
                container?
                    .AsParallel()
                    .ForAll(C => C
                        .GetContent()
                        .AsParallel()
                        .ForAll(I => {
                            EmpyrionScripting.ItemInfos.ItemInfo.TryGetValue(I.id, out ItemInfo details);
                            allItems.AddOrUpdate(I.id,
                            new ItemsData()
                            {
                                Source = new[] { C }.ToList(),
                                Id     = I.id,
                                Count  = I.count,
                                Key    = details == null ? I.id.ToString() : details.Key,
                                Name   = details == null ? I.id.ToString() : details.Name,
                            },
                            (K, U) => U.AddCount(I.count, C));
                    }));


                if (allItems.Count > 0) allItems.Values.OrderBy(I => I.Id).ForEach(I => options.Template(output, I));
                else                    options.Inverse (output, context as object);
            }
            catch (Exception error)
            {
                output.Write("{{items}} error " + error.Message);
            }
        };

        public static readonly HandlebarsBlockHelper ItemListBlockHelper = (TextWriter output, HelperOptions options, dynamic context, object[] arguments) =>
        {
            if (arguments.Length != 2) throw new HandlebarsException("{{itemlist list ids}} helper must have exactly two argument: (list) (id1;id2;id3)");

            var items = arguments[0] as ItemsData[];
            var ids   = (arguments[1] as string).Split(';').Select(N => int.TryParse(N, out int i) ? i : 0).Where(i => i != 0).ToArray();

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
                output.Write("{{itemlist}} error " + error.Message);
            }
        };

        public static readonly HandlebarsBlockHelper ScrollBlockHelper = (TextWriter output, HelperOptions options, dynamic context, object[] arguments) =>
        {
            if (arguments.Length != 2) throw new HandlebarsException("{{scroll lines delay}} helper must have exactly two argument: (lines) (delay)");

            int.TryParse(arguments[0] as string, out int lines);
            int.TryParse(arguments[1] as string, out int delay);

            try
            {
                var content = new StringWriter();
                options.Template(content, context as object);
                var textlines = content.ToString().Split('\n');
                var overlapp = textlines.Length - lines;
                if(overlapp <= 0)
                {
                    output.Write(content.ToString());
                }
                else
                {
                    var skip = (TimeSpan.FromTicks(DateTime.Now.Ticks).TotalSeconds % (delay * overlapp)) / delay;
                    output.Write(string.Join("\n", textlines.Skip((int)skip).Take(lines)));
                }
            }
            catch (Exception error)
            {
                output.Write("{{scroll}} error " + error.Message);
            }
        };


    }
}
