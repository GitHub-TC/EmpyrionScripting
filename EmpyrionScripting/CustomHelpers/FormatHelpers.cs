using HandlebarsDotNet;
using System;
using System.IO;

namespace EmpyrionScripting.CustomHelpers
{
    [HandlebarHelpers]
    public static class FormatHelpers
    {
        [HandlebarTag("i18n")]
        public static void I18NHelper(TextWriter output, dynamic context, object[] arguments)
        {
            if (arguments.Length != 2) throw new HandlebarsException("{{i18n}} helper must have exactly two argument: (key|id) (language)");

            var data     = arguments[0]?.ToString();
            var language = arguments[1] as string;

            try
            {
                if(arguments[0] is int &&
                    EmpyrionScripting.ItemInfos.ItemInfo.TryGetValue((int)arguments[0], out ItemInfo details1)) data = details1.Key;
                else if (int.TryParse(data, out int itemId) &&
                    EmpyrionScripting.ItemInfos.ItemInfo.TryGetValue(itemId, out ItemInfo details2)) data = details2.Key;

                output.Write(EmpyrionScripting.Localization.GetName(data, language));
            }
            catch (Exception error)
            {
                output.Write("{{i18n}} error " + error.Message);
            }
        }

        [HandlebarTag("format")]
        public static void FormatHelper(TextWriter output, dynamic context, object[] arguments)
        {
            if (arguments.Length != 2) throw new HandlebarsException("{{format data format}} helper must have exactly two argument: (data) (format)");

            try
            {
                output.Write(string.Format(arguments[1]?.ToString(), arguments[0]).Replace(" ", "\u2007\u2009"));
            }
            catch (Exception error)
            {
                output.Write("{{format}} error " + error.Message);
            }
        }

    }
}
