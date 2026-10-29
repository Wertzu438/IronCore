using MCR_Engine.Objects;

namespace MCR_Engine.Core.BaseObjectTypes;

public class MovingObject : GameObject
{
    private readonly RGB?[,] _baseTexture;
    private readonly Direction? _textureExpansion;
    private readonly int? _amount;
    public MovingObject(int x, int y, int layer, bool gravity, RGB?[,] baseTexture, Direction? textureExpansion = null, int? amount = null, bool visible = true) : base(x, y, GetTexture(baseTexture, textureExpansion, amount), layer, true, gravity)
    {
        _baseTexture = baseTexture;
        _textureExpansion = textureExpansion;
        _amount = amount;
        ChangeVisibility(visible);
    }
    protected internal override void UpdatePosition()
    {
        ArgumentNullException.ThrowIfNull(Objects);
        (double X, double Y) objVelocity = (0, 0);
        if (GetDistanceToNextObject(Direction.Down, true, true, false, [typeof(MovingObject)]) < 4)
        {
            GameObject? obj = Objects.GetNearestObject(this, Direction.Down, [typeof(MovingObject)]);
            if (obj is not null)
            {
                objVelocity = obj.GetVelocity();
            }
        }
        if (Gravity) 
            HandleGravity();
        if (!Move && objVelocity.X == 0 && objVelocity.Y == 0)
            return;
        double[] velocityStepsX = GetVelocitySteps((VelocityX + objVelocity.X) * Objects.DeltaTime);
        double[] velocityStepsY = GetVelocitySteps((VelocityY + objVelocity.Y) * Objects.DeltaTime);
        for (int i = 0; i < Math.Max(velocityStepsX.Length, velocityStepsY.Length); i++)
        {
            if (i < velocityStepsY.Length)
                TryMoveY(velocityStepsY[i]);
            if (i < velocityStepsX.Length)
                TryMoveX(velocityStepsX[i]);
        }
        ApplyDeceleration();
    }
    public override GameObject? Clone(ObjectManager objects) => new SolidObject(X, Y, Layer, _baseTexture, _textureExpansion, _amount, Visible);
    private static RGB?[,] GetTexture(RGB?[,] baseTexture, Direction? textureExpansion, int? amount)
    {
        if (!(textureExpansion.HasValue && amount.HasValue) || amount.Value <= 1)
            return baseTexture;
        return Textures.CreateExpansion(baseTexture, textureExpansion.Value, amount.Value);
    }
}