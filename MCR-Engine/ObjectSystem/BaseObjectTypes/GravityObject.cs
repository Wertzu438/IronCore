using MCR_Engine.Core;

namespace MCR_Engine.ObjectSystem.BaseObjectTypes;

public abstract class GravityObject : MovingObject
{
    private const double DefaultDeceleration = 90;
    private const double DefaultGravity = 340, MaxGravity = 150;
    private MovingObject? _objectBeneath;
    protected GravityObject(int x, int y, int layer, RGB?[,] texture) : base(x, y, layer, true, texture)
    {
        _objectBeneath = null;   
    }

    protected virtual double CombinedVelocityX => VelocityX + (_objectBeneath is null ? 0  : _objectBeneath.GetVelocity().X);
    protected virtual double CombinedVelocityY => VelocityY + (_objectBeneath is null ? 0 : _objectBeneath.GetVelocity().Y);
    protected bool PauseGravity { get; set; }
    protected int MovingObjectsBeneath
    {
        get
        {
            int amount = 0;
            GravityObject? a = _objectBeneath as GravityObject;
            while (a != null)
            {
                amount++;
                a = (GravityObject?)a._objectBeneath;
            }
            return amount;
        }
    }
    public override void TickUpdate()
    {
        if (!PauseGravity)
            HandleGravity();
        if (GetDistanceToNextObject(Direction.Down, false, [typeof(MovingObject)]) == 0)
        {
            _objectBeneath = GetNearestObject(Direction.Down, [typeof(MovingObject)]) as MovingObject;
        }
        else _objectBeneath = null;
        MoveAndCheckForCollision(CombinedVelocityX, CombinedVelocityY);
        ApplyDeceleration();
    }
    public override (double X, double Y) GetVelocity() => (CombinedVelocityX, CombinedVelocityY);
    protected virtual bool IsFloorBeneath() => GetDistanceToNextObject(Direction.Down, true) is 0;
    private void HandleGravity()
    {
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
}