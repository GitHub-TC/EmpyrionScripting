using Eleon.Modding;
using System.Collections.Generic;
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

            ScriptDebugLcd = entity?.Structure?.GetDevice<ILcd>("ScriptDebugInfo");
        }

        public ScriptRootData(ScriptRootData data) : this(data.playfield, data.entity)
        {
            _p = data._p;
            _e = data._e;
            ScriptDebugLcd = data.ScriptDebugLcd;
            DisplayType    = data.DisplayType;
        }

        public ILcd ScriptDebugLcd { get; }

        public PlayfieldData P { get => _p == null ? _p = new PlayfieldData(playfield) : _p; set => _p = value; }
        private PlayfieldData _p;

        public EntityData E { get => _e == null ? _e = new EntityData(entity) : _e; set => _e = value; }
        private EntityData _e;

        public List<string> LcdTargets { get; set; } = new List<string>();
        public bool FontSizeChanged { get; set; }
        public int FontSize { get; set; }
        public bool ColorChanged { get; set; }
        public Color Color { get; set; }
        public bool BackgroundColorChanged { get; set; }
        public Color BackgroundColor { get; set; }
        public string Script { get; set; }
        public DisplayOutputConfiguration DisplayType { get; set; }
    }
}
