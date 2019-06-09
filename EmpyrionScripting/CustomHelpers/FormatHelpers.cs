using HandlebarsDotNet;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace EmpyrionScripting.CustomHelpers
{
    public static class FormatHelpers
    {
        public static void I18NHelper(TextWriter output, dynamic context, object[] arguments)
        {
            if (arguments.Length != 2) throw new HandlebarsException("{{i18n}} helper must have exactly two argument: (key|id) (language)");

            var data = arguments.Length > 0 ? arguments[0] as string : null;
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
        }

        public static void FormatHelper(TextWriter output, dynamic context, object[] arguments)
        {
            if (arguments.Length != 2) throw new HandlebarsException("{{format data format}} helper must have exactly two argument: (data) (format)");

            try
            {
                output.Write(string.Format(arguments[1] as string, arguments[0]).Replace(" ", "\u2007\u2009"));
            }
            catch (Exception error)
            {
                output.Write("{{format}} error " + error.Message);
            }
        }

    }
}
