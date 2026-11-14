using MCR_Engine.Core;

namespace MCR_Engine.ObjectSystem.BaseObjectTypes;

public class MovingObject : GameObject
{
    private readonly RGB?[,] _texture;
    private MovingObject? _objectBeneath;
    public MovingObject(int x, int y, int layer, bool collision, bool gravity, RGB?[,] texture, bool visible = true) : base(x, y, texture, layer, collision, gravity)
    {
        _texture = texture;
        ChangeVisibility(visible);
        _objectBeneath = null;
    }
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
    public override (double X, double Y) GetVelocity(bool removeDeceleration = false)
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
    private MovingObject? ObjectBeneath => _objectBeneath;
    protected internal override void UpdatePosition()
    {
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
    public override GameObject? Clone(ObjectSystem.Objects objects) => new MovingObject(X, Y, Layer, Collision, Gravity, _texture, Visible);
}