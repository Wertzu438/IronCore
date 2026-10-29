using MCR_Engine.Objects;

namespace MCR_Engine.Core.BaseObjectTypes;

public class PushableObjekt : GameObject
{
    private static Direction[] checkDirections = [Direction.Left, Direction.Right];
    private readonly RGB?[,] _baseTexture;
    public PushableObjekt(int x, int y, RGB?[,] texture, int layer) : base(x, y, texture, layer, true, true)
    {
        _baseTexture = texture;
    }
    protected internal override void UpdatePosition()
    {
        (double X, double Y) velocityObj = (0, 0);
        foreach (Direction direction in checkDirections)
        {
            int distance = GetDistanceToNextObject(direction, true, true, false, [typeof(MovingObject)]);
            if (distance > 1)
                continue;
            GameObject? obj = Objects.GetNearestObject(this, direction, [typeof(MovingObject)]);
            if (obj == null)
                continue;
            velocityObj = obj.GetVelocity();
            break;
        }
        if (Gravity) 
            HandleGravity();
        VelocityX = velocityObj.X * 1;
        double[] velocityStepsX = GetVelocitySteps((VelocityX) * Objects.DeltaTime);
        double[] velocityStepsY = GetVelocitySteps((VelocityY) * Objects.DeltaTime);
        for (int i = 0; i < Math.Max(velocityStepsX.Length, velocityStepsY.Length); i++)
        {
            if (i < velocityStepsY.Length)
                TryMoveY(velocityStepsY[i]);
            if (i < velocityStepsX.Length)
                TryMoveX(velocityStepsX[i]);
        }
        ApplyDeceleration();
    }
    public override GameObject? Clone(ObjectManager objects) => new PushableObjekt(X, Y, _baseTexture, Layer);
}