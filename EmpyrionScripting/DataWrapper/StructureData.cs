using Eleon.Modding;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;

namespace EmpyrionScripting.DataWrapper
{
    public class StructureData : IStructureData
    {
        private readonly Lazy<Tuple<ItemsData[], ConcurrentDictionary<string, ContainerSource>>> _is;

        public StructureData()
        {
            _n = new Lazy<string[]>(() => GetCurrent().GetAllCustomDeviceNames().OrderBy(N => N).ToArray());
            _is = new Lazy<Tuple<ItemsData[], ConcurrentDictionary<string, ContainerSource>>>(() => CollectAllItems(GetCurrent()));
            _d = new Lazy<IEntityData[]>(() => GetCurrent()
                .GetDockedStructures()
                .Select(S => EmpyrionScripting.ModApi.Playfield.Entities.FirstOrDefault(E => E.Value.Structure?.Id == S.Id))
                .Where(E => E.Value != null)
                .Select(E => new EntityData(E.Value)).ToArray());
            _s = new Lazy<IStructure>(() => E.GetCurrent().Structure);
            _pilot = new Lazy<PlayerData>(() => new PlayerData(GetCurrent().Pilot));
            _passengers = new Lazy<PlayerData[]>(() => GetCurrent().GetPassengers()?.Select(P => new PlayerData(P)).ToArray());
            _FuelTank = new Lazy<StructureTank>(() => new StructureTank(GetCurrent().FuelTank));
            _OxygenTank = new Lazy<StructureTank>(() => new StructureTank(GetCurrent().OxygenTank));
        }

        public StructureData(IEntityData entity) : this()
        {
            E = entity;
        }

        public virtual IEntityData E { get; protected set; }

        public bool IsPowerd => GetCurrent().IsPowered;
        public float DamageLevel => GetCurrent().DamageLevel;
        public bool IsOfflineProtectable => GetCurrent().IsOfflineProtectable;
        public bool IsReady => GetCurrent().IsReady;

        public string[] AllCustomDeviceNames => _n.Value;
        readonly Lazy<string[]> _n;

        public ItemsData[] Items => _is.Value.Item1;

        public IEntityData[] DockedE => _d.Value;
        private readonly Lazy<IEntityData[]> _d;

        public string[] GetDeviceTypeNames => GetCurrent().GetDeviceTypeNames();

        public ConcurrentDictionary<string, ContainerSource> ContainerSource => _is.Value.Item2;

        private Tuple<ItemsData[], ConcurrentDictionary<string, ContainerSource>> CollectAllItems(IStructure structure)
        {
            var allItems = new ConcurrentDictionary<int, ItemsData>();
            var containerSource = new ConcurrentDictionary<string, ContainerSource>();

            Parallel.ForEach(AllCustomDeviceNames, N =>
            {
                GetCurrent().GetDevicePositions(N)
                    .ForEach(P =>
                    {
                        var container = structure.GetDevice<IContainer>(P);
                        var block = structure.GetBlock(P);
                        if (container == null || block == null) return;

                        containerSource.TryAdd(block.CustomName, new ContainerSource() { E = E, Container = container, CustomName = block.CustomName, Position = P });

                        container.GetContent()
                            .ForEach(I =>
                            {
                                EmpyrionScripting.ItemInfos.ItemInfo.TryGetValue(I.id, out ItemInfo details);
                                var source = new ItemsSource() { E = E, Id = I.id, Count = I.count, Container = container, CustomName = block.CustomName, Position = P };
                                allItems.AddOrUpdate(I.id,
                                new ItemsData()
                                {
                                    Source = new[] { source }.ToList(),
                                    Id = I.id,
                                    Count = I.count,
                                    Key = details == null ? I.id.ToString() : details.Key,
                                    Name = details == null ? I.id.ToString() : details.Name,
                                },
                                (K, U) => U.AddCount(I.count, source));
                            });
                    });
            });

            return new Tuple<ItemsData[], ConcurrentDictionary<string, ContainerSource>>(allItems.Values.OrderBy(I => I.Id).ToArray(), containerSource);
        }

        virtual public IStructure GetCurrent() => _s.Value;
        private readonly Lazy<IStructure> _s;

        public PlayerData Pilot => _pilot.Value;
        private readonly Lazy<PlayerData> _pilot;

        public PlayerData[] Passengers => _passengers.Value;
        private readonly Lazy<PlayerData[]> _passengers;

        public StructureTank FuelTank => _FuelTank.Value;
        private readonly Lazy<StructureTank> _FuelTank;

        public StructureTank OxygenTank => _OxygenTank.Value;
        private readonly Lazy<StructureTank> _OxygenTank;

    }
}