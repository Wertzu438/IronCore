using MCR_Engine.Core;
using MCR_Engine.Core.BaseObjectTypes;
using MCR_Engine.Objects;

namespace JumpOn.Entities.PhysicsEntities;

public class Box : Entity
{
    private const string BoxUndamagedName = "woodBox", BoxDamagedName = "woodBoxDamaged";
    private const double DefaultMaxHealth = 4.0;
    private static readonly Direction[] CheckDirections = [Direction.Left, Direction.Right];
    private readonly RGB?[,] _undamagedTexture;
    private readonly RGB?[,] _damagedTexture;
    private readonly int _xSpawn, _ySpawn;
    public Box(int x, int y, Clock clock, Textures textures) : base(x, y, true, DefaultMaxHealth, clock)
    {
        _xSpawn = x; _ySpawn = y;
        _undamagedTexture = textures.Get(BoxUndamagedName);
        _damagedTexture = textures.Get(BoxDamagedName);
        SetNewTexture(_undamagedTexture);
        
    }
    private Box(int x, int y, double maxHealth, Clock clock, RGB?[,] undamagedTexture, RGB?[,] damagedTexture) : base(x, y, true, maxHealth, clock)
    {
        _xSpawn = x; _ySpawn = y;
        _undamagedTexture = undamagedTexture;
        _damagedTexture = damagedTexture;
        SetNewTexture(_undamagedTexture);
    }
    protected override double CombinedVelocityX
    {
        get
        {
            double a = base.CombinedVelocityX;
            int objBeneath = MovingObjectsBeneath;
            if (objBeneath != 1 || a == 0)
                return a;
            return a + (a > 0 ? 90 * Objects.DeltaTime : 90 * Objects.DeltaTime * -1);
        }
    }
    protected override bool StopOnCollisionX => false;
    protected override bool StopOnCollisionY => false;
    protected override double CombinedVelocityY => base.CombinedVelocityY + (VelocityY == 0 ? 0 : 90 * Objects.DeltaTime);
    private static double DamagePerFall => DefaultMaxHealth / 2; 
    public override GameObject? Clone(ObjectManager objects) => new Box(_xSpawn, _ySpawn, MaxHealth, Clock, _undamagedTexture, _damagedTexture);
    protected override void UpdatePosition()
    {
        bool isBox = false;
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
            isBox = gameObject.GetType() == typeof(Box);
            if ((direction is Direction.Left && objectVelocityX < 0) || (direction is Direction.Right && objectVelocityX > 0))
                objectVelocityX = 0;
        }
        if (Math.Abs(objectVelocityX) > Math.Abs(VelocityX))
            VelocityX = objectVelocityX * (!isBox ? 0.6 : 1);
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