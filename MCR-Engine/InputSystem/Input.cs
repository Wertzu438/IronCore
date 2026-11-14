using System.Runtime.InteropServices;
using MCR_Engine.Core;

namespace MCR_Engine.InputSystem;

public class Input : ProcessTemplate
{
    private readonly Dictionary<string, Key> _keys;
    private readonly Thread _thread;
    internal Input()
    {
        _thread = new Thread(Loop);
        _keys = [];
    }
    public void AddKey(string keyName, int keyCode)
    {
        lock (_keys)
        {
            if (_keys.ContainsKey(keyName))
                throw new ArgumentException("Key already exists");
            _keys.Add(keyName, new Key(keyCode));
        }
    }
    public Key Get(string keyName)
    {
        lock (_keys)
        {
            if (_keys.TryGetValue(keyName, out Key key))
                return key;
            throw new ArgumentException("Key doesn't exist");
        }
    }
    protected override void StartLoop() => _thread.Start();
    private void Loop()
    {
        while (Action is not ProcessAction.Stop)
        {
            while (Action is ProcessAction.Pause)
            {
                State = ProcessState.Paused;
                Thread.Sleep(1000);
            }
            State = ProcessState.Running;
            lock (_keys)
            {
                foreach (Key key in _keys.Values)
                {
                    if (IsKeyPressed(key))
                    {
                        key.Pressed = true;
                        continue;
                    }
                    key.Pressed = false;
                }
            }
            Thread.Sleep(5);
        }
        State = ProcessState.Stopped;
    }
    public static bool IsOnePressed(Key[] keys)
    {
        foreach (Key key in keys)
        {
            if (key.Pressed)
            {
                return true;
            }
        }
        return false;
    }
    private static bool IsKeyPressed(Key key) => (GetAsyncKeyState(key.Code) & 0x8000) != 0;
    [DllImport("user32.dll")]
    private static extern short GetAsyncKeyState(int keyCode);
}