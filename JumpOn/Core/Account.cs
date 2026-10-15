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
    public bool LevelFinished => Time.HasValue && Respawns.HasValue;
    public bool LevelStarted => Tries > 0;
    public void AddTry() => Tries++;
}
public class Account
{
    private const string FileName = "Account/account.txt";
    private readonly LevelStats[] _stats;
    private string? _name;
    private int _currentLevel;
    public Account(string? name, int levelCount)
    {
        _stats = new LevelStats[levelCount];
        _stats.Initialize();
        Name = name;
        _currentLevel = 0;
    }
    public string? Name
    {
        get => _name ?? throw new InvalidOperationException();
        set
        {
            if (value == null)
            {
                _name = value;
                for (int i = 0; i < _stats.Length; i++)
                {
                    _stats[i] = new LevelStats(0, null, null);
                }
                return;
            }
            _name = value.Trim();
            if (_name.Length >= 20)
                _name = "??";
        }
    }
    public bool ValidName => !string.IsNullOrEmpty(_name);
    public int MaxLevel => GetMaxLevel();
    public int CurrentLevel
    {
        get => _currentLevel;
        set => _currentLevel = (value < 0 ? MaxLevel : (value > MaxLevel ? 0 : value));
    }
    public LevelStats GetStats(int level) => _stats[level];
    public void SaveAccount()
    {
        int maxLevel = GetMaxLevel();
        string[] lines = new string[maxLevel + 2];
        lines[0] = Name;
        for (int i = 0; i < maxLevel + 1; i++)
        {
            if (_stats[i].LevelFinished)
            {
                lines[i+ 1] = $"{_stats[i].Tries};{_stats[i].Time};{_stats[i].Respawns}";
            }
            else lines[i + 1] = $"{_stats[i].Tries};null;null";
        }
        File.WriteAllLines(FileName, lines);
    }
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
        return _stats.Length - 1;
    }
    public static Account GetAccount(int levelCount)
    {
        Account account;
        if (File.Exists(FileName))
        {
            string[] lines = File.ReadAllLines(FileName);
            account = new Account(lines[0], levelCount);
            for (int i = 1; i < lines.Length; i++)
            {
                string[] parts = lines[i].Split(';');
                account._stats[i - 1] = new LevelStats(
                    int.Parse(parts[0]),
                    int.TryParse(parts[1], out int time) ? time : null, 
                    int.TryParse(parts[2], out int respawns) ? respawns : null);
            }
        }
        else
        {
            account = new Account(null, levelCount);
        }
        return account;
    }
}