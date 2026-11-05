using JumpOn.Entities.LivingEntities;
using MCR_Engine.Core;
using MCR_Engine.Objects;

namespace JumpOn.Entities.OtherEntities;

public class MovingPlatform : Entity
{
    private readonly int _xSpawn, _ySpawn;
    private readonly double _xVelocity, _yVelocity;
    private readonly int _distance;
    private readonly RGB?[,] _baseTexture;
    private readonly int? _amount;
    public MovingPlatform(int x, int y, double xVelocity, double yVelocity, int distance, Clock clock, Textures textures, int? amount = null) : 
        base(x, y, false, double.MaxValue, clock)
    {
        _xSpawn = x; _ySpawn = y;
        VelocityX = _xVelocity = xVelocity; VelocityY = _yVelocity = yVelocity;
        _distance = distance;
        SetNewTexture(GetTexture(_baseTexture = textures.Get("metalWall"), Direction.Right, _amount = amount));
    }
    private MovingPlatform(int x, int y, double xVelocity, double yVelocity, int distance, Clock clock, RGB?[,] baseTexture, int? amount = null) : 
        base(x, y, false, double.MaxValue, clock)
    {
        _xSpawn = x; _ySpawn = y;
        VelocityX = _xVelocity = xVelocity; VelocityY = _yVelocity = yVelocity;
        _distance = distance; 
        SetNewTexture(GetTexture(_baseTexture = baseTexture, Direction.Right, _amount = amount));
    }
    public override void Update()
    {
        double nextX = X + VelocityX;
        double nextY = Y + VelocityY;

        double dx = nextX - _xSpawn;
        double dy = nextY - _ySpawn;

        if (dx * dx + dy * dy > _distance * _distance)
        {
            VelocityX *= -1;
            VelocityY *= -1;
        }
        if (GetDistanceToNextObject(Direction.Up, true, [typeof(Entity)]) is 0)
        {
            GameObject? obj = GetNearestObject(Direction.Up, [typeof(Entity)]) as Entity;
            if (obj is null)
                throw new ArgumentException("Object not found.");
            int entityY = obj.Y + obj.Height;
            if (entityY >= Y && entityY < Y + obj.Height / 2)
            {
                obj.Y = Y - obj.Height;
                obj.StopMovingY();
            }
        }
    }
    public override GameObject? Clone(Objects objects) => new MovingPlatform(_xSpawn, _ySpawn, _xVelocity, _yVelocity, _distance, Clock, _baseTexture, _amount);
    protected override void UpdatePosition() => MoveAndCheckForCollision(VelocityX, VelocityY);
    private static RGB?[,] GetTexture(RGB?[,] baseTexture, Direction? textureExpansion, int? amount)
    {
        if (!(textureExpansion.HasValue && amount.HasValue) || amount.Value <= 1)
            return baseTexture;
        return Textures.CreateExpansion(baseTexture, textureExpansion.Value, amount.Value);
    }
}