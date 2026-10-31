using MCR_Engine.Core;
using MCR_Engine.Core.BaseObjectTypes;
using MCR_Engine.Objects;

namespace JumpOn.Entities.PhysicsEntities;

public class Box : Entity
{
    private const string TextureName = "woodBox";
    private static readonly Direction[] CheckDirections = [Direction.Left, Direction.Right];
    private readonly RGB?[,] _texture;
    public Box(int x, int y, int maxHealth, Clock clock, Textures textures) : base(x, y, maxHealth, clock)
    {
        SetNewTexture(_texture = textures.Get(TextureName));
    }
    private Box(int x, int y, double currentHealth, double maxHealth, Clock clock, RGB?[,] texture) : base(x, y, maxHealth, clock)
    {
        _texture = texture;
        if (currentHealth > maxHealth / 2)
            Health = currentHealth;
        SetNewTexture(_texture);
    }
    public override GameObject? Clone(ObjectManager objects) => new Box(X, Y, Health, MaxHealth, Clock, _texture);
    protected override void UpdatePosition()
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
    public override void Update()
    {
        return;
    }
}