using Eleon.Modding;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;

namespace EmpyrionScripting
{
    public class StructureData
    {
        private IStructure structure;

        public StructureData()
        {
        }

        public StructureData(IStructure structure)
        {
            this.structure = structure;
        }

        public bool IsPowerd => structure.IsPowered;

        public string[] AllCustomDeviceNames { get => _n == null ? _n = structure.GetAllCustomDeviceNames() : _n; set => _n = value; }
        string[] _n;

        public ItemsData[] Items { get => _i == null ? _i = CollectAllItems(structure) : _i; set => _i = value; }
        private ItemsData[] _i;

        private ItemsData[] CollectAllItems(IStructure structure)
        {
            var allItems = new ConcurrentDictionary<int, ItemsData>();
            var devices = structure.GetDevices("Container");

            Parallel.For(0, devices.Count, i =>
            {
                var at = devices.GetAt(i);
                var container = structure.GetDevice<IContainer>(at);
                var block     = structure.GetBlock(at.x, at.y, at.z);

                container?.GetContent()
                    .AsParallel()
                    .ForAll(I => {
                        EmpyrionScripting.ItemInfos.ItemInfo.TryGetValue(I.id, out ItemInfo details);
                        var source = new ItemsSource() { Id = I.id, Container = container, CustomName = block.CustomName, Count = I.count };
                        allItems.AddOrUpdate(I.id,
                        new ItemsData() {
                            Source      = new[] { source }.ToList(),
                            Id          = I.id,
                            Count       = I.count,
                            Key         = details == null ? I.id.ToString() : details.Key,
                            Name        = details == null ? I.id.ToString() : details.Name,
                        },
                        (K, U) => U.AddCount(I.count, source));
                    });
            });

            return allItems
                .Values
                .OrderBy(I => I.Id)
                .ToArray();
        }

        public IStructure GetCurrent() => structure;
    }
}