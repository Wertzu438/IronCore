using System.Diagnostics;
using MCR_Engine.Core;

namespace MCR_Engine.Objects;

public abstract class Sprite : IComparable<Sprite>
{
    private const double DefaultGravity = 330, DefaultDeceleration = 90, MaxGravity = 150;
    private readonly bool _collision, _gravity;
    private bool _visible;
    private int _layer;
    private double _velocityX, _velocityY;
    private double _x,  _y;
    private RGB?[,] _texture;
    protected Sprite(int x, int y, RGB?[,] texture, int layer, bool collision, bool gravity)
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
    public double VelocityX => _velocityX;
    public double VelocityY => _velocityY;
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
        get => _layer; private set => _layer = (value >= 0 ? value : throw new ArgumentException("Value must be equal or greater than 0."));
    }
    public SpriteManager Sprites
    {
        protected get => field ?? throw new InvalidOperationException("Sprite manager is not set."); set => field = value;
    }
    protected virtual bool Gravity => _gravity;
    public void UpdatePosition()
    {
        if (Gravity) 
            HandleGravity();
        if (!Move) 
            return;
        double[] velocityStepsX = GetVelocitySteps(_velocityX * Sprites.DeltaTime);
        double[] velocityStepsY = GetVelocitySteps(_velocityY * Sprites.DeltaTime);
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
        if (acceleration <= 0 || maxVelocity <= 0)
            throw new ArgumentException("Invalid acceleration or maxVelocity.");
        switch (direction)
        {
            case Direction.Up: _velocityY = Math.Max(_velocityY - acceleration * Sprites.DeltaTime, -maxVelocity); break;
            case Direction.Down: _velocityY = Math.Min(_velocityY + acceleration * Sprites.DeltaTime, maxVelocity); break;
            case Direction.Left: _velocityX = Math.Max(_velocityX - acceleration * Sprites.DeltaTime, -maxVelocity); break;
            case Direction.Right: _velocityX = Math.Min(_velocityX + acceleration * Sprites.DeltaTime, maxVelocity); break;
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
    public bool OnScene() => Sprites.SpriteOnScene(this);
    public int GetDistanceToNextSprite(Direction direction, bool onlySpritesWithCollision, bool startAtTop, Type[]? searchFor = null) => Sprites.GetDistanceToNextSprite(this, direction, onlySpritesWithCollision, startAtTop, searchFor);
    public bool IsBordersCollision(out Sprite? collidedSprite, Type[]? searchFor = null) => Sprites.Collision(this, false, out collidedSprite, searchFor);
    public bool IsExactCollision(out Sprite? collidedSprite, Type[]? searchFor = null) => Sprites.Collision(this, true, out collidedSprite, searchFor);
    public void ChangeVisibility(bool? value = null) => _visible = value.HasValue ? value.Value : !_visible;
    public void StopMovingX() => _velocityX = 0;
    public void StopMovingY() => _velocityY = 0;
    public RGB? GetColor(int x, int y) => _texture[x, y];
    public void SetNewTexture(RGB?[,] texture) => _texture = texture;
    public bool CoordinatesOnSprite(int x, int y) => (x >= X && y >= Y && x < X + Width && y < Y + Height);
    public abstract Sprite? Clone(SpriteManager sprites);
    protected virtual bool IsFloorBeneath() => GetDistanceToNextSprite(Direction.Down, true, false) is 0 && OnScene();
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
            throw new InvalidOperationException("Gravity is set to or sprite has no collision false.");
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
        if (_velocityY > 0)
            _velocityY = Math.Max(_velocityY - DefaultDeceleration * Sprites.DeltaTime, 0);
        else if (_velocityY < 0)
            _velocityY = Math.Min(_velocityY + DefaultDeceleration * Sprites.DeltaTime, 0);

        bool inAir = !IsFloorBeneath();
        
        if (_velocityX > 0)
            _velocityX = Math.Max(_velocityX - (inAir ? DefaultDeceleration / 3 : DefaultDeceleration) * Sprites.DeltaTime, 0);
        else if (_velocityX < 0)
            _velocityX = Math.Min(_velocityX + (inAir ? DefaultDeceleration / 3 : DefaultDeceleration) * Sprites.DeltaTime, 0);
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
    public int CompareTo(Sprite? other)
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