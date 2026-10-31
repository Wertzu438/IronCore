using System.Diagnostics;
using JumpOn.Entities.LivingEntities;
using JumpOn.Interactables;
using MCR_Engine.Core;
using MCR_Engine.Core.BaseObjectTypes;
using MCR_Engine.Objects;

namespace JumpOn.Entities;

public abstract class Entity : MovingObject
{
    private const int DamageTickOffsetMs = 850;
    protected readonly Clock Clock;
    protected readonly double MaxHealth;
    private double _currentHealth;
    private int _lastFloorY;
    private int _lastDamageTimeMs;
    protected Entity(int x, int y, double maxHealth, Clock clock) : base(x, y, 1, true, Textures.DebuggingTexture(6, 4))
    {
        MaxHealth = maxHealth;
        _currentHealth = maxHealth;
        _lastDamageTimeMs = int.MinValue;
        Clock = clock;
        _lastFloorY = 0;
    }
    public new int Y
    {
        get => base.Y;
        set => base.Y = _lastFloorY = value;
    }
    public bool Alive => _currentHealth > 0;

    public double Health
    {
        get => Alive ? _currentHealth : 0;
        set
        {
            if (value < 0 && value >= MaxHealth)
                throw new ArgumentException("Health cannot be less than 0 or equal to MaxHealth");
            _currentHealth = value;
        }
    }
    protected virtual bool ResetFallDamage => IsFloorBeneath();
    public abstract void Update();
    public virtual bool Damage(double damage, bool critical)
    {
        bool getDamage = _lastDamageTimeMs + DamageTickOffsetMs < Clock.PassedMilliSeconds;
        if (!getDamage && !critical)
            return false;
        _currentHealth -= Math.Abs(damage);
        _lastDamageTimeMs = Clock.PassedMilliSeconds;
        if (getDamage && Alive)
            Flash(20);
        return true;
    }
    public virtual void Heal(double heal)
    {
        _currentHealth += Math.Abs(heal);
        if (_currentHealth > MaxHealth)
            _currentHealth = MaxHealth;
    }
    protected virtual bool GetFallDamage(out double fallDamage)
    {
        int distance = Y - _lastFloorY;
        if (distance > 200)
        {
            fallDamage = double.MaxValue;
            return true;
        }
        if (ResetFallDamage)
        {
            if (distance > 20)
            {
                fallDamage = distance / 20.0;
                _lastFloorY = Y;
                return true;
            }
            _lastFloorY = Y;
        }
        fallDamage = 0;
        return false;
    }
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
    public static void Update(List<Entity> entities, Scene scene)
    {
        foreach (Entity entity in entities)
        {
            entity.Update();
            if (entity is LivingEntity)
                ((LivingEntity)entity).SetInteractable(null);
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
    public static List<Entity> LoadEntities(ObjectManager objects) => objects.GetAllObjectsOfType<Entity>();
}