using System.Runtime.InteropServices;
using MCR_Engine.Core;

namespace MCR_Engine.InputSystem;

public class Input
    {
        private readonly Dictionary<string, Key> _keys;
        private readonly Thread _thread;
        private ProcessAction? _action;
        private ProcessState _state;
        internal Input()
        {
            _action = null;
            _state = ProcessState.Ready;
            _thread = new Thread(Loop);
            _keys = [];
        }
        public void ChangeAction(ProcessAction newAction)
        {
            if (newAction is ProcessAction.Start && _state is ProcessState.Ready or ProcessState.Paused)
            {
                _action = newAction;
                if (_state is not ProcessState.Paused)
                    StartLoop();
            }
            else if ((newAction is ProcessAction.Stop && _state is ProcessState.Running or ProcessState.Paused) || (newAction is ProcessAction.Pause && _state is ProcessState.Running))
            {
                _action = newAction;
            }
            else throw new ArgumentException("Invalid action");
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
        private void Loop()
        {
            while (_action is not ProcessAction.Stop)
            {
                while (_action is ProcessAction.Pause)
                {
                    _state = ProcessState.Paused;
                    Thread.Sleep(1000);
                }
                _state = ProcessState.Running;
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
            _state = ProcessState.Stopped;
        }
        private void StartLoop() => _thread.Start();
        public static bool IsOnePressed(List<Key> keys)
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