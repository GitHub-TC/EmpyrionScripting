using System;
using System.Threading;
using Eleon.Modding;

namespace EmpyrionScripting
{
    public class DeviceLock : IDisposable
    {
        readonly Action unlockAction;

        public enum Locking
        {
            AlwaysLock,
            AlwaysUnlocked,
            Check
        }

        public DeviceLock(IPlayfield playfield, int id, VectorInt3 position)
        {
            if (EmpyrionScripting.Configuration.Current.Locking == Locking.AlwaysLock) return;
            if (EmpyrionScripting.Configuration.Current.Locking == Locking.AlwaysUnlocked)
            {
                Success = true;
                return;
            }

            try
            {
                if (!playfield.IsStructureDeviceLocked(id, position)) return;

                var e = new AutoResetEvent(false);
                Success = playfield.LockStructureDevice(id, position, true, (s) => {
                        Success = s;
                        e.Set();
                    }) && e.WaitOne(10000);

                if(Success) unlockAction = () => playfield.LockStructureDevice(id, position, false, null);
            }
            catch (Exception error)
            {
                throw new Exception($"DeviceLock:failed on Playfield:{playfield?.Name} at Id:{id} on {position} with: {error}");
            }
        }

        public bool Success { get; private set; }

        public void Dispose()
        {
            unlockAction?.Invoke();
        }
    }
}