using EmpyrionScripting.CustomHelpers;
using EmpyrionScripting.Interface;
using System.Collections.Generic;

namespace EmpyrionScripting.CsHelper
{
    public partial class CsScriptFunctions : ICsScriptFunctions
    {
        public IList<IItemMoveInfo> Move(IItemsData item, IStructureData structure, string names, int? maxLimit = null) => ConveyorHelpers.Move(Root, item, structure, names, maxLimit);
        public IList<IItemMoveInfo> Fill(IItemsData item, IStructureData structure, StructureTankType type, int? maxLimit = null) => ConveyorHelpers.Fill(Root, item, structure, type, maxLimit ?? 100);
    }
}
