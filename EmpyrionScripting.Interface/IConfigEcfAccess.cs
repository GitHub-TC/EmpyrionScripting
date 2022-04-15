using EcfParser;
using System.Collections.Generic;

namespace EmpyrionScripting.Interface
{
    public interface IConfigEcfAccess
    {
        IReadOnlyDictionary<string, int> BlockIdMapping { get; }
        IReadOnlyDictionary<int, string> IdBlockMapping { get; }

        IReadOnlyDictionary<int, EcfBlock> ConfigBlockById { get; }
        IReadOnlyDictionary<string, EcfBlock> ConfigBlockByName { get; }
        EcfFile Configuration_Ecf { get; }
        IReadOnlyDictionary<int, EcfBlock> FlatConfigBlockById { get; }
        IReadOnlyDictionary<string, EcfBlock> FlatConfigBlockByName { get; }
        IReadOnlyDictionary<int, Dictionary<int, double>> ResourcesForBlockById { get; }
        Dictionary<int, IHarvestInfo> HarvestBlockData { get; }
    }
}