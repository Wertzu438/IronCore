namespace JumpOn.Core;

public struct LevelStats
{
    public int Tries;
    public int? Time;
    public int? Respawns;
    public LevelStats(int tries, int? time, int? respawns)
    {
        Tries = tries;
        Time = time;
        Respawns = respawns;
    }
    public LevelStats()
    {
        Tries = 0;
        Time = null;
        Respawns = null;
    }
    public bool LevelFinished => Time.HasValue && Respawns.HasValue;
    public bool LevelStarted => Tries > 0;
    public void AddTry() => Tries++;
}
public class Account
{
    private readonly string _name;
    private readonly LevelStats[] _stats;
    private int _currentLevel;
    public Account(string name, int levelCount)
    {
        _name = name;
        _stats = new LevelStats[levelCount];
        _stats.Initialize();
        _currentLevel = 0;
    }
    public string Name => _name;
    public int MaxLevel => GetMaxLevel();
    public int CurrentLevel
    {
        get => _currentLevel;
        set => _currentLevel = (value < 0 ? MaxLevel : (value > MaxLevel ? 0 : value));
    }
    public LevelStats GetStats(int level) => _stats[level];
    public void UpdateLevelStats(int level, int? time, int? respawns)
    {
        _stats[level].AddTry();
        if (!(time.HasValue && respawns.HasValue))
            return;
        if (_stats[level].Time < time)
            return;
        _stats[level].Time = time.Value;
        _stats[level].Respawns = respawns;
    }
    private int GetMaxLevel()
    {
        for (int level = 0; level < _stats.Length; level++)
            if (!_stats[level].LevelFinished)
                return level;
        return 0;
    }
}