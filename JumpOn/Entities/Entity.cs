using JumpOn.Interactables;
using MCR_Engine.Core;
using MCR_Engine.Objects;

namespace JumpOn.Entities;

public abstract class Entity : Sprite
{
    private const int DamageTickOffsetMs = 700;
    private readonly double _maxHealth;
    private readonly Clock _clock;
    private double _currentHealth;
    private int _lastDamageTimeMs;
    private Interactable? _currentInteractable;
    public Entity(int x, int y, RGB?[,] texture, int maxHealth, Clock clock) : base(x, y, texture, 1, true, true)
    {
        _maxHealth = maxHealth;
        _currentHealth = maxHealth;
        _clock = clock;
        _lastDamageTimeMs = 0;
        _currentInteractable = null;
    }
    public bool Alive => _currentHealth > 0;
    public Interactable? CurrentInteractable
    {
        get => _currentInteractable; set => _currentInteractable = value;
    }
    protected abstract double WalkingSpeed { get; }
    protected abstract double ClimbingSpeed { get; }
    protected abstract double HangingSpeed { get; }
    protected override bool Gravity => !(OnLadder || OnBar);
    protected bool OnLadder => _currentInteractable is Ladder;
    protected bool OnBar => _currentInteractable is Bar;
    public abstract void Update();
    public void Damage (double damage, bool critical)
    {
        if (damage < 0)
            throw new ArgumentException("Damage cannot be negative");
        if (_lastDamageTimeMs + DamageTickOffsetMs < _clock.PassedMilliSeconds && !critical)
            return;
        _currentHealth -= damage;
        _lastDamageTimeMs = _clock.PassedMilliSeconds;
    }
    public void Heal (double heal)
    {
        if (heal < 0)
            throw new ArgumentException("Heal cannot be negative");
        _currentHealth += heal;
        if (_currentHealth > _maxHealth)
            _currentHealth = _maxHealth;
    }
    protected bool Walk(Direction direction)
    {
        if (Contains(direction, [Direction.None, Direction.Up, Direction.Down]))
            return false;
        MoveOnX(direction, 65,WalkingSpeed);
        return true;
    }
    protected bool Climb(Direction direction)
    {
        if (!OnLadder || Contains(direction, [Direction.None, Direction.Left, Direction.Right]))
            return false;
        X = ((Ladder)_currentInteractable!).CenterPlayer(Width);
        MoveOnY(direction, ClimbingSpeed);
        return true;
    }
    protected bool Hang(Direction direction)
    {
        if (!OnBar)
            return false;
        Y = ((Bar)_currentInteractable!).HangOnIt();
        if (Contains(direction, [Direction.None, Direction.Up]))
            return true;
        if (direction is Direction.Down)
        {
            Y += 2;
            CurrentInteractable = null;
            return false;
        }
        MoveOnX(direction, 1,HangingSpeed);
        return true;
    }
    private void MoveOnX(Direction direction, double acceleration, double maxSpeed)
    {
        if (Contains(direction, [Direction.None, Direction.Up, Direction.Down]))
            throw new ArgumentException("Invalid direction");
        AddVelocity(direction, acceleration, maxSpeed);
        //AddMoveToQueue(speed * (direction is Direction.Left ? -1 : 1), 0, 1);
    }
    private void MoveOnY(Direction direction, double speed)
    {
        if (Contains(direction, [Direction.None, Direction.Left, Direction.Right]))
            throw new ArgumentException("Invalid direction");
        //AddMoveToQueue(0, speed * (direction is Direction.Up ? -1 : 1), 1);
    }
    private bool GetFallDamage(out double fallDamage)
    {
        fallDamage = 0;
        return false;
    }
    private static bool Contains<T>(T searchedType, T[] types) => types.Contains(searchedType);
}