using Eleon.Modding;

namespace EmpyrionScripting.DataWrapper
{
    public class SignalData
    {
        private readonly SenderSignal   _Signal;
        private readonly IStructureData _Structure;
        bool? currentState;

        public SignalData(IStructureData structure, SenderSignal signal)
        {
            _Signal = signal;
            _Structure = structure;
        }

        public SenderSignal GetCurrent() => _Signal;

        public string Name => _Signal.Name;
        public int Index => _Signal.Index;
        public VectorInt3? BlockPos => _Signal.BlockPos;
        public bool State => currentState.HasValue ? currentState.Value : (currentState = _Structure.GetCurrent().GetSignalState(_Signal.Name)).Value;
    }
}
