using EmpyrionNetAPIDefinitions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace EmpyrionScripting
{
    public class Localization
    {
        public static Action<string, LogLevel> Log { get; set; } = (s, l) => Console.WriteLine(s);

        public Dictionary<string, List<string>> LocalisationData { get; }
        public Localization(string contentPath, string activeScenario)
        {
            var scenarioPath = string.IsNullOrEmpty(activeScenario) ? null : Path.Combine(contentPath, "Scenarios", activeScenario);

            LocalisationData = ReadLocalisation(contentPath);

            if (!string.IsNullOrEmpty(scenarioPath))
            {
                ReadLocalisation(scenarioPath).ForEach(item =>
                {
                    if (LocalisationData.ContainsKey(item.Key)) LocalisationData[item.Key] = RemoveFormats(item.Value);
                    else                                        LocalisationData.Add(item.Key, RemoveFormats(item.Value));
                });
            }
        }

        private List<string> RemoveFormats(List<string> values)
            => values.Select(v => RemoveFormats(v)).ToList();

        private string RemoveFormats(string value)
        {
            if (string.IsNullOrEmpty(value)) return value;

            var startPos = value.IndexOf('[');
            while(startPos >= 0)
            {
                var endPos = value.IndexOf(']', startPos);
                if (endPos >= 0) value = value.Substring(0, startPos) + value.Substring(endPos + 1);
                else             startPos++;

                if (startPos >= value.Length) return value;

                startPos = value.IndexOf('[', startPos);
            }
            return value;
        }

        private Dictionary<string, List<string>> ReadLocalisation(string contentPath)
            => ReadLocalisatioonFile(contentPath)
                .Where(L => char.IsLetter(L[0]))
                .Select(L =>
                {
                    var line = L.Split(',');
                    return new { ID = line.First(), Names = line.Skip(1) };
                })
                .SafeToDictionary(L => L.ID, L => L.Names.ToList(), StringComparer.CurrentCultureIgnoreCase);

        private static string[] ReadLocalisatioonFile(string contentPath)
        {
            var filename = Path.Combine(contentPath, @"Extras\Localization.csv");
            Log($"LocalisationData from '{filename}'", LogLevel.Message);
            return File.ReadAllLines(filename);
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