namespace MCR_Engine.Core;

public class Clock
{
    private int _seconds;
    private float _milliSeconds;
    public Clock()
    {
        _seconds = 0;
        _milliSeconds = 0;
    }
    public int PassedSeconds => _seconds;
    public int PassedMilliSeconds => (int)_milliSeconds + _seconds * 1000;
    public void Update(float stoppedMilliseconds)
    {
        _milliSeconds += stoppedMilliseconds;
        while (_milliSeconds >= 1000)
        {
            _seconds++;
            _milliSeconds -= 1000;
        }
    }
    public void Reset()
    {
        _seconds = 0;
        _milliSeconds = 0;
    }
}