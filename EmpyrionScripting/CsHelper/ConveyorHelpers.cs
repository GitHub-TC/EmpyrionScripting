using EmpyrionScripting.CustomHelpers;
using EmpyrionScripting.DataWrapper;
using System.Collections.Generic;

namespace EmpyrionScripting.CsHelper
{
    public partial class CsScriptFunctions
    {
        public IList<IItemMoveInfo> Move(ItemsData item, IStructureData structure, string names, int? maxLimit = null)
        {
            return ConveyorHelpers.Move(Root, item, structure, names, maxLimit);
        }

        public IList<IItemMoveInfo> Fill(ItemsData item, IStructureData structure, StructureTankType type, int? maxLimit = null)
        {
            return ConveyorHelpers.Fill(Root, item, structure, type, maxLimit ?? 100);
        }

    }
}
