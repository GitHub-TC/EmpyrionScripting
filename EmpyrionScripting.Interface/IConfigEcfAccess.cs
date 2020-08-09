using EcfParser;
using System.Collections.Generic;

namespace EmpyrionScripting.Interface
{
    public interface IConfigEcfAccess
    {
        Dictionary<int, EcfBlock> ConfigBlockById { get; set; }
        Dictionary<string, EcfBlock> ConfigBlockByName { get; set; }
        EcfFile Configuration_Ecf { get; set; }
        Dictionary<int, EcfBlock> FlatConfigBlockById { get; set; }
        Dictionary<string, EcfBlock> FlatConfigBlockByName { get; set; }
        Dictionary<int, Dictionary<int, int>> RecipeForBlockById { get; set; }
    }
}