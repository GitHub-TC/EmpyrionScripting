using Eleon.Modding;

namespace EmpyrionScripting.DataWrapper
{
    public class TeleporterData : BlockData
    {
        private ITeleporter                  _teleporter;
        private Eleon.Modding.TeleporterData _targetData;

        public TeleporterData(IStructure structure, VectorInt3 pos) : base(structure, pos)
        {
            _teleporter = GetDevice() as ITeleporter;
            if(_teleporter != null) _targetData = _teleporter.TargetData;
        }

        public ITeleporter GetTeleporter() => _teleporter;

        public string DeviceName    { get => Get(_targetData.TargetEntityNameOrGroup, 0);   set { _targetData.TargetEntityNameOrGroup = $"{value}@{Get(_targetData.TargetEntityNameOrGroup, 1)}"; _teleporter.TargetData = _targetData; } }
        public string Target        { get => Get(_targetData.TargetEntityNameOrGroup, 1);   set { _targetData.TargetEntityNameOrGroup = $"{Get(_targetData.TargetEntityNameOrGroup, 0)}@{value}"; _teleporter.TargetData = _targetData; } }
        public string Playfield     { get => _targetData.TargetPlayfield;           set { _targetData.TargetPlayfield           = value; _teleporter.TargetData = _targetData; } }
        public byte   Origin        { get => _targetData.Origin;                    set { _targetData.Origin                    = value; _teleporter.TargetData = _targetData; } }
        public string Destination { 
            get => $"{DeviceName}@{Target}:{Playfield}"; 
            set {
                var data = value?.Split('@', ':', '#');
                _teleporter.TargetData = new Eleon.Modding.TeleporterData($"{data[0]}@{data[1]}", data[2], data.Length == 4 && byte.TryParse(data[3], out var origin) ? origin : (byte)255);
            }
        }

        private string Get(string target, int pos)
        {
            var data = target?.Split('@');
            return data == null || data.Length < pos ? string.Empty : data[pos];
        }

    }
}
