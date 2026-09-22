namespace MCR_Engine.InputSystem;

public class Key
{
    private readonly int _keyCode;
    private bool _pressed;
    public Key(int keyCode)
    {
        _keyCode = keyCode;
        _pressed = false;
    }
    public int Code => _keyCode;
    public bool Pressed
    {
        get
        {
            lock (this)
                return _pressed;
        }
        set
        {
            lock (this)
                _pressed = value;
        }
    }
}