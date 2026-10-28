using System.Diagnostics;
using MCR_Engine.Core;

namespace MCR_Engine.Objects;

public abstract class GameObject : IComparable<GameObject>
{
    public const int MinLayer = 0, MaxLayer = 14;
    private const double DefaultGravity = 330, DefaultDeceleration = 90, MaxGravity = 150;
    protected ObjectManager? Objects;
    private readonly bool _collision, _gravity;
    private bool _visible;
    private int _layer;
    private double _velocityX, _velocityY;
    private double _x,  _y;
    private RGB?[,] _texture;
    protected GameObject(int x, int y, RGB?[,] texture, int layer, bool collision, bool gravity)
    {
        X  = x; Y = y;
        _texture = texture;
        Layer = layer;
        _collision = collision;
        _gravity = gravity;
        _velocityX = _velocityY = 0;
        _visible = true;
    }
    public int Width => _texture.GetLength(0);
    public int Height => _texture.GetLength(1);
    public bool Collision => _collision && Visible;
    public bool Move => Math.Abs(_velocityX) + Math.Abs(_velocityY) > 0;
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
        get => _layer; set => _layer = (value >= MinLayer && value <= MaxLayer ? value : throw new ArgumentException("Value must be equal or greater than 0."));
    }
    protected virtual bool Gravity => _gravity;
    public void UpdatePosition()
    {
        ArgumentNullException.ThrowIfNull(Objects);
        if (Gravity) 
            HandleGravity();
        if (!Move) 
            return;
        double[] velocityStepsX = GetVelocitySteps(_velocityX * Objects.DeltaTime);
        double[] velocityStepsY = GetVelocitySteps(_velocityY * Objects.DeltaTime);
        for (int i = 0; i < Math.Max(velocityStepsX.Length, velocityStepsY.Length); i++)
        {
            if (i < velocityStepsY.Length)
                TryMoveY(velocityStepsY[i]);
            if (i < velocityStepsX.Length)
                TryMoveX(velocityStepsX[i]);
        }
        ApplyDeceleration();
    }
    public void AddVelocity(Direction direction, double acceleration, double maxVelocity)
    {
        ArgumentNullException.ThrowIfNull(Objects);
        if (acceleration <= 0 || maxVelocity <= 0)
            throw new ArgumentException("Invalid acceleration or maxVelocity.");
        switch (direction)
        {
            case Direction.Up: _velocityY = Math.Max(_velocityY - acceleration * Objects.DeltaTime, -maxVelocity); break;
            case Direction.Down: _velocityY = Math.Min(_velocityY + acceleration * Objects.DeltaTime, maxVelocity); break;
            case Direction.Left: _velocityX = Math.Max(_velocityX - acceleration * Objects.DeltaTime, -maxVelocity); break;
            case Direction.Right: _velocityX = Math.Min(_velocityX + acceleration * Objects.DeltaTime, maxVelocity); break;
            default: throw new ArgumentException("Invalid direction.");
        }
    }
    public void SetNewVelocity(Direction direction, double velocity)
    {
        if (velocity < 0)
            throw new ArgumentException("Velocity must be greater than or equal to 0.");
        switch (direction)
        {
            case Direction.Up: _velocityY = -velocity; break;
            case Direction.Down: _velocityY = velocity; break;
            case Direction.Left: _velocityX = -velocity; break;
            case Direction.Right: _velocityX = velocity; break;
            default: throw new ArgumentException("Invalid direction.");
        }
    }
    public int GetDistanceToNextObject(Direction direction, bool onlyOnScene, bool onlyWithCollision,
        bool startAtTop, Type[]? searchFor = null)
    {
        ArgumentNullException.ThrowIfNull(Objects);
        return Objects.GetDistanceToNextObject(this, direction, onlyOnScene, onlyWithCollision, startAtTop, searchFor);
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
    public (double X, double Y) GetVelocity() => (_velocityX, _velocityY);
    public void SetObjectManager(ObjectManager objects) => Objects = objects ?? throw new InvalidOperationException("Sprite manager is not set.");
    public void ChangeVisibility(bool? value = null) => _visible = value ?? !_visible;
    public void StopMovingX() => _velocityX = 0;
    public void StopMovingY() => _velocityY = 0;
    public RGB? GetColor(int x, int y) => _texture[x, y];
    public void SetNewTexture(RGB?[,] texture) => _texture = texture;
    public bool CoordinatesOnObject(int x, int y) => (x >= X && y >= Y && x < X + Width && y < Y + Height);
    public abstract GameObject? Clone(ObjectManager objects);
    protected virtual bool IsFloorBeneath() => GetDistanceToNextObject(Direction.Down, false, true, false) is 0;
    private void TryMoveX(double xAdd)
    {
        double xOld = _x;
        _x += xAdd;
        if (!_collision || !IsBordersCollision(out _))
            return;
        StopMovingX();
        _x = xOld;
    }
    private void TryMoveY(double yAdd)
    {
        double yOld = _y;
        _y += yAdd;
        if ((!_collision && !Gravity) || !IsBordersCollision(out _))
            return;
        StopMovingY();
        _y = yOld;
    }
    private void HandleGravity()
    {
        if (!Gravity)
            throw new InvalidOperationException("Gravity is set to false or object has no collision.");
        if (IsFloorBeneath())
        {
            if (_velocityY > 0)
                StopMovingY();
            return;
        }
        AddVelocity(Direction.Down, DefaultGravity, MaxGravity);
    }
    private void ApplyDeceleration()
    {
        ArgumentNullException.ThrowIfNull(Objects);
        if (_velocityY > 0)
            _velocityY = Math.Max(_velocityY - DefaultDeceleration * Objects.DeltaTime, 0);
        else if (_velocityY < 0)
            _velocityY = Math.Min(_velocityY + DefaultDeceleration * Objects.DeltaTime, 0);

        bool inAir = !IsFloorBeneath();
        
        if (_velocityX > 0)
            _velocityX = Math.Max(_velocityX - (inAir ? DefaultDeceleration / 3 : DefaultDeceleration) * Objects.DeltaTime, 0);
        else if (_velocityX < 0)
            _velocityX = Math.Min(_velocityX + (inAir ? DefaultDeceleration / 3 : DefaultDeceleration) * Objects.DeltaTime, 0);
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
            return -1;
        int layerCompare = _layer.CompareTo(other._layer);
        if (layerCompare != 0)
            return layerCompare;
        return (Width * Height).CompareTo(other.Width * other.Height);
    }
}