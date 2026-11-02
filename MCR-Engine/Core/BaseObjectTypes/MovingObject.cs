using MCR_Engine.Objects;

namespace MCR_Engine.Core.BaseObjectTypes;

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
    private MovingObject ObjectBeneath => _objectBeneath;
    protected internal override void UpdatePosition()
    {
        if (IsFloorBeneath())
        {
            _objectBeneath = GetNearestObject(Direction.Down, [typeof(MovingObject)]) as MovingObject;
        }
        else _objectBeneath = null;
        if (Gravity) 
            HandleGravity();
        MoveAndCheckForCollision(CombinedVelocityX, CombinedVelocityY);
        ApplyDeceleration();
    }
    public override GameObject? Clone(ObjectManager objects) => new MovingObject(X, Y, Layer, Collision, Gravity, _texture, Visible);
    protected override bool IsFloorBeneath() => base.IsFloorBeneath() || GetDistanceToNextObject(Direction.Down, true, false, [typeof(MovingObject)]) == 0;
}