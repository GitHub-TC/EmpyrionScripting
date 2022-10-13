using Eleon.Modding;
using EmpyrionScripting.Interface;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

namespace EmpyrionScripting.DataWrapper
{
    public class StructureData : IStructureData
    {
        private readonly Lazy<Tuple<ItemsData[], ConcurrentDictionary<string, IContainerSource>>> _is;

        public StructureData()
        {
            _n = new Lazy<string[]>(() => GetCurrent()?.GetAllCustomDeviceNames().OrderBy(N => N).ToArray());
            _is = new Lazy<Tuple<ItemsData[], ConcurrentDictionary<string, IContainerSource>>>(() => CollectAllItems(GetCurrent()));
            _d = new Lazy<IEntityData[]>(() => GetCurrent()?
                .GetDockedVessels()
                .Select(S => E.GetCurrentPlayfield().Entities.FirstOrDefault(E => E.Value.Structure?.Id == S.Id))
                .Where(E => E.Value != null)
                .Select(DockedE => new EntityData(E.GetCurrentPlayfield(), DockedE.Value)).ToArray());
            _s = new Lazy<WeakReference<IStructure>>(() => new WeakReference<IStructure>(E.GetCurrent()?.Structure));
            _pilot = new Lazy<IPlayerData>(() => new PlayerData(E.GetCurrentPlayfield(), GetCurrent()?.Pilot));
            _passengers = new Lazy<IPlayerData[]>(() => GetCurrent()?.GetPassengers()?.Select(P => new PlayerData(E.GetCurrentPlayfield(), P)).ToArray());
            _FuelTank = new Lazy<StructureTank>(() => new StructureTank(GetCurrent()?.FuelTank, StructureTankType.Fuel));
            _OxygenTank = new Lazy<StructureTank>(() => new StructureTank(GetCurrent()?.OxygenTank, StructureTankType.Oxygen));
            _PentaxidTank = new Lazy<StructureTank>(() => new StructureTank(GetCurrent()?.PentaxidTank, StructureTankType.Pentaxid));
            _ControlPanelSignals = new Lazy<ISignalData[]>(() => GetCurrent()?.GetControlPanelSignals().Select(S => new SignalData(this, S)).ToArray());
            _BlockSignals = new Lazy<ISignalData[]>(() => GetCurrent()?.GetBlockSignals().Select(S => new SignalData(this, S)).ToArray());
        }

        public StructureData(IEntityData entity) : this()
        {
            E = entity;
        }

        public virtual IEntityData E { get; protected set; }

        public bool IsPowerd => GetCurrent()?.IsPowered ?? false;
        public float DamageLevel => GetCurrent()?.DamageLevel ?? 0;
        public bool IsOfflineProtectable => GetCurrent()?.IsOfflineProtectable ?? false;
        public bool IsReady => GetCurrent()?.IsReady ?? false;

        public int BlockCount => GetCurrent()?.BlockCount ?? 0;
        public int TriangleCount => GetCurrent()?.TriangleCount ?? 0;
        public int LightCount => GetCurrent()?.LightCount ?? 0;

        public float Fuel => GetCurrent()?.Fuel ?? 0;
        public int PowerConsumption => GetCurrent()?.PowerConsumption ?? 0;
        public int PowerOutCapacity => GetCurrent()?.PowerOutCapacity ?? 0;

        public int SizeClass => GetCurrent()?.SizeClass ?? 0;

        public bool IsShieldActive => GetCurrent()?.IsShieldActive ?? false;
        public int ShieldLevel => GetCurrent()?.ShieldLevel ?? 0;

        public float TotalMass => GetCurrent()?.TotalMass ?? 0;
        public bool HasLandClaimDevice => GetCurrent()?.HasLandClaimDevice ?? false;
        public ulong LastVisitedTicks => GetCurrent()?.LastVisitedTicks ?? 0;
        public string PlayerCreatedSteamId => GetCurrent()?.PlayerCreatedSteamId;

        public VectorInt3 MinPos => GetCurrent()?.MinPos ?? VectorInt3.Undef;
        public VectorInt3 MaxPos => GetCurrent()?.MaxPos ?? VectorInt3.Undef;

        public string[] AllCustomDeviceNames => _n.Value;
        readonly Lazy<string[]> _n;

        public IItemsData[] Items => _is.Value.Item1;

        public IEntityData[] DockedE => _d.Value;
        private readonly Lazy<IEntityData[]> _d;

        public IPlayerData[] Players => _p == null ? _p = E.GetCurrentPlayfield().Players.Values
            .Where(P => E.IsElevated || (E.Faction.Group == FactionGroup.Player && P.Id == E.Faction.Id) || P.Faction.Id == E.Faction.Id)
            .Where(P => P.CurrentStructure?.Id == GetCurrent()?.Id)
            .Select(P => new PlayerData(E.GetCurrentPlayfield(), P)).ToArray() : _p;
        IPlayerData[] _p;

        public string[] GetDeviceTypeNames => Enum.GetNames(typeof(DeviceTypeName));

        public ConcurrentDictionary<string, IContainerSource> ContainerSource => _is.Value.Item2;

        private Tuple<ItemsData[], ConcurrentDictionary<string, IContainerSource>> CollectAllItems(IStructure structure)
        {
            var allItems = new ConcurrentDictionary<int, ItemsData>();
            var containerSource = new ConcurrentDictionary<string, IContainerSource>();

            Parallel.ForEach(AllCustomDeviceNames, N =>
            {
                GetCurrent()?.GetDevicePositions(N)
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
                                IItemsSource source = new ItemsSource() { E = E, Id = I.CreateId(), Count = I.count, Ammo = I.ammo, Decay = I.decay, Container = container, CustomName = block.CustomName, Position = P };
                                allItems.AddOrUpdate(I.id,
                                new ItemsData()
                                {
                                    Source = new[] { source }.ToList(),
                                    Id     = I.id,
                                    Count  = I.count,
                                    Ammo   = I.ammo, 
                                    Decay  = I.decay,
                                    Key    = details == null ? I.id.ToString() : details.Key,
                                    Name   = details == null ? I.id.ToString() : details.Name,
                                },
                                (K, U) => U.AddCount(I.count, source));
                            });
                    });
            });

            return new Tuple<ItemsData[], ConcurrentDictionary<string, IContainerSource>>(allItems.Values.OrderBy(I => I.Id).ToArray(), containerSource);
        }

        virtual public IStructure GetCurrent() => _s.Value.TryGetTarget(out var s) ? s : null;

        public VectorInt3 GlobalToStructPos(Vector3 globalPos) => GetCurrent()?.GlobalToStructPos(globalPos) ?? VectorInt3.Undef;
        public Vector3 StructToGlobalPos(VectorInt3 structPos) => GetCurrent()?.StructToGlobalPos(structPos) ?? Vector3.zero;

        private readonly Lazy<WeakReference<IStructure>> _s;

        public IPlayerData Pilot => _pilot.Value;
        private readonly Lazy<IPlayerData> _pilot;

        public IPlayerData[] Passengers => _passengers.Value;
        private readonly Lazy<IPlayerData[]> _passengers;

        public IStructureTankWrapper FuelTank => _FuelTank.Value;
        private readonly Lazy<StructureTank> _FuelTank;
        public IStructureTankWrapper OxygenTank => _OxygenTank.Value;
        private readonly Lazy<StructureTank> _OxygenTank;
        public IStructureTankWrapper PentaxidTank => _PentaxidTank.Value;
        private readonly Lazy<StructureTank> _PentaxidTank;

        public ISignalData[] ControlPanelSignals => _ControlPanelSignals.Value;
        private readonly Lazy<ISignalData[]> _ControlPanelSignals;
        public ISignalData[] BlockSignals => _BlockSignals.Value;
        private readonly Lazy<ISignalData[]> _BlockSignals;

        
    }
}