using Eleon.Modding;

namespace EmpyrionScripting.DataWrapper
{
    public class BlockData
    {
        private IBlock      _block;
        private IDevice     _device;
        private IStructure  _structure;

        public VectorInt3 Position { get; }

        private int blockShape;
        private int blockType;
        private int blockRotation;
        private bool? blockActive;

        private int textureTop;
        private int textureBottom;
        private int textureNorth;
        private int textureSouth;
        private int textureWest;
        private int textureEast;
        private bool textureReaded;

        public BlockData(IStructure structure, VectorInt3 pos)
        {
            _structure  = structure;
            _block      = structure?.GetBlock(pos);
            _device     = structure?.GetDevice<IDevice>(pos);
            Position    = pos;

            if(_device is IContainer c) Device = new ContainerData(c);
        }

        public IStructure GetStructure() => _structure;
        public IBlock GetBlock() => _block;
        public IDevice GetDevice() => _device;

        public object Device { get; }

        private BlockData GetData()
        {
            if (blockActive.HasValue) return this;
            _block.Get(out blockType, out blockShape, out blockRotation, out bool active);
            blockActive = active;
            return this;
        }

        private BlockData GetTexture()
        {
            if (textureReaded) return this;
            _block.GetTextures(out textureTop, out textureBottom, out textureNorth, out textureSouth, out textureWest, out textureEast);
            textureReaded = true;
            return this;
        }

        public int Id => GetData().blockType;
        public bool Active { get => GetData().blockActive.Value; set => _block?.Set(null, null, null, value); }
        public int Shape { get => GetData().blockShape;         set => _block?.Set(null, value, null, null); }
        public int Rotation { get => GetData().blockRotation;   set => _block?.Set(null, null, value, null); }
        public int Top { get => GetTexture().textureTop;        set => _block?.SetTextures(value, null, null, null, null, null); }
        public int Bottom { get => GetTexture().textureBottom;  set => _block?.SetTextures(null, value, null, null, null, null); }
        public int North { get => GetTexture().textureNorth;    set => _block?.SetTextures(null, null, value, null, null, null); }
        public int South { get => GetTexture().textureSouth;    set => _block?.SetTextures(null, null, null, value, null, null); }
        public int West { get => GetTexture().textureWest;      set => _block?.SetTextures(null, null, null, null, value, null); }
        public int East { get => GetTexture().textureEast;      set => _block?.SetTextures(null, null, null, null, null, value); }
        public bool SwitchState { get { var s = GetData()._block?.GetSwitchState(); return s == null ? false : s.Value; }  set => GetData()._block?.SetSwitchState(value); }
        public int Damage => _block.GetDamage();
        public int HitPoints => _block.GetHitPoints();
        public string CustomName => _block.CustomName;
        public int? LockCode => _block.LockCode;
    }
}
