using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace EmpyrionScripting
{
    public class Localization
    {
        public Dictionary<string, List<string>> LocalisationData { get; }
        public Localization(string contentPath)
        {
            LocalisationData = File.ReadAllLines(Path.Combine(contentPath, @"Extras\Localization.csv"))
                .Where(L => Char.IsLetter(L[0]))
                .Select(L => {
                    var line = L.Split(',');
                    return new { ID = line.First(), Names = line.Skip(1) };
                })
                .SafeToDictionary(L => L.ID, L => L.Names.ToList(), StringComparer.CurrentCultureIgnoreCase);
        }

        public string GetName(string name, string language)
        {
            if (!LocalisationData.TryGetValue(name, out List<string> i18nData)) return name;

            var languagePos = LocalisationData["KEY"].IndexOf(language);
            return languagePos == -1 || languagePos > i18nData.Count
                ? name
                : i18nData[languagePos];
        }
    }
}