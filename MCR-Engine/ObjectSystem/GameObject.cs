using MCR_Engine.Core;

namespace MCR_Engine.ObjectSystem;

public abstract class GameObject : IComparable<GameObject>
{
    public const int MinLayer = 0, MaxLayer = 8;
    private const double DefaultGravity = 330, DefaultDeceleration = 90, MaxGravity = 150;
    protected Objects? Objects;
    protected double VelocityX, VelocityY;
    private readonly bool _collision, _gravity;
    private bool _visible;
    private int _layer;
    private double _x,  _y;
    private RGB?[,] _texture;
    internal GameObject(int x, int y, RGB?[,] texture, int layer, bool collision, bool gravity)
    {
        X  = x; Y = y;
        _texture = texture;
        Layer = layer;
        _collision = collision;
        _gravity = gravity;
        VelocityX = VelocityY = 0;
        _visible = true;
    }
    public int Width => _texture.GetLength(0);
    public int Height => _texture.GetLength(1);
    public bool Collision => _collision && Visible;
    public bool Move => Math.Abs(VelocityX) + Math.Abs(VelocityY) > 0;
    public bool Visible => _visible;
    public int X
    {
        get => (int)Math.Round(_x); set => _x = value; 
    }
    public int Y
    {
        get => (int)Math.Round(_y); set => _y = value; 
    }
    public int Layer
    {
        get => _layer; 
        set => _layer = (value >= MinLayer && value < MaxLayer ? value : throw new ArgumentException($"Value must be equal or greater than 0 and smaller than {MaxLayer}. ({value})"));
    }
    protected virtual bool Gravity => _gravity;
    protected virtual bool StopOnCollisionX => true;
    protected virtual bool StopOnCollisionY => true;
    protected internal virtual void UpdatePosition()
    {
        if (Gravity) 
            HandleGravity();
        if (!Move) 
            return;
        MoveAndCheckForCollision(VelocityX, VelocityY);
        ApplyDeceleration();
    }
    protected void MoveAndCheckForCollision(double velocityX, double velocityY)
    {
        ArgumentNullException.ThrowIfNull(Objects);
        double[] velocityStepsX = GetVelocitySteps(velocityX * Objects.DeltaTime);
        double[] velocityStepsY = GetVelocitySteps(velocityY * Objects.DeltaTime);
        for (int i = 0; i < Math.Max(velocityStepsX.Length, velocityStepsY.Length); i++)
        {
            if (i < velocityStepsY.Length)
            {
                if (!TryMoveY(velocityStepsY[i]) && StopOnCollisionY)
                    StopMovingY();
            }
            if (i < velocityStepsX.Length)
            {
                if (!TryMoveX(velocityStepsX[i]) && StopOnCollisionX)
                    StopMovingX();
            }
        }
    }
    public void AddVelocity(Direction direction, double acceleration, double maxVelocity)
    {
        ArgumentNullException.ThrowIfNull(Objects);
        if (acceleration <= 0 || maxVelocity <= 0)
            throw new ArgumentException("Invalid acceleration or maxVelocity.");
        switch (direction)
        {
            case Direction.Up: VelocityY = Math.Max(VelocityY - acceleration * Objects.DeltaTime, -maxVelocity); break;
            case Direction.Down: VelocityY = Math.Min(VelocityY + acceleration * Objects.DeltaTime, maxVelocity); break;
            case Direction.Left: VelocityX = Math.Max(VelocityX - acceleration * Objects.DeltaTime, -maxVelocity); break;
            case Direction.Right: VelocityX = Math.Min(VelocityX + acceleration * Objects.DeltaTime, maxVelocity); break;
            default: throw new ArgumentException("Invalid direction.");
        }
    }
    public void SetNewVelocity(Direction direction, double velocity)
    {
        if (velocity < 0)
            throw new ArgumentException("Velocity must be greater than or equal to 0.");
        switch (direction)
        {
            case Direction.Up: VelocityY = -velocity; break;
            case Direction.Down: VelocityY = velocity; break;
            case Direction.Left: VelocityX = -velocity; break;
            case Direction.Right: VelocityX = velocity; break;
            default: throw new ArgumentException("Invalid direction.");
        }
    }
    public int GetDistanceToNextObject(Direction direction, bool onlyWithCollision, Type[]? searchFor = null)
    {
        ArgumentNullException.ThrowIfNull(Objects);
        return Objects.GetDistanceToNextObject(this, direction, onlyWithCollision, searchFor);
    }
    public GameObject? GetNearestObject(Direction direction, Type[] searchFor)
    {
        ArgumentNullException.ThrowIfNull(Objects);
        return Objects.GetNearestObject(this, direction, searchFor);
    }
    public bool IsBordersCollision(out GameObject? collidedGameObject, Type[]? searchFor = null)
    {
        ArgumentNullException.ThrowIfNull(Objects);
        return Objects.Collision(this, false, out collidedGameObject, searchFor);
    }
    public bool IsBordersCollision(GameObject otherGameObject)
    {
        ArgumentNullException.ThrowIfNull(Objects);
        return Objects.Collision(this, otherGameObject, false);
    }
    public bool IsExactCollision(out GameObject? collidedGameObject, Type[]? searchFor = null)
    {
        ArgumentNullException.ThrowIfNull(Objects);
        return Objects.Collision(this, true, out collidedGameObject, searchFor);
    }
    public bool IsExactCollision(GameObject otherGameObject)
    {
        ArgumentNullException.ThrowIfNull(Objects);
        return Objects.Collision(this, otherGameObject, true);
    }
    public bool OnScene(Direction? searchingDirection = null)
    {
        ArgumentNullException.ThrowIfNull(Objects);
        return Objects.ObjectOnScene(this, searchingDirection);
    }
    public (double X, double Y) GetVelocity(bool removeDeceleration = false)
    {
        if (!removeDeceleration)
            return (VelocityX, VelocityY);
        ArgumentNullException.ThrowIfNull(Objects);
        double vX = 0, vY = 0;
        if (VelocityX > 0)
            vX = VelocityX + DefaultDeceleration * Objects.DeltaTime;
        else if (VelocityX < 0)
            vX = VelocityX - DefaultDeceleration * Objects.DeltaTime;
        
        if (VelocityY > 0)
            vY = VelocityY + DefaultDeceleration * Objects.DeltaTime;
        else if (VelocityY < 0)
            vY = VelocityY - DefaultDeceleration * Objects.DeltaTime;
        return (vX, vY);
    }
    public void SetObjectManager(Objects objects) => Objects = objects ?? throw new InvalidOperationException("Sprite manager is not set.");
    public void ChangeVisibility(bool? value = null) => _visible = value ?? !_visible;
    public void StopMovingX() => VelocityX = 0;
    public void StopMovingY() => VelocityY = 0;
    public RGB? GetColor(int x, int y) => _texture[x, y];
    public void SetNewTexture(RGB?[,] texture) => _texture = texture;
    public bool CoordinatesOnObject(int x, int y) => (x >= X && y >= Y && x < X + Width && y < Y + Height);
    public abstract GameObject? Clone(Objects objects);
    protected virtual bool IsFloorBeneath() => _gravity && GetDistanceToNextObject(Direction.Down, true) is 0;
    protected void HandleGravity()
    {
        if (!Gravity)
            throw new InvalidOperationException("Gravity is set to false or object has no collision.");
        if (IsFloorBeneath())
        {
            if (VelocityY > 0)
                StopMovingY();
            return;
        }
        AddVelocity(Direction.Down, DefaultGravity, MaxGravity);
    }
    protected void ApplyDeceleration()
    {
        ArgumentNullException.ThrowIfNull(Objects);
        if (VelocityY > 0)
            VelocityY = Math.Max(VelocityY - DefaultDeceleration * Objects.DeltaTime, 0);
        else if (VelocityY < 0)
            VelocityY = Math.Min(VelocityY + DefaultDeceleration * Objects.DeltaTime, 0);

        bool inAir = !IsFloorBeneath();
        
        if (VelocityX > 0)
            VelocityX = Math.Max(VelocityX - (inAir ? DefaultDeceleration / 3 : DefaultDeceleration) * Objects.DeltaTime, 0);
        else if (VelocityX < 0)
            VelocityX = Math.Min(VelocityX + (inAir ? DefaultDeceleration / 3 : DefaultDeceleration) * Objects.DeltaTime, 0);
    }
    private bool TryMoveX(double xAdd)
    {
        double xOld = _x;
        _x += xAdd;
        if (!_collision || !IsBordersCollision(out _))
            return true;
        _x = xOld;
        return false;
    }
    private bool TryMoveY(double yAdd)
    {
        double yOld = _y;
        _y += yAdd;
        if (!IsBordersCollision(out _))
            return true;
        _y = yOld;
        return false;
    }
    private static double[] GetVelocitySteps(double velocity)
    {
        double[] velocitySteps = new double[Math.Abs((int)velocity) + 1];
        double velocityPerStep = velocity / velocitySteps.Length;
        for (int i = 0; i < velocitySteps.Length; i++)
        {
            velocitySteps[i] = velocityPerStep;
        }
        return velocitySteps;
    }
    protected async Task Flash(int amount, int msBetweenChange = 20)
    {
        if (amount % 2 != 0 || !Visible)
            throw new ArgumentException("Amount must be odd and sprite must be visible.");
        for (int i = 0; i < amount; i++)
        {
            ChangeVisibility();
            await Task.Delay(msBetweenChange);
        }
    }
    public int CompareTo(GameObject? other)
    {
        if (ReferenceEquals(this, other)) 
            return 0;
        if (other is null)
            return 1;
        int layerCompare = _layer.CompareTo(other._layer);
        if (layerCompare != 0)
            return layerCompare;
        return (Width * Height).CompareTo(other.Width * other.Height);
    }
}