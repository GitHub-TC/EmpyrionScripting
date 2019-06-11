using Eleon.Modding;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;

namespace EmpyrionScripting
{
    public class StructureData
    {
        public StructureData()
        {
        }

        public StructureData(EntityData entity)
        {
            E = entity;
        }

        public virtual EntityData E { get; }

        public bool IsPowerd => GetCurrent().IsPowered;

        public string[] AllCustomDeviceNames { get => _n == null ? _n = GetCurrent().GetAllCustomDeviceNames().OrderBy(N => N).ToArray() : _n; set => _n = value; }
        string[] _n;

        public ItemsData[] Items { get => _i == null ? _i = CollectAllItems(GetCurrent()) : _i; set => _i = value; }
        private ItemsData[] _i;

        public EntityData[] DockedE { get => _d == null ? _d = new[] { E } : _d; set => _d = value; }
        private EntityData[] _d;

        public string[] GetDeviceTypeNames => GetCurrent().GetDeviceTypeNames();

        private ItemsData[] CollectAllItems(IStructure structure)
        {
            var allItems = new ConcurrentDictionary<int, ItemsData>();

            Parallel.ForEach(GetCurrent().GetAllCustomDeviceNames(), N =>
            {
                GetCurrent().GetDevicePositions(N)
                    .ForEach(P => { 
                        var container = structure.GetDevice<IContainer>(P);
                        var block     = structure.GetBlock(P.x, P.y, P.z);

                        container?.GetContent()
                            .AsParallel()
                            .ForAll(I => {
                                EmpyrionScripting.ItemInfos.ItemInfo.TryGetValue(I.id, out ItemInfo details);
                                var source = new ItemsSource() { E = E, Id = I.id, Container = container, CustomName = block.CustomName, Count = I.count };
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
            });

            return allItems
                .Values
                .OrderBy(I => I.Id)
                .ToArray();
        }

        virtual public IStructure GetCurrent() => _s == null ? _s = E.GetCurrent().Structure : _s;
        private IStructure _s;
    }
}