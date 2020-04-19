namespace EmpyrionScripting.UnitTests
{
    public class MockDeviceLock : IDeviceLock
    {
        public bool Success => true;

        public void Dispose()
        {
        }
    }
}
