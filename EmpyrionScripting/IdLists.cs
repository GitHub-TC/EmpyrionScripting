using EmpyrionScripting.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EmpyrionScripting
{
    public class IdLists
    {
        public IReadOnlyDictionary<string, int> BlockIdMapping { get; set; } = new Dictionary<string, int>();
        public IReadOnlyDictionary<int, string> IdBlockMapping { get; set; } = new Dictionary<int, string>();
        public Dictionary<string, string> MappedIds { get; set; } = new Dictionary<string, string>();
        public Dictionary<string, string> NamedIds { get; set; } = new Dictionary<string, string>();

        public void ProcessLists(IDictionary<string, string> ids)
        {
            MappedIds.Clear();
            NamedIds .Clear();

            ids.ForEach(idList =>
            {
                var values = idList.Value
                        .Split(',', ';')
                        .Select(T => T.Trim())
                        .ToArray();

                var idValues = new List<int>();
                var unknownNames = new List<string>();

                values.ForEach(T =>
                {
                    var delimiter = T.IndexOf('-', T.StartsWith("-") ? 1 : 0);
                    if (delimiter > 0)
                    {
                        int? fromId = BlockIdMapping.TryGetValue(T.Substring(0, delimiter ).Trim(), out var idLeft ) ? idLeft  : (int?)null;
                        int? toId   = BlockIdMapping.TryGetValue(T.Substring(delimiter + 1).Trim(), out var idRight) ? idRight : (int?)null;

                        if (fromId.HasValue && toId.HasValue) idValues.AddRange(Enumerable.Range(fromId.Value, toId.Value - fromId.Value + 1));
                    }
                    else if (BlockIdMapping.TryGetValue(T, out var id)) idValues.Add(id);
                    else if (int.TryParse(T, out var directId))         idValues.Add(directId);
                    else if (!string.IsNullOrWhiteSpace(T)) unknownNames.Add(T);
                });

                idValues.Sort();
                idValues     = idValues.Distinct().ToList();
                unknownNames = unknownNames.Distinct().ToList();

                var compressedIdList = new StringBuilder();
                int? lastId = null;
                bool withinRange = false;
                foreach (var currentId in idValues)
                {
                    if (lastId.HasValue)
                    {
                        if (Math.Abs(lastId.Value - currentId) == 1) { lastId = currentId; withinRange = true; }
                        else if (withinRange) { withinRange = false; compressedIdList.Append($"-{lastId},{(lastId = currentId).Value}"); }
                        else                                         compressedIdList.Append($",{(lastId = currentId).Value}");
                    }
                    else compressedIdList.Append($",{(lastId = currentId).Value}");
                }
                compressedIdList.Append(withinRange ? $"-{lastId}," : ",");

                MappedIds.Add(idList.Key, compressedIdList.ToString());

                NamedIds.Add(idList.Key, "," + string.Join(",", idValues.Select(id => IdBlockMapping.TryGetValue(id, out var name) ? name : id.ToString()).OrderBy(name => name).Concat(unknownNames)) + ",");
            });
        }
    }
}
