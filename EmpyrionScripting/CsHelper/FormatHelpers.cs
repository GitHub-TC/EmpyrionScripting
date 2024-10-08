﻿using System;
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

        public string Format(object data, string format) => string.Format(ScriptRoot.CultureInfo.CultureInfo, format, data).Replace(" ", EmpyrionScripting.Configuration.Current.NumberSpaceReplace);

        public string Bar(double data, double min, double max, int length, string barChar = null, string barBgChar = null)
        {
            var len = (int)(length / (max - min) * data);

            return string.Concat(Enumerable.Repeat(barChar   ?? EmpyrionScripting.Configuration.Current.BarStandardValueSign, Math.Max(0, Math.Min(length, len)))) +
                   string.Concat(Enumerable.Repeat(barBgChar ?? EmpyrionScripting.Configuration.Current.BarStandardSpaceSign, Math.Max(0, Math.Min(length, length - len))));
        }

        public string ToId(string names)
        {
            var idList = names.Split(new[] { ';', ',' }, StringSplitOptions.RemoveEmptyEntries)
                        .Select(I => EmpyrionScripting.ConfigEcfAccess.BlockIdMapping.TryGetValue(I.Trim(), out var id) ? id : 0)
                        .Where(I => I != 0)
                        .ToArray();

            return idList.Aggregate((string)null, (n, i) => n == null ? i.ToString() : $"{n};{i}");
        }

        public string ToName(string ids)
        {
            var idList = ids.Split(new[] { ';', ',' }, StringSplitOptions.RemoveEmptyEntries)
                        .Select(I => int.TryParse(I, out var id) ? EmpyrionScripting.ConfigEcfAccess.IdBlockMapping.TryGetValue(id, out var name) ? name : null : null)
                        .Where(I => !string.IsNullOrEmpty(I))
                        .ToArray();

            return idList.Aggregate((string)null, (n, i) => n == null ? i.ToString() : $"{n};{i}");
        }


    }
}
