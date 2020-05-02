using Eleon.Modding;
using EmpyrionScripting;
using EmpyrionScripting.CustomHelpers;
using EmpyrionScripting.DataWrapper;
using EmpyrionScripting.Interface;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Linq;

public class ModMain
{
    public static void Main(IScriptModData rootObject)
    {
        if (!(rootObject is IScriptSaveGameRootData root)) return;
        if (root.E.Faction.Id == 0) return;

        var infoOutLcds = root.CsRoot.GetDevices<ILcd>(root.CsRoot.Devices(root.E.S, "CargoOutInfo*"));

        root.E.S
            .AllCustomDeviceNames
            .GetUniqueNames("CargoOut@*")
            .ForEach(cargoOutContainerName =>
            {
                var container = root.CsRoot.Devices(root.E.S, cargoOutContainerName).FirstOrDefault();
                if (container == null) return;
                if (!int.TryParse(cargoOutContainerName.Substring("CargoOut@".Length), out var targetEntityId))
                {
                    WriteTo(infoOutLcds, $"CargoOut@[ID] id is not a number");
                    return;
                }

                var cargoTargetFileName = Path.Combine(root.MainScriptPath, "..", "CargoTeleport", root.E.Faction.Id.ToString(), $"Cargo-{targetEntityId}.json");

                if (!File.Exists(cargoTargetFileName))
                {
                    WriteTo(infoOutLcds, $"CargoIn in [ID] not ready");
                    return;
                }

                root.CsRoot.WithLockedDevice(root.E.S, container, () =>
                {
                    var nativeContainer = ((ContainerData)container.Device).GetContainer() as IContainer;

                    var items = nativeContainer.GetContent();
                    var failedItems = new List<ItemStack>();
                    items.ForEach(i =>
                    {
                        try
                        {
                            File.AppendAllText(cargoTargetFileName, JsonConvert.SerializeObject(i) + "\n");
                            WriteTo(infoOutLcds, $"Transfer: Item:[{i.id}] {i.count} {root.CsRoot.I18n(i.id)}");
                        }
                        catch
                        {
                            failedItems.Add(i);
                            WriteTo(infoOutLcds, $"Transfer failed: Item:[{i.id}] {i.count} {root.CsRoot.I18n(i.id)}");
                        }
                    });
                    nativeContainer.SetContent(failedItems);
                });
            });

        var infoInLcds = root.CsRoot.GetDevices<ILcd>(root.CsRoot.Devices(root.E.S, "CargoInInfo*"));

        root.E.S
            .AllCustomDeviceNames
            .GetUniqueNames("CargoIn")
            .ForEach(cargoInContainerName =>
            {
                var container = root.CsRoot.Devices(root.E.S, cargoInContainerName).FirstOrDefault();
                if (container == null) return;

                var nativeContainer = ((ContainerData)container.Device).GetContainer() as IContainer;
                if (nativeContainer == null) return;

                var cargoTargetFileName = Path.Combine(root.MainScriptPath, "..", "CargoTeleport", root.E.Faction.Id.ToString(), $"Cargo-{root.E.Id}.json");
                Directory.CreateDirectory(Path.GetDirectoryName(cargoTargetFileName));

                root.CsRoot.WithLockedDevice(root.E.S, container, () =>
                {
                    var items = nativeContainer.GetContent() ?? new List<ItemStack>();
                    bool itemsAdded = false;
                    using (var lockFile = File.Open(cargoTargetFileName, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None))
                    {
                        using (var file = new StreamReader(lockFile))
                        {
                            var remainLines = new List<string>();

                            while (true)
                            {
                                var itemLine = file.ReadLine();
                                if (string.IsNullOrEmpty(itemLine)) break;
                                else if (items.Count < 64)
                                {
                                    var item = JsonConvert.DeserializeObject<ItemStack>(itemLine);
                                    items.Add(item);
                                    itemsAdded = true;
                                    WriteTo(infoInLcds, $"Transfer: Item:[{item.id}] {item.count} {root.CsRoot.I18n(item.id)}");
                                }
                                else remainLines.Add(itemLine);
                            }

                            lockFile.Seek(0, SeekOrigin.Begin);
                            lockFile.SetLength(0);

                            using (var fileWrite = new StreamWriter(lockFile))
                            {
                                remainLines.ForEach(l => fileWrite.WriteLine(l));
                            }
                        }
                    }
                    if (items.Count > 0 && itemsAdded)
                    {
                        byte index = 0;
                        nativeContainer.SetContent(items
                            .Select(i => new ItemStack(i.id, i.count) { slotIdx = index++, decay = i.decay, ammo = i.ammo }).ToList());
                    }
                });
            });

    }

    private static void WriteTo(ILcd[] lcds, string text)
    {
        lcds.ForEach(L => L.SetText($"{text}\n{L.GetText()}"));
    }
}