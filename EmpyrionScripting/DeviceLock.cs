using System;
using System.Collections.Concurrent;
using System.Threading;
using Eleon.Modding;

namespace EmpyrionScripting
{
    public class DeviceLock : IDisposable
    {
        readonly Action unlockAction;

        public DeviceLock(IPlayfield playfield, IStructure structure, VectorInt3 position)
        {
            if (!EmpyrionScripting.DeviceLockAllowed) return;

            var witherror = false;
            try
            {
                if (playfield.IsStructureDeviceLocked(structure.Id, position)) return;

                var lockkey = $"{structure.Id}#{position.x}#{position.y}#{position.z}";

                var e = new AutoResetEvent(false);
                playfield.LockStructureDevice(structure.Id, position, true, (s) =>
                {
                    if (witherror)
                    {
                        playfield.LockStructureDevice(structure.Id, position, false, null);
                    }
                    else
                    {
                        Success = s;
                        e.Set();
                    }
                });
                witherror = !e.WaitOne(10000);

                if (Success) unlockAction = () =>
                 {
                     e.Reset();
                     playfield.LockStructureDevice(structure.Id, position, false, (s) => e.Set());
                     e.WaitOne(10000);
                 };
            }
            catch (Exception error)
            {
                witherror = true;
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