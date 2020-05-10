using EcfParser;
using System.Collections.Generic;

namespace EmpyrionScripting.Interface
{
    public interface IConfigEcfAccess
    {
        Dictionary<int, EcfBlock> ConfigBlockById { get; set; }
        Dictionary<string, EcfBlock> ConfigBlockByName { get; set; }
        EcfFile Configuration_Ecf { get; set; }
    }
}