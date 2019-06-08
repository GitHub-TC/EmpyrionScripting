using Eleon.Modding;
using UnityEngine;

namespace EmpyrionScripting
{
    public class ScriptRootData
    {
        private IPlayfield playfield;
        private IEntity entity;

        public ScriptRootData()
        {
        }

        public ScriptRootData(IPlayfield playfield, IEntity entity)
        {
            this.playfield = playfield;
            this.entity = entity;
        }

        public ScriptRootData(ScriptRootData data) : this(data.playfield, data.entity)
        {
            _p = data._p;
            _e = data._e;
            _s = data._s;
        }

        public PlayfieldData P { get => _p == null ? _p = new PlayfieldData(playfield) : _p; set => _p = value; }
        private PlayfieldData _p;

        public EntityData E { get => _e == null ? _e = new EntityData(entity) : _e; set => _e = value; }
        private EntityData _e;

        public StructureData S { get => _s == null ? _s = new StructureData(entity.Structure) : _s; set => _s = value; }
        private StructureData _s;

        public string[] DeviceNames => entity.Structure.GetDeviceTypeNames();

        public string[] LcdTargets { get; set; }
        public int FontSize { get; set; }
        public Color Color { get; set; }
        public Color BackgroundColor { get; set; }
    }
}
