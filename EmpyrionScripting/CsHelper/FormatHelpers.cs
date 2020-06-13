using EmpyrionScripting.CustomHelpers;
using System;
using System.Linq;

namespace EmpyrionScripting.CsHelper
{
    public partial class CsScriptFunctions
    {
        public string I18nDefaultLanguage { get; set; } = "English";

        public string I18n(int id) => I18n(id, I18nDefaultLanguage);
        public string I18n(int id, string language) => EmpyrionScripting.ItemInfos.ItemInfo.TryGetValue(id, out ItemInfo data)
            ? EmpyrionScripting.Localization.GetName(data.Key,      language)
            : EmpyrionScripting.Localization.GetName(id.ToString(), language);

        public string Format(object data, string format) => string.Format(FormatHelpers.FormatCulture, format, data).Replace(" ", EmpyrionScripting.Configuration.Current.NumberSpaceReplace);

        public string Bar(double data, double min, double max, int length, string barChar = null, string barBgChar = null)
        {
            var len = (int)(length / (max - min) * data);

            return string.Concat(Enumerable.Repeat(barChar   ?? EmpyrionScripting.Configuration.Current.BarStandardValueSign, Math.Max(0, Math.Min(length, len)))) +
                   string.Concat(Enumerable.Repeat(barBgChar ?? EmpyrionScripting.Configuration.Current.BarStandardSpaceSign, Math.Max(0, Math.Min(length, length - len))));
        }
    }
}
