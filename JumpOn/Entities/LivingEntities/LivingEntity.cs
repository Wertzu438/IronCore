using JumpOn.Entities.OtherEntities;
using JumpOn.Interactables;
using MCR_Engine.Core;

namespace JumpOn.Entities.LivingEntities;

public abstract class LivingEntity : Entity
{
    protected readonly RGB?[,] DeadTexture;
    private Interactable? _currentInteractable;
    protected LivingEntity(int x, int y, int maxHealth, Clock clock, Textures textures) : base(x, y, maxHealth, clock)
    {
        DeadTexture = textures.Get("deadEntity");
        _currentInteractable = null;
    }
    protected abstract double WalkingSpeed { get; }
    protected abstract double ClimbingSpeed { get; }
    protected abstract double HangingSpeed { get; }
    protected abstract bool IsInteractingWithInteractable { get; }
    protected override bool StopOnCollisionX => false;
    protected override bool ResetFallDamage => base.ResetFallDamage || OnLadder || OnBar;
    private bool OnBar => _currentInteractable is Bar;
    private bool OnLadder => _currentInteractable is Ladder;
    public abstract override void Update();
    public void SetInteractable(Interactable? interactable) => _currentInteractable = interactable;
    protected override bool GetFallDamage(out double fallDamage) => base.GetFallDamage(out fallDamage) && !OnBar;
    protected override bool IsFloorBeneath() => base.IsFloorBeneath() || Ladder.IsEntityOnTopOfLadder(this) || GetDistanceToNextObject(Direction.Down, false, [typeof(MovingPlatform)]) == 0;
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
        if (Ladder.IsEntityOnTopOfLadder(this) && direction is Direction.Down)
            Y++;
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
}