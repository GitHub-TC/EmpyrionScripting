using EmpyrionScripting.CsHelper;
using EmpyrionScripting.Interface;
using EmpyrionScripting.Internal.Interface;
using HandlebarsDotNet;
using System;
using System.IO;
using System.Linq;

namespace EmpyrionScripting.CustomHelpers
{
    [HandlebarHelpers]
    public static class FormatHelpers
    {
        [HandlebarTag("i18n")]
        public static void I18NHelper(TextWriter output, object rootObject, dynamic context, object[] arguments)
        {
            if (arguments.Length != 1 && arguments.Length != 2) throw new HandlebarsException("{{i18n}} helper must have at least one argument: (key|id) [language]");

            var root        = rootObject as IScriptRootData;
            var data        = arguments[0]?.ToString();
            var language    = arguments.Get(1) as string ?? root.CultureInfo.i18nDefault;

            try
            {
                
                if (int.TryParse(data, out int itemId)) {
                    if      (EmpyrionScripting.ConfigEcfAccess.IdBlockMapping.TryGetValue(itemId, out var name))                            data = name;
                    else if (EmpyrionScripting.ItemInfos.ItemInfo.TryGetValue(itemId, out ItemInfo details2))                               data = details2.Key;
                    else if (itemId.IsToken() && EmpyrionScripting.ConfigEcfAccess.TokenById.TryGetValue(itemId.TokenId(), out var token))  data = token.Values?.FirstOrDefault(A => A.Key == "Name").Value?.ToString() ?? itemId.TokenId().ToString();
                }

                output.Write(EmpyrionScripting.Localization.GetName(data, language));
            }
            catch (Exception error)
            {
                if (!CsScriptFunctions.FunctionNeedsMainThread(error, root)) output.Write("{{i18n}} error " + EmpyrionScripting.ErrorFilter(error));
            }
        }

        [HandlebarTag("format")]
        public static void FormatHelper(TextWriter output, object rootObject, dynamic context, object[] arguments)
        {
            if (arguments.Length != 2) throw new HandlebarsException("{{format data format}} helper must have exactly two argument: (data) (format)");

            var root = rootObject as IScriptRootData;

            try
            {
                output.Write(string.Format(root.CultureInfo.CultureInfo, arguments[1]?.ToString(), arguments[0]).Replace(" ", EmpyrionScripting.Configuration.Current.NumberSpaceReplace));
            }
            catch (Exception error)
            {
                if (!CsScriptFunctions.FunctionNeedsMainThread(error, root)) output.Write("{{format}} error " + EmpyrionScripting.ErrorFilter(error));
            }
        }

        [HandlebarTag("bar")]
        public static void ProcessBarHelper(TextWriter output, object root, dynamic context, object[] arguments)
        {
            if (arguments.Length < 4) throw new HandlebarsException("{{bar data min max length [char] [bgchar] [l|r]}} helper must have at least four argument: (data) (min) (max) (length)");

            try
            {
                double.TryParse(arguments[0]?.ToString(), out var value);
                double.TryParse(arguments[1]?.ToString(), out var min);
                double.TryParse(arguments[2]?.ToString(), out var max);
                int.TryParse(arguments[3]?.ToString(), out var barLength);

                var len = (int)(barLength / Math.Abs(max - min) * value);

                if (arguments.Get(6)?.ToString() == "r")
                {
                    output.Write(string.Concat(Enumerable.Repeat(arguments.Length > 5 ? arguments[5].ToString() : EmpyrionScripting.Configuration.Current.BarStandardSpaceSign, Math.Max(0, Math.Min(barLength, barLength + len)))));
                    output.Write(string.Concat(Enumerable.Repeat(arguments.Length > 4 ? arguments[4].ToString() : EmpyrionScripting.Configuration.Current.BarStandardValueSign, Math.Max(0, Math.Min(barLength, -len)))));
                }
                else {
                    output.Write(string.Concat(Enumerable.Repeat(arguments.Length > 4 ? arguments[4].ToString() : EmpyrionScripting.Configuration.Current.BarStandardValueSign, Math.Max(0, Math.Min(barLength, len)))));
                    output.Write(string.Concat(Enumerable.Repeat(arguments.Length > 5 ? arguments[5].ToString() : EmpyrionScripting.Configuration.Current.BarStandardSpaceSign, Math.Max(0, Math.Min(barLength, barLength - len)))));
                }
            }
            catch (Exception error)
            {
                if (!CsScriptFunctions.FunctionNeedsMainThread(error, root)) output.Write("{{bar}} error " + EmpyrionScripting.ErrorFilter(error));
            }
        }

    }
}
