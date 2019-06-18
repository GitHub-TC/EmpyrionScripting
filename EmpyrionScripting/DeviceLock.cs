using System;
using System.Threading;
using Eleon.Modding;

namespace EmpyrionScripting
{
    public class DeviceLock : IDisposable
    {
        readonly Action unlockAction;

        public DeviceLock(IPlayfield playfield, IStructure structure, VectorInt3 position)
        {
            try
            {
                if (playfield.IsStructureDeviceLocked(structure.Id, position)) return;

                var e = new AutoResetEvent(false);
                playfield.LockStructureDevice(structure.Id, position, true, (s) =>
                {
                    Success = s;
                    e.Set();
                });
                e.WaitOne(10000);

                if(Success) unlockAction = () => playfield.LockStructureDevice(structure.Id, position, false, null);
            }
            catch (Exception error)
            {
                throw new Exception($"DeviceLock:failed on Playfield:{playfield?.Name} at Id:{structure.Id} on {position} with: {error}");
            }
        }

        public bool Success { get; private set; }

        public void Dispose()
        {
            unlockAction?.Invoke();
        }
    }
}