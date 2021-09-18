using EmpyrionScripting.Internal.Interface;

namespace EmpyrionScripting.UnitTests
{
    public class MockDeviceLock : IDeviceLock
    {
        public bool Success => true;

        public bool Exit => false;

        public void Dispose()
        {
        }
    }
}
