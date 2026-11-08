namespace MCR_Engine.InputSystem;

public class Key
{
    private readonly int _keyCode;
    private bool _pressed;
    private bool _virtualPressed;
    public Key(int keyCode)
    {
        _keyCode = keyCode;
        _pressed = false;
        _virtualPressed = false;
    }
    public int Code => _keyCode;
    public bool Pressed
    {
        get
        {
            lock (this)
                return _pressed || _virtualPressed;
        }
        set
        {
            lock (this)
                _pressed = value;
        }
    }
    public async Task PressVirtual(int milliSeconds)
    {
        _virtualPressed = true;
        await Task.Delay(milliSeconds);
        _virtualPressed = false;
    }
}