using System.Diagnostics;
using JumpOn.Interactables;
using MCR_Engine.Core;
using MCR_Engine.Objects;

namespace JumpOn.Entities;

public abstract class Entity : Sprite
{
    private const int DamageTickOffsetMs = 850;
    protected readonly RGB?[,] DeadTexture;
    private readonly Clock _clock;
    private readonly double _maxHealth;
    private Interactable? _currentInteractable;
    private double _currentHealth;
    private int _lastFloorY;
    private int _lastDamageTimeMs;
    protected Entity(int x, int y, Textures textures, int maxHealth, Clock clock) : base(x, y, Textures.DebuggingTexture(1, 1), 1, true, true)
    {
        _maxHealth = maxHealth;
        _currentHealth = maxHealth;
        _lastDamageTimeMs = int.MinValue;
        _currentInteractable = null;
        _clock = clock;
        _lastFloorY = 0;
        DeadTexture = textures.Get("deadEntity");
    }
    public new int Y
    {
        get => base.Y;
        set => base.Y = _lastFloorY = value;
    }
    public bool Alive => _currentHealth > 0;
    public double Health => Alive ? _currentHealth : 0;
    protected abstract double WalkingSpeed { get; }
    protected abstract double ClimbingSpeed { get; }
    protected abstract double HangingSpeed { get; }
    protected abstract bool IsInteractingWithInteractable { get; }
    protected override bool Gravity => !IsInteractingWithInteractable;
    private bool OnLadder => _currentInteractable is Ladder;
    private bool OnBar => _currentInteractable is Bar;
    private bool OnTopOfLadder => Ladder.IsEntityOnTopOfLadder(this);
    public abstract void Update();
    public void Damage(double damage, bool critical)
    {
        if (damage < 0)
            throw new ArgumentException("Damage cannot be negative");
        bool getDamage = _lastDamageTimeMs + DamageTickOffsetMs < _clock.PassedMilliSeconds;
        if (!getDamage && !critical)
            return;
        _currentHealth -= damage;
        _lastDamageTimeMs = _clock.PassedMilliSeconds;
        if (getDamage && Alive)
            Flash(20);
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
        if (Engine.Contains(direction, [Direction.None, Direction.Up, Direction.Down]))
            return false;
        MoveOnX(direction, 229, WalkingSpeed);
        return true;
    }
    protected bool Climb(Direction direction)
    {
        if (!OnLadder || Engine.Contains(direction, [Direction.None, Direction.Left, Direction.Right]))
        {
            if (!OnLadder || !IsInteractingWithInteractable)
                return false;
            if (Engine.Contains(direction, [Direction.Left, Direction.Right]))
                MoveOnX(direction, 299, ClimbingSpeed);
            return !IsFloorBeneath();
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
        if (Engine.Contains(direction, [Direction.None, Direction.Up]))
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
        int distance = Y - _lastFloorY;
        if (distance > 200)
        {
            fallDamage = double.MaxValue;
            return true;
        }
        if (IsFloorBeneath() || OnLadder || OnBar)
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
    private void MoveOnX(Direction direction, double acceleration, double maxSpeed)
    {
        if (Engine.Contains(direction, [Direction.None, Direction.Up, Direction.Down]))
            throw new ArgumentException("Invalid direction");
        AddVelocity(direction, acceleration, maxSpeed);
    }
    private void MoveOnY(Direction direction, double speed)
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
            entity._currentInteractable =  null;
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
    public static List<Entity> LoadEntities(SpriteManager sprites) => sprites.GetAllSpritesOfType<Entity>();
}