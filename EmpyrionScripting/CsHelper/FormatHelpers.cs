using EmpyrionScripting.CustomHelpers;
using System;
using System.Linq;

namespace EmpyrionScripting.CsHelper
{
    public partial class CsScriptFunctions
    {
        public string i18nDefaultLanguage { get; set; } = "English";

        public string i18n(int id) => i18n(id, i18nDefaultLanguage);
        public string i18n(int id, string language) => EmpyrionScripting.ItemInfos.ItemInfo.TryGetValue(id, out ItemInfo data)
            ? EmpyrionScripting.Localization.GetName(data.Key,      language)
            : EmpyrionScripting.Localization.GetName(id.ToString(), language);

        public string format(object data, string format) => string.Format(FormatHelpers.FormatCulture, format, data).Replace(" ", "\u2007\u2009");

        public string bar(double data, double min, double max, int length, string barChar = null, string barBgChar = null)
        {
            var len = (int)(length / (max - min) * data);

            return string.Concat(Enumerable.Repeat(barChar   ?? "\u2588", Math.Max(0, Math.Min(length, len)))) +
                   string.Concat(Enumerable.Repeat(barBgChar ?? "\u2591", Math.Max(0, Math.Min(length, length - len))));
        }
    }
}
