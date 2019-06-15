using Eleon.Modding;

namespace EmpyrionScripting.DataWrapper
{
    public class BlockData
    {
        public VectorInt3 Position { get; }

        private IBlock _block;
        private int blockShape;
        private int blockType;
        private int blockRotation;
        private bool? blockActive;
        private IStructure _structure;

        public BlockData(IStructure structure, IBlock block, VectorInt3 v)
        {
            _structure = structure;
            Position = v;
            _block = block;
        }

        public IStructure GetStructure() => _structure;

        private BlockData GetData()
        {
            if (blockActive.HasValue) return this;
            _block.Get(out blockType, out blockShape, out blockRotation, out bool active);
            blockActive = active;
            return this;
        }

        public bool Active { get => GetData().blockActive.Value; set => _block.Set(null, null, null, value); }
        public int Id => GetData().blockType;
        public int Shape => GetData().blockShape;
        public int Rotation => GetData().blockRotation;
        public int Damage => _block.GetDamage();
        public int HitPoints => _block.GetHitPoints();
        public string CustomName => _block.CustomName;
        public int? LockCode => _block.LockCode;
    }
}
