namespace MCR_Engine.Core;

public class Clock
{
    private float _seconds;
    public Clock()
    {
        _seconds = 0;
    }
    public int PassedSeconds => (int)_seconds;
    public int PassedMilliSeconds => (int)(_seconds * 1000);
    public void Update(float deltaTime)
    {
        if (deltaTime < 0)
            throw new ArgumentOutOfRangeException(nameof(deltaTime));
        _seconds += deltaTime;
    }
    public void Reset() => _seconds = 0;
}