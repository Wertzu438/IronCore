namespace JumpOn;

public struct LevelStats(int tries, int? time)
{
    public int Tries = tries;
    public int? Time = time;
    public bool LevelFinished => Time is not null;
}
public class Account
{
    private readonly string _name;
    private readonly LevelStats?[] _stats;
    private int _currentLevel;
    public Account(string name, int levelCount)
    {
        this._name = name;
        this._stats = new LevelStats?[levelCount];
        this._currentLevel = 0;
    }
    public string Name => _name;
    public int MaxLevel => GetMaxLevel();
    public int CurrentLevel
    {
        get => _currentLevel;
        set => _currentLevel = (value < 0 ? MaxLevel : (value > MaxLevel ? 0 : value));
    }
    public LevelStats? GetStats(int level) => _stats[level];
    public void UpdateLevelStats(int level, int? time)
    {
        LevelStats? levelStats = _stats[level];
        if (levelStats.HasValue)
        {
            if (levelStats.Value.LevelFinished)
            {
                if (time is null)
                    return;
                _stats[level] = new LevelStats(levelStats.Value.Tries, time);
            }
            else _stats[level] = new LevelStats(levelStats.Value.Tries + 1, time);
        }
        else _stats[level] = new LevelStats(1, time);
    }
    private int GetMaxLevel()
    {
        for (int level = 0; level < _stats.Length; level++)
            if (_stats[level] is null || !_stats[level].Value.LevelFinished)
                return level;
        return 0;
    }
}