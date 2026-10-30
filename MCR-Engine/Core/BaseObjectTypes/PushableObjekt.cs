using MCR_Engine.Objects;

namespace MCR_Engine.Core.BaseObjectTypes;

public class PushableObjekt : MovingObject
{
    private static readonly Direction[] CheckDirections = [Direction.Left, Direction.Right];
    private readonly RGB?[,] _baseTexture;
    public PushableObjekt(int x, int y, RGB?[,] texture, int layer) : base(x, y, layer, true, texture)
    {
        _baseTexture = texture;
    }
    protected internal override void UpdatePosition()
    {
        double objectVelocityX = 0d;
        foreach (Direction direction in CheckDirections)
        {
            int distance = GetDistanceToNextObject(direction, true, true, [typeof(MovingObject)]);
            if (distance > 0)
                continue;
            GameObject? gameObject = GetNearestObject(direction, [typeof(MovingObject)]);
            if (gameObject == null || Math.Abs(gameObject.GetVelocity().X) < Math.Abs(objectVelocityX))
                continue;
            objectVelocityX = gameObject.GetVelocity().X;
            if ((direction is Direction.Left && objectVelocityX < 0) || (direction is Direction.Right && objectVelocityX > 0))
                objectVelocityX = 0;
                
        }
        if (Math.Abs(objectVelocityX) > Math.Abs(VelocityX))
            VelocityX = objectVelocityX * 0.80;
        base.UpdatePosition();
    }
    public override GameObject? Clone(ObjectManager objects) => new PushableObjekt(X, Y, _baseTexture, Layer);
}