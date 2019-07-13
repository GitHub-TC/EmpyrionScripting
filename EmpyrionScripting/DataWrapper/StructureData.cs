using Eleon.Modding;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;

namespace EmpyrionScripting.DataWrapper
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

        public virtual EntityData E { get; protected set; }

        public bool IsPowerd => GetCurrent().IsPowered;
        public float DamageLevel => GetCurrent().DamageLevel;
        public bool IsOfflineProtectable => GetCurrent().IsOfflineProtectable;
        public bool IsReady => GetCurrent().IsReady;

        public string[] AllCustomDeviceNames { get => _n == null ? _n = GetCurrent().GetAllCustomDeviceNames().OrderBy(N => N).ToArray() : _n; set => _n = value; }
        string[] _n;

        public ItemsData[] Items { get => _i == null ? _i = CollectAllItems(GetCurrent()) : _i; set => _i = value; }
        private ItemsData[] _i;

        public EntityData[] DockedE { get => _d == null ? _d = GetCurrent()
                .GetDockedStructures()
                .Select(S => EmpyrionScripting.ModApi.Playfield.Entities.FirstOrDefault(E => E.Value.Structure?.Id == S.Id))
                .Where(E => E.Value != null)
                .Select(E => new EntityData(E.Value)).ToArray() : _d; set => _d = value; }
        private EntityData[] _d;

        public string[] GetDeviceTypeNames => GetCurrent().GetDeviceTypeNames();

        public ConcurrentDictionary<string, ContainerSource> ContainerSource {
            get {
                if (_ContainerSource == null) Items = CollectAllItems(GetCurrent());
                return _ContainerSource;
            }
            set => _ContainerSource = value;
        }
        private ConcurrentDictionary<string, ContainerSource> _ContainerSource;

        private ItemsData[] CollectAllItems(IStructure structure)
        {
            var allItems = new ConcurrentDictionary<int, ItemsData>();
            ContainerSource = new ConcurrentDictionary<string, ContainerSource>();

            Parallel.ForEach(GetCurrent().GetAllCustomDeviceNames(), N =>
            {
                GetCurrent().GetDevicePositions(N)
                    .ForEach(P => { 
                        var container = structure.GetDevice<IContainer>(P);
                        var block     = structure.GetBlock(P);
                        if (container == null || block == null) return;

                        ContainerSource.TryAdd(block.CustomName, new ContainerSource() { E = E, Container = container, CustomName = block.CustomName, Position = P });

                        container.GetContent()
                            .AsParallel()
                            .ForAll(I => {
                                EmpyrionScripting.ItemInfos.ItemInfo.TryGetValue(I.id, out ItemInfo details);
                                var source = new ItemsSource() { E = E, Id = I.id, Count = I.count, Container = container, CustomName = block.CustomName, Position = P };
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

        public PlayerData Pilot => _pilot == null ? _pilot = new PlayerData(GetCurrent().Pilot) : _pilot;
        private PlayerData _pilot;

        public PlayerData[] Passengers => _passengers == null ? _passengers = GetCurrent().GetPassengers()?.Select(P => new PlayerData(P)).ToArray() : _passengers;
        private PlayerData[] _passengers;

        public StructureTank FuelTank => _FuelTank == null ? _FuelTank = new StructureTank(GetCurrent().FuelTank) : _FuelTank;
        private StructureTank _FuelTank;

        public StructureTank OxygenTank => _OxygenTank == null ? _OxygenTank = new StructureTank(GetCurrent().OxygenTank) : _OxygenTank;
        private StructureTank _OxygenTank;

    }
}