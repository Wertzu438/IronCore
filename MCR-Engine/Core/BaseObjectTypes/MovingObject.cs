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
        GameObject? gameObject = null;
        if (GetDistanceToNextObject(Direction.Down, true, true, [typeof(MovingObject)]) < 1)
            gameObject = GetNearestObject(Direction.Down, [typeof(MovingObject)]);
        if (Gravity) 
            HandleGravity();
        if (!Move && (gameObject is null || !gameObject.Move))
            return;
        double gX = (gameObject is null ? 0d : gameObject.GetVelocity(true).X);
        double gY = (gameObject is null ? 0d : gameObject.GetVelocity(true).Y);
        if (Math.Abs(gX) > Math.Abs(VelocityX))
            VelocityX = gX;
        if (Math.Abs(gY) > Math.Abs(VelocityY))
            VelocityY = gY;
        MoveAndCheckForCollision(VelocityX, VelocityY, false);
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