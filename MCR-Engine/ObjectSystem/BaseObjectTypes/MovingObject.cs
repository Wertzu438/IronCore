using MCR_Engine.Core;

namespace MCR_Engine.ObjectSystem.BaseObjectTypes;

public class MovingObject : GameObject
{
    private const double DefaultDeceleration = 90;
    private const double DefaultGravity = 170, MaxGravity = 150;
    protected double VelocityX, VelocityY;
    private readonly RGB?[,] _texture;
    private MovingObject? _objectBeneath;
    public MovingObject(int x, int y, int layer, bool collision, bool gravity, RGB?[,] texture, bool visible = true) : base(x, y, texture, layer, collision, gravity)
    {
        VelocityX = VelocityY = 0;
        _texture = texture;
        ChangeVisibility(visible);
        _objectBeneath = null;
    }
    public bool Move => Math.Abs(VelocityX) + Math.Abs(VelocityY) > 0;
    protected virtual double CombinedVelocityX => VelocityX + (_objectBeneath == null ? 0 : _objectBeneath.CombinedVelocityX);
    protected virtual double CombinedVelocityY => VelocityY + (_objectBeneath == null ? 0 : _objectBeneath.CombinedVelocityY);
    protected int MovingObjectsBeneath
    {
        get
        {
            int amount = 0;
            MovingObject? a = _objectBeneath;
            while (a != null)
            {
                amount++;
                a = a.ObjectBeneath;
            }
            return amount;
        }
    }
    private MovingObject? ObjectBeneath => _objectBeneath;
    public override void TickUpdate()
    {
        if (Gravity) 
            HandleGravity();
        if (!Move) 
            return;
        if (GetDistanceToNextObject(Direction.Down, false, [typeof(MovingObject)]) == 0)
        {
            _objectBeneath = GetNearestObject(Direction.Down, [typeof(MovingObject)]) as MovingObject;
        }
        else _objectBeneath = null;
        if (Gravity) 
            HandleGravity();
        MoveAndCheckForCollision(CombinedVelocityX, CombinedVelocityY);
        ApplyDeceleration();
    }
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
    public (double X, double Y) GetVelocity(bool removeDeceleration = false)
    {
        if (!removeDeceleration)
            return (CombinedVelocityX, CombinedVelocityY);
        ArgumentNullException.ThrowIfNull(Objects);
        double vX = 0, vY = 0;
        if (CombinedVelocityX > 0)
            vX = CombinedVelocityX + DefaultDeceleration * Objects.DeltaTime;
        else if (CombinedVelocityX < 0)
            vX = CombinedVelocityX - DefaultDeceleration * Objects.DeltaTime;
        
        if (CombinedVelocityY > 0)
            vY = CombinedVelocityY + DefaultDeceleration * Objects.DeltaTime;
        else if (CombinedVelocityY < 0)
            vY = CombinedVelocityY - DefaultDeceleration * Objects.DeltaTime;
        return (vX, vY);
    }
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
    protected virtual bool IsFloorBeneath() => Gravity && GetDistanceToNextObject(Direction.Down, true) is 0;
    private void HandleGravity()
    {
        if (!Gravity)
            throw new InvalidOperationException("Gravity is set to false or object has no collision.");
        if (IsFloorBeneath())
        {
            if (VelocityY > 0)
                StopMovingY();
            return;
        }
        AddVelocity(Direction.Down, DefaultGravity, MaxGravity);
    }
    private void ApplyDeceleration()
    {
        ArgumentNullException.ThrowIfNull(Objects);
        if (VelocityY > 0)
            VelocityY = Math.Max(VelocityY - DefaultDeceleration * Objects.DeltaTime, 0);
        else if (VelocityY < 0)
            VelocityY = Math.Min(VelocityY + DefaultDeceleration * Objects.DeltaTime, 0);

        bool inAir = !IsFloorBeneath();
        
        if (VelocityX > 0)
            VelocityX = Math.Max(VelocityX - (inAir ? DefaultDeceleration / 3 : DefaultDeceleration) * Objects.DeltaTime, 0);
        else if (VelocityX < 0)
            VelocityX = Math.Min(VelocityX + (inAir ? DefaultDeceleration / 3 : DefaultDeceleration) * Objects.DeltaTime, 0);
    }
    private bool TryMoveX(double xAdd)
    {
        double xOld = CurrentX;
        CurrentX += xAdd;
        if (!Collision || !IsBordersCollision(out _))
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
    public override GameObject? Clone(Objects objects) => new MovingObject(X, Y, Layer, Collision, Gravity, _texture, Visible);
}