using System;

namespace EmpyrionScripting.Internal.Interface
{
    public interface IDeviceLock : IDisposable
    {
        bool Success { get; }
        bool Exit { get; }
    }
}