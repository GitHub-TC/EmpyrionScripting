using EmpyrionScripting.Interface;

namespace EmpyrionScripting.CsHelper
{
    public class ConsoleMock : IConsoleMock
    {
        IScriptRootData Root { get; }

        public ConsoleMock(IScriptRootData root){ Root = root; }

        public void Write(string value) => Root.ScriptOutput?.Write(value);
        public void Write(object value) => Root.ScriptOutput?.Write(value);
        public void Write(ulong value) => Root.ScriptOutput?.Write(value);
        public void Write(long value) => Root.ScriptOutput?.Write(value);
        public void Write(int value) => Root.ScriptOutput?.Write(value);
        public void Write(uint value) => Root.ScriptOutput?.Write(value);
        public void Write(bool value) => Root.ScriptOutput?.Write(value);
        public void Write(char value) => Root.ScriptOutput?.Write(value);
        public void Write(decimal value) => Root.ScriptOutput?.Write(value);
        public void Write(float value) => Root.ScriptOutput?.Write(value);
        public void Write(double value) => Root.ScriptOutput?.Write(value);
        public void Write(string format, params object[] arg) => Root.ScriptOutput?.WriteLine(format, arg);
        public void Write(char[] buffer) => Root.ScriptOutput?.WriteLine(buffer);
        public void Write(char[] buffer, int index, int count) => Root.ScriptOutput?.WriteLine(buffer, index, count);

        public void WriteLine() => Root.ScriptOutput?.WriteLine();
        public void WriteLine(float value) => Root.ScriptOutput?.WriteLine(value);
        public void WriteLine(int value) => Root.ScriptOutput?.WriteLine(value);
        public void WriteLine(uint value) => Root.ScriptOutput?.WriteLine(value);
        public void WriteLine(long value) => Root.ScriptOutput?.WriteLine(value);
        public void WriteLine(ulong value) => Root.ScriptOutput?.WriteLine(value);
        public void WriteLine(object value) => Root.ScriptOutput?.WriteLine(value);
        public void WriteLine(string value) => Root.ScriptOutput?.WriteLine(value);
        public void WriteLine(decimal value) => Root.ScriptOutput?.WriteLine(value);
        public void WriteLine(char value) => Root.ScriptOutput?.WriteLine(value);
        public void WriteLine(bool value) => Root.ScriptOutput?.WriteLine(value);
        public void WriteLine(double value) => Root.ScriptOutput?.WriteLine(value);
        public void WriteLine(string format, params object[] arg) => Root.ScriptOutput?.WriteLine(format, arg);
        public void WriteLine(char[] buffer, int index, int count) => Root.ScriptOutput?.WriteLine(buffer, index, count);
        public void WriteLine(char[] buffer) => Root.ScriptOutput?.WriteLine(buffer);
    }
}
