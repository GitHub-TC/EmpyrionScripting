using EmpyrionScripting.CsHelper;
using HandlebarsDotNet;
using System;
using System.Globalization;
using System.IO;
using System.Linq;

namespace EmpyrionScripting.CustomHelpers
{
    [HandlebarHelpers]
    public static class FormatHelpers
    {
        public static CultureInfo FormatCulture { get; } = CreateFormatCulture();

        private static CultureInfo CreateFormatCulture()
        {
            var c = (CultureInfo)CultureInfo.CurrentUICulture.Clone();

            c.NumberFormat.PercentNegativePattern = 1;
            c.NumberFormat.PercentPositivePattern = 1;

            return c;
        }

        [HandlebarTag("i18n")]
        public static void I18NHelper(TextWriter output, object root, dynamic context, object[] arguments)
        {
            if (arguments.Length != 2) throw new HandlebarsException("{{i18n}} helper must have exactly two argument: (key|id) (language)");

            var data     = arguments[0]?.ToString();
            var language = arguments[1] as string;

            try
            {
                
                if (arguments[0] is int id) {
                    if (EmpyrionScripting.ConfigEcfAccess.IdBlockMapping.TryGetValue(id, out var name)) data = name;
                    else if(EmpyrionScripting.ItemInfos.ItemInfo.TryGetValue(id, out ItemInfo details1)) data = details1.Key;
                }
                else if (int.TryParse(data, out int itemId)) {
                    if (EmpyrionScripting.ConfigEcfAccess.IdBlockMapping.TryGetValue(itemId, out var name)) data = name;
                    else if (EmpyrionScripting.ItemInfos.ItemInfo.TryGetValue(itemId, out ItemInfo details2)) data = details2.Key;
                }

                output.Write(EmpyrionScripting.Localization.GetName(data, language));
            }
            catch (Exception error)
            {
                if (!CsScriptFunctions.FunctionNeedsMainThread(error, root)) output.Write("{{i18n}} error " + EmpyrionScripting.ErrorFilter(error));
            }
        }

        [HandlebarTag("format")]
        public static void FormatHelper(TextWriter output, object root, dynamic context, object[] arguments)
        {
            if (arguments.Length != 2) throw new HandlebarsException("{{format data format}} helper must have exactly two argument: (data) (format)");

            try
            {
                output.Write(string.Format(FormatCulture, arguments[1]?.ToString(), arguments[0]).Replace(" ", EmpyrionScripting.Configuration.Current.NumberSpaceReplace));
            }
            catch (Exception error)
            {
                if (!CsScriptFunctions.FunctionNeedsMainThread(error, root)) output.Write("{{format}} error " + EmpyrionScripting.ErrorFilter(error));
            }
        }

        [HandlebarTag("bar")]
        public static void ProcessBarHelper(TextWriter output, object root, dynamic context, object[] arguments)
        {
            if (arguments.Length < 4) throw new HandlebarsException("{{bar data min max length [char] [bgchar]}} helper must have at least four argument: (data) (min) (max) (length)");

            try
            {
                double.TryParse(arguments[0]?.ToString(), out var value);
                double.TryParse(arguments[1]?.ToString(), out var min);
                double.TryParse(arguments[2]?.ToString(), out var max);
                int.TryParse(arguments[3]?.ToString(), out var barLength);

                var len = (int)((barLength / (max - min)) * value);

                output.Write(string.Concat(Enumerable.Repeat(arguments.Length > 4 ? arguments[4].ToString() : EmpyrionScripting.Configuration.Current.BarStandardValueSign, Math.Max(0, Math.Min(barLength, len)))));
                output.Write(string.Concat(Enumerable.Repeat(arguments.Length > 5 ? arguments[5].ToString() : EmpyrionScripting.Configuration.Current.BarStandardSpaceSign, Math.Max(0, Math.Min(barLength, barLength - len)))));
            }
            catch (Exception error)
            {
                if (!CsScriptFunctions.FunctionNeedsMainThread(error, root)) output.Write("{{bar}} error " + EmpyrionScripting.ErrorFilter(error));
            }
        }

    }
}
