using System.Diagnostics;
using JumpOn.Interactables;
using MCR_Engine.Core;
using MCR_Engine.Objects;

namespace JumpOn.Entities;

public abstract class Entity : Sprite
{
    protected readonly Clock Clock;
    private Interactable? _currentInteractable;
    private const int DamageTickOffsetMs = 1000;
    private readonly double _maxHealth;
    private double _currentHealth;
    private int _lastDamageTimeMs;
    protected Entity(int x, int y, RGB?[,] texture, int maxHealth, Clock clock) : base(x, y, texture, 1, true, true)
    {
        _maxHealth = maxHealth;
        _currentHealth = maxHealth;
        _lastDamageTimeMs = 0;
        _currentInteractable = null;
        Clock = clock;
    }
    public bool Alive => _currentHealth > 0;
    public double Health => Alive ?  _currentHealth : 0;
    protected abstract double WalkingSpeed { get; }
    protected abstract double ClimbingSpeed { get; }
    protected abstract double HangingSpeed { get; }
    protected override bool Gravity => !(OnLadder || OnBar);
    private bool OnLadder => _currentInteractable is Ladder;
    private bool OnBar => _currentInteractable is Bar;
    private bool OnTopOfLadder => Ladder.IsEntityOnTopOfLadder(this);
    public abstract void Update();
    public void Damage(double damage, bool critical)
    {
        if (damage < 0)
            throw new ArgumentException("Damage cannot be negative");
        bool getDamage = _lastDamageTimeMs + DamageTickOffsetMs < Clock.PassedMilliSeconds;
        if (!getDamage && !critical)
            return;
        if (getDamage)
            Flash(20);
        _currentHealth -= damage;
        _lastDamageTimeMs = Clock.PassedMilliSeconds;
    }
    public void Heal(double heal)
    {
        if (heal < 0)
            throw new ArgumentException("Heal cannot be negative");
        _currentHealth += heal;
        if (_currentHealth > _maxHealth)
            _currentHealth = _maxHealth;
    }
    public void SetInteractable(Interactable? interactable) => _currentInteractable = interactable;
    protected override bool IsFloorBeneath() => base.IsFloorBeneath() || OnTopOfLadder;
    protected bool Walk(Direction direction)
    {
        if (Contains(direction, [Direction.None, Direction.Up, Direction.Down]))
            return false;
        MoveOnX(direction, 229, WalkingSpeed);
        return true;
    }
    protected bool Climb(Direction direction)
    {
        if (!Gravity && OnLadder)
            StopMovingY();
        if (!OnLadder || Contains(direction, [Direction.None, Direction.Left, Direction.Right]))
        {
            if (OnTopOfLadder)
                StopMovingY();
            return false;
        }
        StopMovingX();
        ((Ladder)_currentInteractable!).CenterEntity(this);
        MoveOnY(direction, ClimbingSpeed);
        return true;
    }
    protected bool Hang(Direction direction)
    {
        if (!OnBar)
            return false;
        StopMovingY();
        Y = ((Bar)_currentInteractable!).HangOnIt();
        if (Contains(direction, [Direction.None, Direction.Up]))
            return true;
        if (direction is Direction.Down)
        {
            Y += 2;
            return false;
        }
        MoveOnX(direction, 100, HangingSpeed);
        return true;
    }
    protected bool GetFallDamage(out double fallDamage)
    {
        if (VelocityY > 120 && IsFloorBeneath())
        {
            fallDamage = VelocityY / 100;
            return true;
        }
        fallDamage = 0;
        return false;
    }
    private void MoveOnX(Direction direction, double acceleration, double maxSpeed)
    {
        if (Contains(direction, [Direction.None, Direction.Up, Direction.Down]))
            throw new ArgumentException("Invalid direction");
        AddVelocity(direction, acceleration, maxSpeed);
    }
    private void MoveOnY(Direction direction, double speed)
    {
        if (Contains(direction, [Direction.None, Direction.Left, Direction.Right]))
            throw new ArgumentException("Invalid direction");
        SetNewVelocity(direction, speed);
    }
    public static void Update(List<Entity> entities)
    {
        foreach (Entity entity in entities)
            entity.Update();
    }
    public static void ResetInteractable(List<Entity> entities)
    {
        foreach (Entity entity in entities)
            entity._currentInteractable =  null;
    }
    public static List<Entity> LoadEntities(SpriteManager sprites) => sprites.GetAllSpritesOfType<Entity>();
    private static bool Contains<T>(T searchedType, T[] types) => types.Contains(searchedType);
}