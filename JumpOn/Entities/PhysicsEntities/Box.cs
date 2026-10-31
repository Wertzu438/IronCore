using MCR_Engine.Core;
using MCR_Engine.Core.BaseObjectTypes;
using MCR_Engine.Objects;

namespace JumpOn.Entities.PhysicsEntities;

public class Box : Entity
{
    private const string BoxUndamagedName = "woodBox", BoxDamagedName = "woodBoxDamaged";
    private static readonly Direction[] CheckDirections = [Direction.Left, Direction.Right];
    private readonly RGB?[,] _undamagedTexture;
    private readonly RGB?[,] _damagedTexture;
    private readonly int _xSpawn, _ySpawn;
    public Box(int x, int y, Clock clock, Textures textures) : base(x, y, 4, clock)
    {
        _xSpawn = x; _ySpawn = y;
        _undamagedTexture = textures.Get(BoxUndamagedName);
        _damagedTexture = textures.Get(BoxDamagedName);
        SetNewTexture(_undamagedTexture);
        
    }
    private Box(int x, int y, double maxHealth, Clock clock, RGB?[,] undamagedTexture, RGB?[,] damagedTexture) : base(x, y, maxHealth, clock)
    {
        _xSpawn = x; _ySpawn = y;
        _undamagedTexture = undamagedTexture;
        _damagedTexture = damagedTexture;
        SetNewTexture(_undamagedTexture);
    }
    private double DamagePerFall => MaxHealth / 2; 
    public override GameObject? Clone(ObjectManager objects) => new Box(_xSpawn, _ySpawn, MaxHealth, Clock, _undamagedTexture, _damagedTexture);
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
        if (GetFallDamage(out _))
        {
            Damage(DamagePerFall, true, false);
            SetNewTexture(_damagedTexture);
        }
        if (!Alive)
        {
            Objects.Remove(this);
        }
    }
}