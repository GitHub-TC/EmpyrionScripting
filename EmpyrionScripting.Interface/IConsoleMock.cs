namespace EmpyrionScripting.Interface
{
    public interface IConsoleMock
    {
        void Write(string value);
        void Write(object value);
        void Write(ulong value);
        void Write(long value);
        void Write(int value);
        void Write(uint value);
        void Write(bool value);
        void Write(char value);
        void Write(decimal value);
        void Write(float value);
        void Write(double value);
        void Write(string format, params object[] arg);
        void Write(char[] buffer);
        void Write(char[] buffer, int index, int count);

        void WriteLine();
        void WriteLine(float value);
        void WriteLine(int value);
        void WriteLine(uint value);
        void WriteLine(long value);
        void WriteLine(ulong value);
        void WriteLine(object value);
        void WriteLine(string value);
        void WriteLine(decimal value);
        void WriteLine(char value);
        void WriteLine(bool value);
        void WriteLine(double value);
        void WriteLine(string format, params object[] arg);
        void WriteLine(char[] buffer);
    }
}