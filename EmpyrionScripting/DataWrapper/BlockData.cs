using Eleon.Modding;
using EmpyrionScripting.Interface;
using System;

namespace EmpyrionScripting.DataWrapper
{
    public class BlockData : IBlockData
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
        private bool colorsReaded;
        private int colorTop;
        private int colorBottom;
        private int colorNorth;
        private int colorSouth;
        private int colorWest;
        private int colorEast;

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
            _block?.GetTextures(out textureTop, out textureBottom, out textureNorth, out textureSouth, out textureWest, out textureEast);
            textureReaded = true;
            return this;
        }

        private BlockData GetColor()
        {
            if (colorsReaded) return this;
            _block?.GetColors(out colorTop, out colorBottom, out colorNorth, out colorSouth, out colorWest, out colorEast);
            colorsReaded = true;
            return this;
        }

        public int Id => GetData().blockType;
        public bool Active { get => GetData().blockActive.Value; set => _block?.Set(null, null, null, value); }
        public int Shape { get => GetData().blockShape;         set => _block?.Set(null, value, null, null); }
        public int Rotation { get => GetData().blockRotation;   set => _block?.Set(null, null, value, null); }

        public void SetTextureForWholeBlock(int texIdx)
        {
            textureTop = textureBottom = textureNorth = textureSouth = textureWest = textureEast = texIdx;
            _block?.SetTextureForWholeBlock(texIdx);
        }

        public int Top      { get => GetTexture().textureTop;       set => _block?.SetTextures(textureTop = value, null, null, null, null, null); }
        public int Bottom   { get => GetTexture().textureBottom;    set => _block?.SetTextures(null, textureBottom = value, null, null, null, null); }
        public int North    { get => GetTexture().textureNorth;     set => _block?.SetTextures(null, null, textureNorth = value, null, null, null); }
        public int South    { get => GetTexture().textureSouth;     set => _block?.SetTextures(null, null, null, textureSouth = value, null, null); }
        public int West     { get => GetTexture().textureWest;      set => _block?.SetTextures(null, null, null, null, textureWest = value, null); }
        public int East     { get => GetTexture().textureEast;      set => _block?.SetTextures(null, null, null, null, null, textureEast = value); }
        public void SetTextures(int? top, int? bottom, int? north, int? south, int? west, int? east)
        {
            if (top     .HasValue) textureTop       = top.Value;
            if (bottom  .HasValue) textureBottom    = bottom.Value;
            if (north   .HasValue) textureNorth     = north.Value;
            if (south   .HasValue) textureSouth     = south.Value;
            if (west    .HasValue) textureWest      = west.Value;
            if (east    .HasValue) textureEast      = east.Value;
            _block?.SetTextures(textureTop, textureBottom, textureNorth, textureSouth, textureWest, textureEast);
        }

        public void SetColorForWholeBlock(int texIdx)
        {
            colorTop = colorBottom = colorNorth = colorSouth = colorWest = colorEast = texIdx;
            _block?.SetColorForWholeBlock(texIdx);
        }

        public int TopColor     { get => GetColor().colorTop;       set => _block?.SetColors(colorTop = value, null, null, null, null, null); }
        public int BottomColor  { get => GetColor().colorBottom;    set => _block?.SetColors(null, colorBottom = value, null, null, null, null); }
        public int NorthColor   { get => GetColor().colorNorth;     set => _block?.SetColors(null, null, colorNorth = value, null, null, null); }
        public int SouthColor   { get => GetColor().colorSouth;     set => _block?.SetColors(null, null, null, colorSouth = value, null, null); }
        public int WestColor    { get => GetColor().colorWest;      set => _block?.SetColors(null, null, null, null, colorWest = value, null); }
        public int EastColor    { get => GetColor().colorEast;      set => _block?.SetColors(null, null, null, null, null, colorEast = value); }
        public void SetColors(int? top, int? bottom, int? north, int? south, int? west, int? east)
        {
            if (top     .HasValue) colorTop     = top.Value;
            if (bottom  .HasValue) colorBottom  = bottom.Value;
            if (north   .HasValue) colorNorth   = north.Value;
            if (south   .HasValue) colorSouth   = south.Value;
            if (west    .HasValue) colorWest    = west.Value;
            if (east    .HasValue) colorEast    = east.Value;
            _block?.SetColors(colorTop, colorBottom, colorNorth, colorSouth, colorWest, colorEast);
        }

        public bool SwitchState { get { var s = GetData()._block?.GetSwitchState(); return s == null ? false : s.Value; }  set { var s = GetData()._block?.GetSwitchState(); if(s != value) GetData()._block?.SetSwitchState(value); } }
        public int Damage => _block.GetDamage();
        public int HitPoints => _block.GetHitPoints();
        public string CustomName => _block.CustomName;
        public int? LockCode => _block.LockCode;
    }
}
