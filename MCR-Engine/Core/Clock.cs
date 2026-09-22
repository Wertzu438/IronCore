namespace MCR_Engine.Core;

public class Clock
{
    private int _seconds;
    private int _milliSeconds;
    public Clock()
    {
        _seconds = 0;
        _milliSeconds = 0;
    }
    public int PassedSeconds => _seconds;
    public int PassedMilliSeconds => _milliSeconds + _seconds * 1000;
    public void Update(int stopTime)
    {
        _milliSeconds += stopTime;
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