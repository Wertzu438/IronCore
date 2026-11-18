using MCR_Engine.Core;

namespace MCR_Engine.ObjectSystem.BaseObjectTypes;

public abstract class MovingObject : GameObject
{
    protected double VelocityX, VelocityY;
    private readonly RGB?[,] _texture;
    protected MovingObject(int x, int y, int layer, bool collision, RGB?[,] texture, bool visible = true) : base(x, y, texture, layer, collision)
    {
        VelocityX = VelocityY = 0;
        _texture = texture;
        ChangeVisibility(visible);
    }
    public bool Move => Math.Abs(VelocityX) + Math.Abs(VelocityY) > 0;
    protected virtual bool StopOnCollisionX => true;
    protected virtual bool StopOnCollisionY => true;
    public override void TickUpdate() => MoveAndCheckForCollision(VelocityX, VelocityY);
    public void AddVelocity(Direction direction, double acceleration, double maxVelocity)
    {
        ArgumentNullException.ThrowIfNull(Objects);
        if (acceleration <= 0 || maxVelocity <= 0)
            throw new ArgumentException("Invalid acceleration or maxVelocity.");
        switch (direction)
        {
            case Direction.Up: VelocityY = Math.Max(VelocityY - acceleration * Objects.DeltaTime, -maxVelocity); break;
            case Direction.Down: VelocityY = Math.Min(VelocityY + acceleration * Objects.DeltaTime, maxVelocity); break;
            case Direction.Left: VelocityX = Math.Max(VelocityX - acceleration * Objects.DeltaTime, -maxVelocity); break;
            case Direction.Right: VelocityX = Math.Min(VelocityX + acceleration * Objects.DeltaTime, maxVelocity); break;
            default: throw new ArgumentException("Invalid direction.");
        }
    }
    public void SetNewVelocity(Direction direction, double velocity)
    {
        if (velocity < 0)
            throw new ArgumentException("Velocity must be greater than or equal to 0.");
        switch (direction)
        {
            case Direction.Up: VelocityY = -velocity; break;
            case Direction.Down: VelocityY = velocity; break;
            case Direction.Left: VelocityX = -velocity; break;
            case Direction.Right: VelocityX = velocity; break;
            default: throw new ArgumentException("Invalid direction.");
        }
    }
    public virtual (double X, double Y) GetVelocity() => (VelocityX, VelocityY);
    public void StopMovingX() => VelocityX = 0;
    public void StopMovingY() => VelocityY = 0;
    protected void MoveAndCheckForCollision(double velocityX, double velocityY)
    {
        ArgumentNullException.ThrowIfNull(Objects);
        double[] velocityStepsX = GetVelocitySteps(velocityX * Objects.DeltaTime);
        double[] velocityStepsY = GetVelocitySteps(velocityY * Objects.DeltaTime);
        for (int i = 0; i < Math.Max(velocityStepsX.Length, velocityStepsY.Length); i++)
        {
            if (i < velocityStepsY.Length)
            {
                if (!TryMoveY(velocityStepsY[i]) && StopOnCollisionY)
                    StopMovingY();
            }
            if (i < velocityStepsX.Length)
            {
                if (!TryMoveX(velocityStepsX[i]) && StopOnCollisionX)
                    StopMovingX();
            }
        }
    }
    private bool TryMoveX(double xAdd)
    {
        double xOld = CurrentX;
        CurrentX += xAdd;
        if (!HasCollision || !IsBordersCollision(out _))
            return true;
        CurrentX = xOld;
        return false;
    }
    private bool TryMoveY(double yAdd)
    {
        double yOld = CurrentY;
        CurrentY += yAdd;
        if (!IsBordersCollision(out _))
            return true;
        CurrentY = yOld;
        return false;
    }
    private static double[] GetVelocitySteps(double velocity)
    {
        double[] velocitySteps = new double[Math.Abs((int)velocity) + 1];
        double velocityPerStep = velocity / velocitySteps.Length;
        for (int i = 0; i < velocitySteps.Length; i++)
        {
            velocitySteps[i] = velocityPerStep;
        }
        return velocitySteps;
    }
}