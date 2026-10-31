namespace JumpOn.Entities.LivingEntities;

public struct PlayerData
{
    //Heal?
    private int _respawns;
    private double _takenDamage;
    private double _takenFallDamage;
    public PlayerData()
    {
        _respawns = 0;
        _takenDamage = 0;
        _takenFallDamage = 0;
    }
    public int Respawns => _respawns;
    public int TakenTrapDamage => (int)(_takenDamage - _takenFallDamage);
    public int TakenFallDamage => (int)_takenFallDamage;
    public void AddRespawn() => _respawns++;
    public void AddTakenDamage(double amount) => _takenDamage += amount > 0 ? amount : throw new ArgumentOutOfRangeException(nameof(amount));
    public void AddTakenFallDamage(double amount) => _takenFallDamage += amount > 0 ? amount : throw new ArgumentOutOfRangeException(nameof(amount));
}