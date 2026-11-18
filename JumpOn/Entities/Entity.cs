using JumpOn.Entities.LivingEntities;
using JumpOn.Entities.OtherEntities;
using MCR_Engine.Core;
using MCR_Engine.ObjectSystem;
using MCR_Engine.ObjectSystem.BaseObjectTypes;

namespace JumpOn.Entities;

public abstract class Entity : GravityObject
{
    private const int DamageTickOffsetMs = 850;
    protected readonly Clock Clock;
    protected int LastFloorY;
    private readonly double _maxHealth;
    private double _currentHealth;
    private int _lastDamageTimeMs;
    protected Entity(int x, int y, double maxHealth, Clock clock) : base(x, y, 1, Textures.DebuggingTexture(6, 4))
    {
        _maxHealth = maxHealth;
        _currentHealth = maxHealth;
        Clock = clock;
        _lastDamageTimeMs = Clock.PassedMilliSeconds;
        LastFloorY = y;
    }
    public new int Y
    {
        get => base.Y;
        set => base.Y = LastFloorY = value;
    }
    public bool Alive => _currentHealth > 0;
    public double Health => Alive ? _currentHealth : 0;
    protected virtual bool ResetFallDamage => IsFloorBeneath();
    public abstract void Update();
    public virtual bool Damage(double damage, bool critical, bool flash)
    {
        bool getDamage = _lastDamageTimeMs + DamageTickOffsetMs < Clock.PassedMilliSeconds;
        if (!getDamage && !critical)
            return false;
        _currentHealth -= Math.Abs(damage);
        _lastDamageTimeMs = Clock.PassedMilliSeconds;
        if (getDamage && Alive && flash)
            Flash(20);
        return true;
    }
    public virtual void Heal(double heal)
    {
        _currentHealth += Math.Abs(heal);
        if (_currentHealth > _maxHealth)
            _currentHealth = _maxHealth;
    }
    protected virtual bool GetFallDamage(out double fallDamage)
    {
        int distance = Y - LastFloorY;
        if (distance > 100)
        {
            fallDamage = _maxHealth;
            return true;
        }
        if (ResetFallDamage)
        {
            if (distance > 20)
            {
                fallDamage = distance / 20.0;
                LastFloorY = Y;
                return true;
            }
            LastFloorY = Y;
        }
        fallDamage = 0;
        return false;
    }
    protected override bool IsFloorBeneath() => base.IsFloorBeneath() || GetDistanceToNextObject(Direction.Down, false, [typeof(MovingPlatform)]) is 0;
    protected void MoveOnX(Direction direction, double acceleration, double maxSpeed)
    {
        if (Engine.Contains(direction, [Direction.None, Direction.Up, Direction.Down]))
            throw new ArgumentException("Invalid direction");
        AddVelocity(direction, acceleration, maxSpeed);
    }
    protected void MoveOnY(Direction direction, double speed)
    {
        if (Engine.Contains(direction, [Direction.None, Direction.Left, Direction.Right]))
            throw new ArgumentException("Invalid direction");
        SetNewVelocity(direction, speed);
    }
    public static void Update(Entity[] entities, Scene scene)
    {
        foreach (Entity entity in entities)
        {
            entity.Update();
            if (entity is LivingEntity livingEntity)
                livingEntity.SetInteractable(null);
            if (entity.OnScene(Direction.Right)) 
                continue;
            if (entity.X < 0)
            {
                entity.X = 0;
            }
            else entity.X = scene.Width - entity.Width;
            entity.StopMovingX();
        }
    }
    public static Entity[] LoadEntities(Objects objects) => objects.GetAllObjectsOfType<Entity>();
}