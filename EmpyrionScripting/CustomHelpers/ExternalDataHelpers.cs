using Eleon.Modding;
using EmpyrionScripting.CsHelper;
using EmpyrionScripting.Interface;
using EmpyrionScripting.Internal.Interface;
using HandlebarsDotNet;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections.Concurrent;
using EmpyrionScripting.DataWrapper;
using Newtonsoft.Json;

namespace EmpyrionScripting.CustomHelpers
{
    [HandlebarHelpers]
    public static class ExternalDataHelpers
    {
        [HandlebarTag("datetime")]
        public static void DateTimeHelper(TextWriter output, object rootObject, dynamic context, object[] arguments)
        {
            var format = arguments.Length > 0 ? arguments[0]?.ToString() : null;
            var add    = arguments.Length > 1 ? arguments[1]?.ToString() : null;

            var root = rootObject as IScriptRootData;

            try
            {
                var current = string.IsNullOrEmpty(add)
                        ? DateTime.UtcNow.AddHours(root.CultureInfo.UTCplusTimezone)
                        : DateTime.UtcNow.AddHours(int.Parse(add));

                if (string.IsNullOrEmpty(format)) output.Write(current.ToString(        root.CultureInfo.CultureInfo));
                else                              output.Write(current.ToString(format, root.CultureInfo.CultureInfo));
            }
            catch (Exception error)
            {
                if (!CsScriptFunctions.FunctionNeedsMainThread(error, root)) output.Write("{{datetime}} error " + EmpyrionScripting.ErrorFilter(error));
            }
        }

        [HandlebarTag("random")]
        public static void RandomHelper(TextWriter output, object root, HelperOptions options, dynamic context, object[] arguments)
        {
            if (arguments.Length != 2) throw new HandlebarsException("{{random start end}} helper must have two argument: (start) (end)");

            var format = arguments.Length > 0 ? arguments[0]?.ToString() : null;
            var add    = arguments.Length > 1 ? arguments[1]?.ToString() : null;

            try
            {
                int.TryParse(arguments[0]?.ToString(), out var start);
                int.TryParse(arguments[1]?.ToString(), out var end);

                options.Template(output, new Random().Next(start, end));
            }
            catch (Exception error)
            {
                if (!CsScriptFunctions.FunctionNeedsMainThread(error, root)) output.Write("{{random}} error " + EmpyrionScripting.ErrorFilter(error));
            }
        }

        [HandlebarTag("use")]
        public static void UseHelper(TextWriter output, object root, HelperOptions options, dynamic context, object[] arguments)
        {
            if (arguments.Length != 1) throw new HandlebarsException("{{use data}} helper must have one argument: (data)");

            try
            {
                if (arguments[0] == null) options.Inverse (output, context as object);
                else                      options.Template(output, arguments[0]);
            }
            catch (Exception error)
            {
                if (!CsScriptFunctions.FunctionNeedsMainThread(error, root)) output.Write("{{use}} error " + EmpyrionScripting.ErrorFilter(error));
            }
        }

        [HandlebarTag("set")]
        public static void SetHelper(TextWriter output, object rootObject, dynamic context, object[] arguments)
        {
            if (arguments.Length != 2 && arguments.Length != 3) throw new HandlebarsException("{{set key data}} helper must have two argument: (key) (data) | (array) (value) | (dictionary) (key) (value)");

            var root = rootObject as IScriptRootData;
            try
            {
                if      (arguments[0] is ConcurrentDictionary<string, object> dictionary) dictionary.AddOrUpdate(arguments[1].ToString(), arguments[2], (k, o) => arguments[2]);
                else if (arguments[0] is List<object>                         list      ) list.Add(arguments[1]);
                else root.Data.AddOrUpdate(arguments[0]?.ToString(), arguments[1], (S, O) => arguments[1]);
            }
            catch (Exception error)
            {
                if (!CsScriptFunctions.FunctionNeedsMainThread(error, root)) output.Write("{{set}} error " + EmpyrionScripting.ErrorFilter(error));
            }
        }

        [HandlebarTag("setcache")]
        public static void SetCacheHelper(TextWriter output, object rootObject, dynamic context, object[] arguments)
        {
            if (arguments.Length != 2) throw new HandlebarsException("{{setcache key data}} helper must have two argument: (key) (data)");

            var root = rootObject as IScriptRootData;
            try
            {
                root.CacheData.AddOrUpdate(arguments[0]?.ToString(), arguments[1], (S, O) => arguments[1]);
            }
            catch (Exception error)
            {
                if (!CsScriptFunctions.FunctionNeedsMainThread(error, root)) output.Write("{{setcache}} error " + EmpyrionScripting.ErrorFilter(error));
            }
        }

        [HandlebarTag("createdictionary")]
        public static void CreateDictionaryHelper(TextWriter output, object root, HelperOptions options, dynamic context, object[] arguments)
        {
            if (arguments.Length != 0) throw new HandlebarsException("{{createdictionary}} helper must have none argument");

            try
            {
                options.Template(output, new ConcurrentDictionary<string, object>());
            }
            catch (Exception error)
            {
                if (!CsScriptFunctions.FunctionNeedsMainThread(error, root)) output.Write("{{createdictionary}} error " + EmpyrionScripting.ErrorFilter(error));
            }
        }

        [HandlebarTag("removekey")]
        public static void RremoveKeyValueHelper(TextWriter output, object rootObject, dynamic context, object[] arguments)
        {
            if (arguments.Length != 3) throw new HandlebarsException("{{removekey}} helper must have three argument: dictionary key value");

            try
            {
                var dictionary = arguments[0] as Dictionary<string, object>;
                dictionary.Remove(arguments[1].ToString());
            }
            catch (Exception error)
            {
                if (!CsScriptFunctions.FunctionNeedsMainThread(error, rootObject)) output.Write("{{removekey}} error " + EmpyrionScripting.ErrorFilter(error));
            }
        }

        [HandlebarTag("createarray")]
        public static void CreateArrayHelper(TextWriter output, object root, HelperOptions options, dynamic context, object[] arguments)
        {
            if (arguments.Length != 0) throw new HandlebarsException("{{createarray}} helper must have none argument");

            try
            {
                options.Template(output, new List<object>());
            }
            catch (Exception error)
            {
                if (!CsScriptFunctions.FunctionNeedsMainThread(error, root)) output.Write("{{createarray}} error " + EmpyrionScripting.ErrorFilter(error));
            }
        }

        [HandlebarTag("removeitem")]
        public static void RemoveItemHelper(TextWriter output, object rootObject, dynamic context, object[] arguments)
        {
            if (arguments.Length != 2) throw new HandlebarsException("{{removeitem}} helper must have three argument: list value");

            try
            {
                var list = arguments[0] as List<object>;
                list.Remove(arguments[1]);
            }
            catch (Exception error)
            {
                if (!CsScriptFunctions.FunctionNeedsMainThread(error, rootObject)) output.Write("{{aremoveitemdditem}} error " + EmpyrionScripting.ErrorFilter(error));
            }
        }

        [HandlebarTag("lookup")]
        public static void LookUpHelper(TextWriter output, object rootObject, dynamic context, object[] arguments)
        {
            if (arguments.Length != 2) throw new HandlebarsException("{{lookup array/dictionary index/key}} helper must have two argument: (array/dictionary) (index/key)");

            var root = rootObject as IScriptRootData;
            try
            {
                if      (arguments[0] is GroupCollection collectiondata) { if (int.TryParse(arguments[1].ToString(), out var index)) output.Write(collectiondata[index]?.Value); else output.Write(collectiondata[$"{arguments[1]}"]?.Value); }
                else if (arguments[0] is object[] arraydata)             { if (int.TryParse(arguments[1].ToString(), out var index)) output.Write(arraydata.Skip(index).FirstOrDefault()); }
                else if (arguments[0] is IList listdata)                 { if (int.TryParse(arguments[1].ToString(), out var index)) output.Write(listdata[index]); }
                else { 
                    var dictionaryType = arguments[0]?.GetType();
                    if (dictionaryType != null && dictionaryType.IsGenericType)
                    {
                        var tryGetValue = dictionaryType.GetMethod("TryGetValue");
                        if (tryGetValue == null) return;

                        object[] parameters = new object[] { arguments[1], null };
                        if ((bool)tryGetValue.Invoke(arguments[0], parameters)) output.Write(parameters[1]);
                    }
                }
            }
            catch (Exception error)
            {
                if (!CsScriptFunctions.FunctionNeedsMainThread(error, root)) output.Write("{{lookup}} error " + EmpyrionScripting.ErrorFilter(error));
            }
        }

        [HandlebarTag("lookupblock")]
        public static void LookUpBlockHelper(TextWriter output, object rootObject, HelperOptions options, dynamic context, object[] arguments)
        {
            if (arguments.Length != 2) throw new HandlebarsException("{{lookupblock array index}} helper must have two argument: (array) (index)");

            var root = rootObject as IScriptRootData;
            try
            {
                if      (arguments[0] is GroupCollection collectiondata) { if (int.TryParse(arguments[1].ToString(), out var index)) options.Template(output, collectiondata[index]); else options.Template(output, collectiondata[$"{arguments[1]}"]); }
                else if (arguments[0] is object[] arraydata)             { if (int.TryParse(arguments[1].ToString(), out var index)) options.Template(output, arraydata.Skip(index).FirstOrDefault()); }
                else if (arguments[0] is IList    listdata)              { if (int.TryParse(arguments[1].ToString(), out var index)) options.Template(output, listdata[index]); }
                else
                {
                    var dictionaryType = arguments[0]?.GetType();
                    if (dictionaryType != null && dictionaryType.IsGenericType)
                    {
                        var tryGetValue = dictionaryType.GetMethod("TryGetValue");
                        if (tryGetValue == null) return;

                        object[] parameters = new object[] { arguments[1], null };
                        if ((bool)tryGetValue.Invoke(arguments[0], parameters)) options.Template(output, parameters[1]);
                        else                                                    options.Inverse(output, (object)context);
                    }
                    else options.Inverse (output, (object)context);
                }
            }
            catch (Exception error)
            {
                if (!CsScriptFunctions.FunctionNeedsMainThread(error, root)) output.Write("{{lookupblock}} error " + EmpyrionScripting.ErrorFilter(error));
            }
        }

        [HandlebarTag("concatarrays")]
        public static void ConcatArraysHelper(TextWriter output, object rootObject, HelperOptions options, dynamic context, object[] arguments)
        {
            if (arguments.Length == 0) throw new HandlebarsException("{{concatarrays (array1,array2,array3,...)}} helper must have one argument: (array1) [(array2) ...]");

            var root = rootObject as IScriptRootData;
            try
            {
                List<object>               resultArray      = null;
                Dictionary<object, object> resultDictionary = null;

                arguments.Where(arg => arg != null).ForEach(arg =>
                {
                    if (arg is object[] arraydata) (resultArray ??= new List<object>()).AddRange(arraydata);
                    else if (arg is IList listdata)
                    {
                        if (resultArray == null) resultArray = new List<object>();
                        for (int i = 0; i < listdata.Count; i++) resultArray.Add(listdata[i]);
                    }
                    else if (arg is IDictionary dictdata)
                    {
                        if (resultDictionary == null) resultDictionary = new Dictionary<object, object>();
                        foreach (var key in dictdata.Keys)
                        {
                            if (resultDictionary.ContainsKey(key)) resultDictionary[key] = dictdata[key];
                            else                                   resultDictionary.Add(key, dictdata[key]);
                        }
                    }
                    else (resultArray ??= new List<object>()).Add(arg);
                });

                if      (resultArray        != null && resultDictionary != null) output.Write("{{concatarrays}} error can't combine arrays with dictionaries");
                else if (resultArray        != null) options.Template(output, resultArray);
                else if (resultDictionary   != null) options.Template(output, resultDictionary);
                else                                 options.Inverse(output, (object)context);
            }
            catch (Exception error)
            {
                if (!CsScriptFunctions.FunctionNeedsMainThread(error, root)) output.Write("{{concatarrays}} error " + EmpyrionScripting.ErrorFilter(error));
            }
        }


        [HandlebarTag("setblock")]
        public static void SetBlockHelper(TextWriter output, object rootObject, HelperOptions options, dynamic context, object[] arguments)
        {
            if (arguments.Length != 1 && arguments.Length != 2) throw new HandlebarsException("{{setblock key}} helper must have one argument: (key|array) | (dictionary) (key)");

            var root = rootObject as IScriptRootData;
            try
            {
                var data = new StringWriter();
                options.Template(data, context as object);

                if      (arguments[0] is ConcurrentDictionary<string, object> dictionary) dictionary.AddOrUpdate(arguments[1].ToString(), data.ToString(), (k, o) => data.ToString());
                else if (arguments[0] is List<object>                         list      ) list.Add(data.ToString());
                else root.Data.AddOrUpdate(arguments[0]?.ToString(), data.ToString(), (S, O) => data.ToString());
            }
            catch (Exception error)
            {
                if (!CsScriptFunctions.FunctionNeedsMainThread(error, root)) output.Write("{{setblock}} error " + EmpyrionScripting.ErrorFilter(error));
            }
        }

        [HandlebarTag("setcacheblock")]
        public static void SetCacheBlockHelper(TextWriter output, object rootObject, HelperOptions options, dynamic context, object[] arguments)
        {
            if (arguments.Length != 1) throw new HandlebarsException("{{setcacheblock key}} helper must have one argument: (key)");

            var root = rootObject as IScriptRootData;
            try
            {
                var data = new StringWriter();
                options.Template(data, context as object);

                root.CacheData.AddOrUpdate(arguments[0]?.ToString(), data.ToString(), (S, O) => data.ToString());
            }
            catch (Exception error)
            {
                if (!CsScriptFunctions.FunctionNeedsMainThread(error, root)) output.Write("{{setcacheblock}} error " + EmpyrionScripting.ErrorFilter(error));
            }
        }

        [HandlebarTag("split")]
        public static void SplitHelper(TextWriter output, object root, HelperOptions options, dynamic context, object[] arguments)
        {
            if (arguments.Length < 2) throw new HandlebarsException("{{split string separator [removeemptyentries] [trimchars]}} helper must have at least two argument: (string) (separator) [true|false] [trimchars]");

            var data      =                arguments[0].ToString();
            var separator = Regex.Unescape(arguments[1].ToString());
            var trimchars = arguments.Get(3)?.ToString().ToCharArray();

            try
            {
                bool.TryParse(arguments.Length > 2 ? arguments[2]?.ToString() : null, out var removeemptyentries);

                options.Template(output, 
                    data.Split(new[] { separator },
                                removeemptyentries ? StringSplitOptions.RemoveEmptyEntries : StringSplitOptions.None)
                    .Select(item => trimchars == null ? item : item.Trim(trimchars))
                    .ToArray());
            }
            catch (Exception error)
            {
                if (!CsScriptFunctions.FunctionNeedsMainThread(error, root)) output.Write("{{split}} error " + EmpyrionScripting.ErrorFilter(error));
            }
        }

        [HandlebarTag("jsontodictionary")]
        public static void JsonToDictionary(TextWriter output, object root, HelperOptions options, dynamic context, object[] arguments)
        {
            if (arguments.Length != 1) throw new HandlebarsException("{{jsontodictionary string}} helper must have at least one argument: (string)");

            var data = arguments[0].ToString();

            try
            {
                options.Template(output, JsonConvert.DeserializeObject<Dictionary<object, object>>(data));
            }
            catch (Exception error)
            {
                if (!CsScriptFunctions.FunctionNeedsMainThread(error, root)) output.Write("{{jsontodictionary}} error " + EmpyrionScripting.ErrorFilter(error));
            }
        }


        [HandlebarTag("fromjson")]
        public static void FromJsonHelper(TextWriter output, object root, HelperOptions options, dynamic context, object[] arguments)
        {
            if (arguments.Length != 1) throw new HandlebarsException("{{fromjson string}} helper must have at least one argument: (string)");

            var data = arguments[0].ToString();

            try
            {
                options.Template(output, JsonConvert.DeserializeObject(data));
            }
            catch (Exception error)
            {
                if (!CsScriptFunctions.FunctionNeedsMainThread(error, root)) output.Write("{{fromjson}} error " + EmpyrionScripting.ErrorFilter(error));
            }
        }

        [HandlebarTag("tojson")]
        public static void ToJsonHelper(TextWriter output, object root, dynamic context, object[] arguments)
        {
            if (arguments.Length != 1) throw new HandlebarsException("{{tojson object}} helper must have at least one argument: (object)");

            try
            {
                output.Write(JsonConvert.SerializeObject(arguments[0]));
            }
            catch (Exception error)
            {
                if (!CsScriptFunctions.FunctionNeedsMainThread(error, root)) output.Write("{{trim}} error " + EmpyrionScripting.ErrorFilter(error));
            }
        }

        [HandlebarTag("trim")]
        public static void TrimHelper(TextWriter output, object root, dynamic context, object[] arguments)
        {
            if (arguments.Length < 1) throw new HandlebarsException("{{trim string [trimchars]}} helper must have at least two argument: (string) [trimchars}");

            var data      = arguments[0].ToString();
            var trimchars = arguments.Get(1)?.ToString().ToCharArray() ?? new[] { ' ' };

            try
            {
                output.Write(data.Trim(trimchars));
            }
            catch (Exception error)
            {
                if (!CsScriptFunctions.FunctionNeedsMainThread(error, root)) output.Write("{{trim}} error " + EmpyrionScripting.ErrorFilter(error));
            }
        }

        [HandlebarTag("concat")]
        public static void ConcatHelper(TextWriter output, object root, dynamic context, object[] arguments)
        {
            if (arguments.Length < 2) throw new HandlebarsException("{{concat a1 a2 a3 ...}} helper must have at least two arguments: (a1) (a2)");

            try
            {
                var text = new StringBuilder();
                for (int i = 0; i < arguments.Length; i++)
                {
                    if      (arguments[i] is string   s) text.Append(s);
                    else if (arguments[i] is string[] a) text.Append(string.Join(Regex.Unescape(arguments[++i]?.ToString()), a));
                    else                                 text.Append(arguments[i]?.ToString());
                }

                output.Write(text.ToString());
            }
            catch (Exception error)
            {
                if (!CsScriptFunctions.FunctionNeedsMainThread(error, root)) output.Write("{{concat}} error " + EmpyrionScripting.ErrorFilter(error));
            }
        }

        [HandlebarTag("substring")]
        public static void SubstringHelper(TextWriter output, object root, dynamic context, object[] arguments)
        {
            if (arguments.Length < 2) throw new HandlebarsException("{{substring text startindex [length]}} helper must have at least two arguments: (text) (startindex)");

            var text = arguments[0]?.ToString();
            int startIndex = 0, 
                length     = 0;
            try
            {
                int.TryParse(arguments[1]?.ToString(), out startIndex);

                if(arguments.Length == 2) output.Write(text?.Substring(startIndex));
                else
                {
                    int.TryParse(arguments[2]?.ToString(), out length);
                    output.Write(text?.Substring(startIndex, Math.Min(length, text.Length - startIndex)));
                }
            }
            catch (Exception error)
            {
                if (CsScriptFunctions.FunctionNeedsMainThread(error, root)) { /* no error */ }
                else if (arguments.Length == 2) output.Write("{{substring}} error (startindex=" + startIndex + ") text='" + text + "' " + EmpyrionScripting.ErrorFilter(error));
                else                            output.Write("{{substring}} error (startindex=" + startIndex + ", length=" + length + ") text='" + text + "' " + EmpyrionScripting.ErrorFilter(error));
            }
        }

        [HandlebarTag("replace")]
        public static void ReplaceHelper(TextWriter output, object root, dynamic context, object[] arguments)
        {
            if (arguments.Length != 3) throw new HandlebarsException("{{replace text find replaceto}} helper must have at least three arguments: (text) (find) (replaceto)");

            try
            {
                output.Write(arguments[0]?.ToString().Replace(arguments[1]?.ToString(), arguments[2]?.ToString()));
            }
            catch (Exception error)
            {
                if (CsScriptFunctions.FunctionNeedsMainThread(error, root)) { /* no error */ }
                else output.Write("{{replace}} error (find=" + arguments[1] + ", replaceto=" + arguments[2] + ") text='" + arguments[0] + "' " + EmpyrionScripting.ErrorFilter(error));
            }
        }


        [HandlebarTag("startswith")]
        public static void StartsWithHelper(TextWriter output, object root, dynamic context, object[] arguments)
        {
            if (arguments.Length < 2) throw new HandlebarsException("{{startswith text starts [ignoreCase]}} helper must have at least two arguments: (text) (starts)");

            try
            {
                output.Write(arguments[0]?.ToString()?.StartsWith(arguments[1]?.ToString(),
                    !bool.TryParse(arguments[1]?.ToString(), out var ignoreCase) || ignoreCase,
                    CultureInfo.InvariantCulture));
            }
            catch (Exception error)
            {
                if (CsScriptFunctions.FunctionNeedsMainThread(error, root)) { /* no error */ }
                output.Write($"{{startswith}} error text='{arguments.Get(0)} starts='{arguments.Get(1)}' ignoreCase={arguments.Get(2) ?? false}' {EmpyrionScripting.ErrorFilter(error)}");
            }
        }

        [HandlebarTag("endsswith")]
        public static void EndsWithHelper(TextWriter output, object root, dynamic context, object[] arguments)
        {
            if (arguments.Length < 2) throw new HandlebarsException("{{endsswith text ends [ignoreCase]}} helper must have at least two arguments: (text) (ends)");

            try
            {
                output.Write(arguments[0]?.ToString()?.EndsWith(arguments[1]?.ToString(),
                    !bool.TryParse(arguments[1]?.ToString(), out var ignoreCase) || ignoreCase,
                    CultureInfo.InvariantCulture));
            }
            catch (Exception error)
            {
                if (CsScriptFunctions.FunctionNeedsMainThread(error, root)) { /* no error */ }
                output.Write($"{{endsswith}} error text='{arguments.Get(0)} ends='{arguments.Get(1)}' ignoreCase={arguments.Get(2) ?? false}' {EmpyrionScripting.ErrorFilter(error)}");
            }
        }

        [HandlebarTag("chararray")]
        public static void CharArrayHelper(TextWriter output, object root, HelperOptions options, dynamic context, object[] arguments)
        {
            if (arguments.Length != 1) throw new HandlebarsException("{{chararray text}} helper must have only one argument: (text)");

            try
            {
                options.Template(output, arguments[0]?.ToString().ToCharArray().Select(C => C.ToString()).ToArray());
            }
            catch (Exception error)
            {
                if (!CsScriptFunctions.FunctionNeedsMainThread(error, root)) output.Write("{{chararray}} error " + EmpyrionScripting.ErrorFilter(error));
            }
        }

        public class ExternalDataAccess
        {
            public IMod Instance { get; set; }
            public Func<IEntity, object[], object> Data { get; set; }
        }

        public static Lazy<ConcurrentDictionary<string, ExternalDataAccess>> ExternalDataModules { get; } = new Lazy<ConcurrentDictionary<string, ExternalDataAccess>>(ExternalDataModulesFactory);

        private static ConcurrentDictionary<string, ExternalDataAccess> ExternalDataModulesFactory()
        {
            var externalDataModules = new ConcurrentDictionary<string, ExternalDataAccess>();
            EmpyrionScripting.AddOnAssemblies.Values.ForEach(A => {
                EmpyrionScripting.Log($"AddOnAssembly: {A.FullAssemblyDllName}", EmpyrionNetAPIDefinitions.LogLevel.Message);
                A.LoadedAssembly.GetTypes().ForEach(T => {
                    if (typeof(IMod).IsAssignableFrom(T))
                    {
                        EmpyrionScripting.Log($"Found mod class: {T}", EmpyrionNetAPIDefinitions.LogLevel.Message);
                        var getDataProperty = T.GetProperty("ScriptExternalDataAccess", typeof(IDictionary<string, Func<IEntity, object[], object>>));

                        if(getDataProperty != null)
                        {
                            EmpyrionScripting.Log($"Found mod Data property: {T}", EmpyrionNetAPIDefinitions.LogLevel.Message);
                            var externalDataAccess = (IMod)Activator.CreateInstance(T);
                            var initialized = false;

                            ((IDictionary<string, Func<IEntity, object[], object>>)getDataProperty.GetValue(externalDataAccess))
                                .ForEach(D => {
                                    if(externalDataModules.TryAdd(D.Key, new ExternalDataAccess { Instance = externalDataAccess, Data = D.Value }))
                                    {
                                        if (!initialized)
                                        {
                                            EmpyrionScripting.Log($"Init external mod: {T}", EmpyrionNetAPIDefinitions.LogLevel.Message);

                                            externalDataAccess.Init(EmpyrionScripting.ModApi);
                                            EmpyrionScripting.StopScriptsEvent += (sender, args) => externalDataAccess.Shutdown();

                                            initialized = true;
                                        }

                                        EmpyrionScripting.Log($"Init external data: {D.Key}", EmpyrionNetAPIDefinitions.LogLevel.Message);
                                    }
                                });
                        }
                    }
                });
            });

            return externalDataModules;
        }


        [HandlebarTag("external")]
        public static void ExternalData(TextWriter output, object rootObject, HelperOptions options, dynamic context, object[] arguments)
        {
            if (arguments.Length == 0) throw new HandlebarsException("{{external data [args]}} helper must have at least one argument: (data)");

            try
            {
                var root     = rootObject as IScriptRootData;
                var dataType = arguments.Get<string>(0);

                if (ExternalDataModules.Value.TryGetValue(dataType, out var access)) options.Template(output, access.Data(root.E.GetCurrent(), arguments.Skip(1).Select(D => ToOridinal(D)).ToArray()));
                else                                                                 options.Inverse (output, context as object);
            }
            catch (Exception error)
            {
                if (!CsScriptFunctions.FunctionNeedsMainThread(error, rootObject)) output.Write("{{external}} error " + EmpyrionScripting.ErrorFilter(error));
            }
        }

        private static object ToOridinal(object obj)
        {
            if (obj is IEntityData       entity      ) return entity.GetCurrent();
            if (obj is IStructureData    structure   ) return structure.GetCurrent();
            if (obj is IBlockData        block       ) return block.Device;
            if (obj is IItemsData        item        ) return new ItemStack { id = item.Id, count = item.Count };
            if (obj is LimitedPlayerData player      ) return player.GetCurrent();

            return obj;
        }
    }
}
