
namespace EmpyrionScripting.Interface
{
    public interface ICsScriptFunctions
    {
        System.Collections.Generic.IList<IItemMoveInfo> Fill(IItemsData item, IStructureData structure, StructureTankType type, int? maxLimit = null);
        IItemsData[] Items(IStructureData structure, string names);
        System.Collections.Generic.IList<IItemMoveInfo> Move(IItemsData item, IStructureData structure, string names, int? maxLimit = null);
    }
}