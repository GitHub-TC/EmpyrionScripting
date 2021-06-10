using EcfParser;
using Eleon.Modding;
using System;
using System.Collections.Generic;

namespace EmpyrionScripting.Interface
{
    public interface ICsScriptFunctions
    {
        string I18nDefaultLanguage { get; set; }
        IScriptModData Root { get; }

        IList<string> Scroll(string content, int lines, int delay, int step = 1);
        string Bar(double data, double min, double max, int length, string barChar = null, string barBgChar = null);
        IBlockData Block(IStructureData structure, int x, int y, int z);
        IBlockData[] Devices(IStructureData structure, string names);
        IBlockData[] DevicesOfType(IStructureData structure, DeviceTypeName deviceType);
        IEnumerable<IEntityData> EntitiesById(params int[] ids);
        IEnumerable<IEntityData> EntitiesById(string ids);
        IEnumerable<IEntityData> EntitiesByName(params string[] names);
        IEnumerable<IEntityData> EntitiesByName(string names);
        string Format(object data, string format);
        string I18n(int id);
        string I18n(int id, string language);
        IItemsData[] Items(IStructureData structure, string names);
        IList<IItemMoveInfo> Fill(IItemsData item, IStructureData structure, StructureTankType type, int? maxLimit = null);
        IList<IItemMoveInfo> Move(IItemsData item, IStructureData structure, string names, int? maxLimit = null);
        void WithLockedDevice(IStructureData structure, IBlockData block, Action action, Action lockFailed = null);
        bool IsLocked(IStructureData structure, IBlockData block);
        object ConfigFindAttribute(int id, string name);
        EcfBlock ConfigById(int id);
        EcfBlock ConfigByName(string name);
        bool FunctionNeedsMainThread(Exception error);
        T[] GetDevices<T>(params IBlockData[] block) where T : class, IDevice;
        T[] GetDevices<T>(IStructureData structure, string names) where T : class, IDevice;
        (IBlockData B, T D)[] GetBlockDevices<T>(IStructureData structure, string names) where T : class, IDevice;
        (IBlockData B, T D)[] GetBlockDevices<T>(params IBlockData[] block) where T : class, IDevice;
        Dictionary<int, int> ResourcesForBlockById(int id);
        bool ShowDialog(int playerId, DialogConfig dialogConfig, DialogActionHandler actionHandler, int customValue);
        bool ShowDialog(string signalNames, DialogConfig dialogConfig, DialogActionHandler actionHandler, int customValue);
    }
}