using System;
using Eleon.Modding;
using EmpyrionScripting.Internal.Interface;

namespace EmpyrionScripting
{
    public sealed class WeakDeviceLock : IDeviceLock
    {
        public WeakDeviceLock(IScriptRootData root, IPlayfield playfield, IStructure structure, VectorInt3 position)
        {
            try
            {
                if (!root.ScriptWithinMainThread)
                {
                    root.ScriptNeedsMainThread = true;
                    Exit = true;
                    return;
                }

                if (!root.DeviceLockAllowed || root.TimeLimitReached)
                {
                    Exit = true;
                    return;
                }

                Success = !playfield.IsStructureDeviceLocked(structure.Id, position);
            }
            catch (Exception error)
            {
                throw new Exception($"WeakDeviceLock:failed on Playfield:{playfield?.Name} at Id:{structure.Id} on {position} with: {error}");
            }
        }
        public bool Exit { get; }

        public bool Success { get; }

        public void Dispose(){}
    }
}