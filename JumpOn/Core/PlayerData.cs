namespace JumpOn.Core;

public struct PlayerData
{
    private int _respawns;
    private double _takenDamage;
    private double _takenFallDamage;
    private double _healedDamage;
    public PlayerData()
    {
        _respawns = 0;
        _takenDamage = 0;
        _takenFallDamage = 0;
        _healedDamage = 0;
    }
    public int Respawns => _respawns < 0 ? 0 : _respawns;
    public int TakenTrapDamage => (int)(_takenDamage - _takenFallDamage);
    public int TakenFallDamage => (int)_takenFallDamage;
    public int TakenDamage => TakenFallDamage + TakenTrapDamage;
    public int HealedDamage => (int)_healedDamage;
    public void AddRespawn() => _respawns++;
    public void AddTakenDamage(double amount) => 
        _takenDamage += amount > 0 ? amount : throw new ArgumentOutOfRangeException(nameof(amount));
    public void AddTakenFallDamage(double amount) => 
        _takenFallDamage += amount > 0 ? amount : throw new ArgumentOutOfRangeException(nameof(amount));
    public void AddHealedDamage(double amount) =>
        _healedDamage += amount > 0 ? amount : throw new ArgumentOutOfRangeException(nameof(amount));
}