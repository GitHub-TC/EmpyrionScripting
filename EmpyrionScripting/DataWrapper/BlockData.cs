using Eleon.Modding;
using EmpyrionScripting.Interface;

namespace EmpyrionScripting.DataWrapper
{
    public class BlockData : IBlockData
    {
        private IBlock      _block;

        private IDevice     _device;
        private IEntityData _entity;
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

        public BlockData(IEntityData entity, VectorInt3 pos)
        {
            _entity        = entity;
            _structure     = _entity.S?.GetCurrent();
            _block         = _structure?.GetBlock(pos);
            _block         = _block.ParentBlock ?? _block;
            _device        = _structure?.GetDevice<IDevice>(pos);
            Position       = pos;

            if(_device is IContainer c) Device = new ContainerData(c);
        }

        public IEntityData GetEntity() => _entity;
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
            colorTop    = colorTop    >= 0 && colorTop    <= 31 ? colorTop    : 0;
            colorBottom = colorBottom >= 0 && colorBottom <= 31 ? colorBottom : 0;
            colorNorth  = colorNorth  >= 0 && colorNorth  <= 31 ? colorNorth  : 0;
            colorSouth  = colorSouth  >= 0 && colorSouth  <= 31 ? colorSouth  : 0;
            colorWest   = colorWest   >= 0 && colorWest   <= 31 ? colorWest   : 0;
            colorEast   = colorEast   >= 0 && colorEast   <= 31 ? colorEast   : 0;
            colorsReaded = true;
            return this;
        }

        public string SendSignalName => GetStructure()?.GetSendSignalName(Position);
        public bool SignalState => GetStructure()?.GetSignalState(SendSignalName) == true;

        public int Id => GetData().blockType;
        public int BlockType{ get => GetData().blockType;           set { if(BlockType  != value) _block?.Set(value, null, null, null); } }
        public bool Active  { get => GetData().blockActive.Value;   set { if(Active     != value) _block?.Set(null, null, null, value); } }
        public int Shape    { get => GetData().blockShape;          set { if((_entity.IsElevated || !IsDamaged) && Shape      != value) _block?.Set(null, value, null, null); } }
        public int Rotation { get => GetData().blockRotation;       set { if((_entity.IsElevated || !IsDamaged) && Rotation   != value) _block?.Set(null, null, value, null); } }

        public int MaxSlots => StructureData.GetMaxSlots(Id);

        public void SetTextureForWholeBlock(int texIdx)
        {
            if (Top == texIdx && Bottom == texIdx && North == texIdx && South == texIdx && West == texIdx && East == texIdx) return;

            textureTop = textureBottom = textureNorth = textureSouth = textureWest = textureEast = texIdx;
            _block?.SetTextureForWholeBlock(texIdx);
        }

        public int Top      { get => GetTexture().textureTop;       set { if (Top       != value) _block?.SetTextures(textureTop = value, Bottom, North, South, West, East);    } }
        public int Bottom   { get => GetTexture().textureBottom;    set { if (Bottom    != value) _block?.SetTextures(Top, textureBottom = value, North, South, West, East);    } }
        public int North    { get => GetTexture().textureNorth;     set { if (North     != value) _block?.SetTextures(Top, Bottom, textureNorth = value, South, West, East);    } }
        public int South    { get => GetTexture().textureSouth;     set { if (South     != value) _block?.SetTextures(Top, Bottom, North, textureSouth = value, West, East);    } }
        public int West     { get => GetTexture().textureWest;      set { if (West      != value) _block?.SetTextures(Top, Bottom, North, South, textureWest = value, East);    } }
        public int East     { get => GetTexture().textureEast;      set { if (East      != value) _block?.SetTextures(Top, Bottom, North, South, West, textureEast = value);    } }

        //public int Top      { get => GetTexture().textureTop;       set { if (Top       != value) _block?.SetTextures(textureTop = value, null, null, null, null, null);    } }
        //public int Bottom   { get => GetTexture().textureBottom;    set { if (Bottom    != value) _block?.SetTextures(null, textureBottom = value, null, null, null, null); } }
        //public int North    { get => GetTexture().textureNorth;     set { if (North     != value) _block?.SetTextures(null, null, textureNorth = value, null, null, null);  } }
        //public int South    { get => GetTexture().textureSouth;     set { if (South     != value) _block?.SetTextures(null, null, null, textureSouth = value, null, null);  } }
        //public int West     { get => GetTexture().textureWest;      set { if (West      != value) _block?.SetTextures(null, null, null, null, textureWest = value, null);   } }
        //public int East     { get => GetTexture().textureEast;      set { if (East      != value) _block?.SetTextures(null, null, null, null, null, textureEast = value);   } }
        public void SetTextures(int? top, int? bottom, int? north, int? south, int? west, int? east)
        {
            bool changed = false;

            if (top     .HasValue && Top     != top      .Value) { changed = true; textureTop       = top.Value;    }
            if (bottom  .HasValue && Bottom  != bottom   .Value) { changed = true; textureBottom    = bottom.Value; }
            if (north   .HasValue && North   != north    .Value) { changed = true; textureNorth     = north.Value;  }
            if (south   .HasValue && South   != south    .Value) { changed = true; textureSouth     = south.Value;  }
            if (west    .HasValue && West    != west     .Value) { changed = true; textureWest      = west.Value;   }
            if (east    .HasValue && East    != east     .Value) { changed = true; textureEast      = east.Value;   }

            if(changed) _block?.SetTextures(textureTop, textureBottom, textureNorth, textureSouth, textureWest, textureEast);
        }

        public void SetColorForWholeBlock(int texIdx)
        {
            if (TopColor == texIdx && BottomColor == texIdx && NorthColor == texIdx && SouthColor == texIdx && WestColor == texIdx && EastColor == texIdx) return;

            colorTop = colorBottom = colorNorth = colorSouth = colorWest = colorEast = texIdx;
            _block?.SetColorForWholeBlock(texIdx);
        }

        //public int TopColor     { get => GetColor().colorTop;       set { if (TopColor      != value) _block?.SetColors(colorTop = value, BottomColor, NorthColor, SouthColor, WestColor, EastColor); } }
        //public int BottomColor  { get => GetColor().colorBottom;    set { if (BottomColor   != value) _block?.SetColors(TopColor, colorBottom = value, NorthColor, SouthColor, WestColor, EastColor); } }
        //public int NorthColor   { get => GetColor().colorNorth;     set { if (NorthColor    != value) _block?.SetColors(TopColor, BottomColor, colorNorth = value, SouthColor, WestColor, EastColor); } }
        //public int SouthColor   { get => GetColor().colorSouth;     set { if (SouthColor    != value) _block?.SetColors(TopColor, BottomColor, NorthColor, colorSouth = value, WestColor, EastColor); } }
        //public int WestColor    { get => GetColor().colorWest;      set { if (WestColor     != value) _block?.SetColors(TopColor, BottomColor, NorthColor, SouthColor, colorWest = value, EastColor); } }
        //public int EastColor    { get => GetColor().colorEast;      set { if (EastColor     != value) _block?.SetColors(TopColor, BottomColor, NorthColor, SouthColor, WestColor, colorEast = value); } }

        public int TopColor     { get => GetColor().colorTop;       set { if (TopColor      != value) _block?.SetColors(colorTop = value, null, null, null, null, null);    } }
        public int BottomColor  { get => GetColor().colorBottom;    set { if (BottomColor   != value) _block?.SetColors(null, colorBottom = value, null, null, null, null); } }
        public int NorthColor   { get => GetColor().colorNorth;     set { if (NorthColor    != value) _block?.SetColors(null, null, colorNorth = value, null, null, null);  } }
        public int SouthColor   { get => GetColor().colorSouth;     set { if (SouthColor    != value) _block?.SetColors(null, null, null, colorSouth = value, null, null);  } }
        public int WestColor    { get => GetColor().colorWest;      set { if (WestColor     != value) _block?.SetColors(null, null, null, null, colorWest = value, null);   } }
        public int EastColor    { get => GetColor().colorEast;      set { if (EastColor     != value) _block?.SetColors(null, null, null, null, null, colorEast = value);   } }

        public void SetColors(int? top, int? bottom, int? north, int? south, int? west, int? east)
        {
            bool changed = false;

            if (top     .HasValue && TopColor     != top      .Value) { changed = true; }
            if (bottom  .HasValue && BottomColor  != bottom   .Value) { changed = true; }
            if (north   .HasValue && NorthColor   != north    .Value) { changed = true; }
            if (south   .HasValue && SouthColor   != south    .Value) { changed = true; }
            if (west    .HasValue && WestColor    != west     .Value) { changed = true; }
            if (east    .HasValue && EastColor    != east     .Value) { changed = true; }

            if (changed)
            {
                EmpyrionScripting.Log($"SetColors: T:{colorTop}->{top} B:{colorBottom}->{bottom} N:{colorNorth}->{north} S:{colorSouth}->{south} W:{colorWest}->{west} E:{colorEast}->{east}", EmpyrionNetAPIDefinitions.LogLevel.Debug);
                _block?.SetColors(top, bottom, north, south, west, east);

                if (top     .HasValue && TopColor     != top      .Value) { colorTop     = top.Value;   }
                if (bottom  .HasValue && BottomColor  != bottom   .Value) { colorBottom  = bottom.Value;}
                if (north   .HasValue && NorthColor   != north    .Value) { colorNorth   = north.Value; }
                if (south   .HasValue && SouthColor   != south    .Value) { colorSouth   = south.Value; }
                if (west    .HasValue && WestColor    != west     .Value) { colorWest    = west.Value;  }
                if (east    .HasValue && EastColor    != east     .Value) { colorEast    = east.Value;  }
            }
        }

        public void ChangeBlockType(int newType) => _block.Set(newType);

        public bool SwitchState { get { var s = GetData()._block?.GetSwitchState(); return s == null ? false : s.Value; }  set { var s = GetData()._block?.GetSwitchState(); if(s != value) GetData()._block?.SetSwitchState(value); } }
        public int Damage => _block.GetDamage();
        public int HitPoints => _block.GetHitPoints();
        public bool IsDamaged => Damage != 0;
        public string CustomName => _block.CustomName;
        public int? LockCode { get => _block?.LockCode ?? 0; set { if (_block != null) _block.LockCode = value; }}
    }
}
