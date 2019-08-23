using System;

namespace EmpyrionScripting
{
    public interface IDeviceLock : IDisposable
    {
        bool Success { get; }

        void Dispose();
    }
}