using System.Diagnostics;
using JumpOn.Interactables;
using MCR_Engine.Core;
using MCR_Engine.Objects;

namespace JumpOn.Entities;

public abstract class Entity : Sprite
{
    protected Interactable? CurrentInteractable;
    private const int DamageTickOffsetMs = 1000;
    private readonly double _maxHealth;
    private readonly Clock _clock;
    private double _currentHealth;
    private int _lastDamageTimeMs;
    protected Entity(int x, int y, RGB?[,] texture, int maxHealth, Clock clock) : base(x, y, texture, 1, true, true)
    {
        _maxHealth = maxHealth;
        _currentHealth = maxHealth;
        _lastDamageTimeMs = 0;
        CurrentInteractable = null;
        _clock = clock;
    }
    public bool Alive => _currentHealth > 0;
    public double Health => Alive ?  _currentHealth : 0;
    protected abstract double WalkingSpeed { get; }
    protected abstract double ClimbingSpeed { get; }
    protected abstract double HangingSpeed { get; }
    protected override bool Gravity => !(OnLadder || OnBar);
    protected bool OnLadder => CurrentInteractable is Ladder;
    protected bool OnBar => CurrentInteractable is Bar;
    public abstract void Update();
    public void Damage(double damage, bool critical)
    {
        if (damage < 0)
            throw new ArgumentException("Damage cannot be negative");
        if (_lastDamageTimeMs + DamageTickOffsetMs >= _clock.PassedMilliSeconds && !critical)
            return;
        _currentHealth -= damage;
        _lastDamageTimeMs = _clock.PassedMilliSeconds;
    }
    public void Heal(double heal)
    {
        if (heal < 0)
            throw new ArgumentException("Heal cannot be negative");
        _currentHealth += heal;
        if (_currentHealth > _maxHealth)
            _currentHealth = _maxHealth;
    }
    public void SetInteractable(Interactable? interactable) => CurrentInteractable = interactable;
    protected bool Walk(Direction direction)
    {
        if (Contains(direction, [Direction.None, Direction.Up, Direction.Down]))
            return false;
        MoveOnX(direction, 220,WalkingSpeed);
        return true;
    }
    protected bool Climb(Direction direction)
    {
        if (!OnLadder || Contains(direction, [Direction.None, Direction.Left, Direction.Right]))
            return false;
        StopMovingX();
        ((Ladder)CurrentInteractable!).CenterEntity(this);
        MoveOnY(direction, ClimbingSpeed);
        return true;
    }
    protected bool Hang(Direction direction)
    {
        if (!OnBar)
            return false;
        StopMovingY();
        Y = ((Bar)CurrentInteractable!).HangOnIt();
        if (Contains(direction, [Direction.None, Direction.Up]))
            return true;
        if (direction is Direction.Down)
        {
            Y += 2;
            return false;
        }
        SetNewVelocity(direction, HangingSpeed);
        return true;
    }
    private void MoveOnX(Direction direction, double acceleration, double maxSpeed)
    {
        if (Contains(direction, [Direction.None, Direction.Up, Direction.Down]))
            throw new ArgumentException("Invalid direction");
        //SetNewVelocity(direction, maxSpeed);
        AddVelocity(direction, acceleration, maxSpeed);
    }
    private void MoveOnY(Direction direction, double speed)
    {
        if (Contains(direction, [Direction.None, Direction.Left, Direction.Right]))
            throw new ArgumentException("Invalid direction");
        SetNewVelocity(direction, speed);
    }
    private bool GetFallDamage(out double fallDamage)
    {
        fallDamage = 0;
        return false;
    }
    public static void Update(List<Entity> entities)
    {
        foreach (Entity entity in entities)
            entity.Update();
    }
    public static List<Entity> LoadEntities(SpriteManager sprites) => sprites.GetAllSpritesOfType<Entity>();
    private static bool Contains<T>(T searchedType, T[] types) => types.Contains(searchedType);
}