using MCR_Engine.Core;

namespace MCR_Engine.ObjectSystem.BaseObjectTypes;

public abstract class GravityObject : MovingObject
{
    private const double DefaultDeceleration = 90;
    private const double DefaultGravity = 340, MaxGravity = 150;
    private MovingObject? _objectBeneath;
    private double _objBeneathVelocityX;
    private double _objBeneathVelocityY;
    protected GravityObject(int x, int y, int layer, RGB?[,] texture) : base(x, y, layer, true, texture)
    {
        _objectBeneath = null;   
        _objBeneathVelocityX = _objBeneathVelocityY = 0;
    }

    protected virtual double CombinedVelocityX => VelocityX + (_objectBeneath is null ? _objBeneathVelocityX  : _objectBeneath.GetVelocity().X);
    protected virtual double CombinedVelocityY => VelocityY + (_objectBeneath is null ? _objBeneathVelocityY : _objectBeneath.GetVelocity().Y);
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
            if (_objectBeneath is not null)
            {
                _objBeneathVelocityX = _objectBeneath.GetVelocity().X;
                _objBeneathVelocityY = _objectBeneath.GetVelocity().Y;
            }
        }
        else _objectBeneath = null;
        MoveAndCheckForCollision(CombinedVelocityX, CombinedVelocityY);
        (VelocityX, VelocityY) = ApplyDeceleration(VelocityX, VelocityY);
        (_objBeneathVelocityX, _objBeneathVelocityY) = ApplyDeceleration(_objBeneathVelocityX, _objBeneathVelocityY);
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
    private (double VelocityX, double VelocityY) ApplyDeceleration(double velocityX, double velocityY)
    {
        ArgumentNullException.ThrowIfNull(Objects);
        if (velocityY > 0)
            velocityY = Math.Max(velocityY - DefaultDeceleration * Objects.DeltaTime, 0);
        else if (velocityY < 0)
            velocityY = Math.Min(velocityY + DefaultDeceleration * Objects.DeltaTime, 0);

        bool inAir = !IsFloorBeneath();
        
        if (velocityX > 0)
            velocityX = Math.Max(velocityX - (inAir ? DefaultDeceleration / 3 : DefaultDeceleration) * Objects.DeltaTime, 0);
        else if (velocityX < 0)
            velocityX = Math.Min(velocityX + (inAir ? DefaultDeceleration / 3 : DefaultDeceleration) * Objects.DeltaTime, 0);
        return (velocityX, velocityY);
    }
}