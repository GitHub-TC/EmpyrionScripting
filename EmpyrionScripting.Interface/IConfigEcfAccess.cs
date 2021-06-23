using EcfParser;
using System.Collections.Generic;

namespace EmpyrionScripting.Interface
{
    public interface IConfigEcfAccess
    {
        IDictionary<int, EcfBlock> ConfigBlockById { get; set; }
        IDictionary<string, EcfBlock> ConfigBlockByName { get; set; }
        EcfFile Configuration_Ecf { get; set; }
        IDictionary<int, EcfBlock> FlatConfigBlockById { get; set; }
        IDictionary<string, EcfBlock> FlatConfigBlockByName { get; set; }
        IDictionary<int, Dictionary<int, double>> ResourcesForBlockById { get; set; }
    }
}