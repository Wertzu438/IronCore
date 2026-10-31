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
    private readonly Clock _clock;
    private readonly double _maxHealth;
    private double _currentHealth;
    private int _lastFloorY;
    private int _lastDamageTimeMs;
    protected Entity(int x, int y, int maxHealth, Clock clock) : base(x, y, 1, true, Textures.DebuggingTexture(6, 4))
    {
        _maxHealth = maxHealth;
        _currentHealth = maxHealth;
        _lastDamageTimeMs = int.MinValue;
        _clock = clock;
        _lastFloorY = 0;
    }
    public new int Y
    {
        get => base.Y;
        set => base.Y = _lastFloorY = value;
    }
    public bool Alive => _currentHealth > 0;
    public double Health => Alive ? _currentHealth : 0;
    protected virtual bool ResetFallDamage => IsFloorBeneath();
    public abstract void Update();
    public virtual bool Damage(double damage, bool critical)
    {
        bool getDamage = _lastDamageTimeMs + DamageTickOffsetMs < _clock.PassedMilliSeconds;
        if (!getDamage && !critical)
            return false;
        _currentHealth -= Math.Abs(damage);
        _lastDamageTimeMs = _clock.PassedMilliSeconds;
        if (getDamage && Alive)
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
            if (!entity.OnScene(Direction.Right))
            {
                if (entity.X < 0)
                {
                    entity.X = 0;
                }
                else entity.X = scene.Width - entity.Width;
                entity.StopMovingX();
            }
            
        }
    }
    public static List<Entity> LoadEntities(ObjectManager objects) => objects.GetAllObjectsOfType<Entity>();
}