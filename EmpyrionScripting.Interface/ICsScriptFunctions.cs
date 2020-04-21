
using Eleon.Modding;
using System.Collections.Generic;

namespace EmpyrionScripting.Interface
{
    public interface ICsScriptFunctions
    {
        string i18nDefaultLanguage { get; set; }

        string bar(double data, double min, double max, int length, string barChar = null, string barBgChar = null);
        IEnumerable<IEntity> EntitiesByName(string names);
        System.Collections.Generic.IList<IItemMoveInfo> Fill(IItemsData item, IStructureData structure, StructureTankType type, int? maxLimit = null);
        string format(object data, string format);
        string i18n(int id);
        string i18n(int id, string language);
        IItemsData[] Items(IStructureData structure, string names);
        System.Collections.Generic.IList<IItemMoveInfo> Move(IItemsData item, IStructureData structure, string names, int? maxLimit = null);
        IList<string> Scroll(string content, int lines, int delay, int? step);
    }
}