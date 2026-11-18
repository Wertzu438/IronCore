using JumpOn.Entities.LivingEntities;
using MCR_Engine.Core;
using MCR_Engine.ObjectSystem;
using MCR_Engine.ObjectSystem.BaseObjectTypes;

namespace JumpOn.Entities.OtherEntities;

public class MovingPlatform : MovingObject
{
    private readonly int _xSpawn, _ySpawn;
    private readonly double _xVelocity, _yVelocity;
    private readonly int _distance;
    private readonly RGB?[,] _baseTexture;
    private readonly int? _amount;
    public MovingPlatform(int x, int y, double xVelocity, double yVelocity, int distance, Textures textures, int? amount = null) : 
        base(x, y, 1, false, Textures.DebuggingTexture(4, 4))
    {
        _xSpawn = x; _ySpawn = y;
        VelocityX = _xVelocity = xVelocity; VelocityY = _yVelocity = yVelocity;
        SetNewTexture(GetTexture(_baseTexture = textures.Get("metalWall"), Direction.Right, _amount = amount));
        _distance = distance;
    }
    private MovingPlatform(int x, int y, double xVelocity, double yVelocity, int distance, RGB?[,] baseTexture, int? amount = null) : 
        base(x, y, 1, false, Textures.DebuggingTexture(4, 4))
    {
        _xSpawn = x; _ySpawn = y;
        VelocityX = _xVelocity = xVelocity; VelocityY = _yVelocity = yVelocity;
        SetNewTexture(GetTexture(_baseTexture = baseTexture, Direction.Right, _amount = amount));
        _distance = distance; 
    }
    private int DistanceX => (int)(_distance * (Math.Abs(VelocityX) / (Math.Sqrt(Math.Pow(VelocityX, 2) + Math.Pow(VelocityY, 2)))));
    private int DistanceY => (int)(_distance * (Math.Abs(VelocityY) / (Math.Sqrt(Math.Pow(VelocityX, 2) + Math.Pow(VelocityY, 2)))));
    public override void TickUpdate()
    {
        base.TickUpdate();
        if (X > _xSpawn + DistanceX)
        {
            X = _xSpawn + DistanceX;
            VelocityX *= -1;
        }
        else if (X < _xSpawn - DistanceX)
        {
            X = _xSpawn - DistanceX;
            VelocityX *= -1;
        }

        if (Y > _ySpawn + DistanceY)
        {
            Y = _ySpawn + DistanceY;
            VelocityY *= -1;
        }
        else if (Y < _ySpawn - DistanceY)
        {
            Y = _ySpawn - DistanceY;
            VelocityY *= -1;
        }
        /*if (GetDistanceToNextObject(Direction.Up, true, [typeof(Entity)]) is 0)
        {
            Entity? obj = GetNearestObject(Direction.Up, [typeof(Entity)]) as Entity;
            if (obj is null)
                throw new ArgumentException("Object not found.");

            int entityY = obj.Y + obj.Height;

            if (entityY >= Y && entityY < Y + obj.Height / 2)
            {
                obj.Y = Y - obj.Height;
                obj.StopMovingY();
            }
        }*/
    }
    public override GameObject? Clone(Objects objects) => new MovingPlatform(_xSpawn, _ySpawn, _xVelocity, _yVelocity, _distance, _baseTexture, _amount);
    private static RGB?[,] GetTexture(RGB?[,] baseTexture, Direction? textureExpansion, int? amount)
    {
        if (!(textureExpansion.HasValue && amount.HasValue) || amount.Value <= 1)
            return baseTexture;
        return Textures.CreateExpansion(baseTexture, textureExpansion.Value, amount.Value);
    }
}