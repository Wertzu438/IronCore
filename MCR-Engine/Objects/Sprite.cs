using MCR_Engine.Core;

namespace MCR_Engine.Objects;

public abstract class Sprite
{
    private const double DefaultGravity = 370, DefaultDeceleration = 60;
    private double _velocityX, _velocityY;
    private readonly bool _collision;
    private double _x,  _y;
    private RGB?[,] _texture;
    protected Sprite(int x, int y, RGB?[,] texture, int layer, bool collision, bool gravity)
    {
        X  = x; Y = y;
        _texture = texture;
        Layer = layer;
        _collision = collision;
        _velocityX = _velocityY = 0;
        Visible = true;
        Gravity = gravity;
    }
    public RGB?[,] Texture => (RGB?[,])_texture.Clone();
    public int Width => _texture.GetLength(0);
    public int Height => _texture.GetLength(1);
    public bool Collision => _collision && Visible;
    public int X
    {
        get => (int)_x; protected set => _x = value; 
    }
    public int Y
    {
        get => (int)_y; protected set => _y = value; 
    }
    public bool Visible
    {
        get => field; set => field = value;
    }
    public int Layer
    {
        get => field; private set => field = (value >= 0 ? value : throw new ArgumentException("Value must be equal or greater than 0."));
    }
    public SpriteManager Sprites
    {
        protected get => field ?? throw new InvalidOperationException("Sprite manager is not set."); set => field = value;
    }
    protected bool Move => Math.Abs(_velocityX) + Math.Abs(_velocityY) > 0;
    protected virtual bool Gravity => field;
    public void UpdatePosition()
    {
        if (!Visible)
            return;
        if (Gravity) 
            HandleGravity();
        if (!Move) return;
        double[] velocityStepsX = GetVelocitySteps(_velocityX * Sprites.DeltaTime);
        double[] velocityStepsY = GetVelocitySteps(_velocityY * Sprites.DeltaTime);
        for (int i = 0; i < Math.Max(velocityStepsX.Length, velocityStepsY.Length); i++)
        {
            double xOld = _x, yOld = _y;
            if (i < velocityStepsX.Length)
                _x += velocityStepsX[i];
            if (i < velocityStepsY.Length)
                _y += velocityStepsY[i];
            if (!IsCollided(out _))
                continue;
            _y = yOld;
            if (!IsCollided(out _))
                continue;
            _x = xOld; 
            break;
        }
        ApplyDeceleration();
    }

    public void StopMovingX() => _velocityX = 0;
    public void StopMovingY() => _velocityY = 0;
    public RGB? GetColor(int x, int y) => _texture[x, y];
    public bool CoordinatesOnSprite(int x, int y) => (x >= X && y >= Y && x < X + Width && y < Y + Height);
    protected void AddVelocity(Direction direction, double acceleration, double maxVelocity)
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
    protected void SetNewVelocity(Direction direction, double velocity)
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
    protected void SetNewTexture(RGB?[,] texture) => _texture = texture;
    protected int GetDistanceToNextSprite(Direction direction, bool onlySpritesWithCollision, bool startAtTop, Type[]? searchFor = null) => 
        Sprites.GetDistanceToNextSprite(this, direction, onlySpritesWithCollision, startAtTop, searchFor);
    protected bool IsCollided(out Sprite? collidedSprite, Type[]? searchFor = null) => 
        Sprites.Collision(this, out collidedSprite, searchFor);
    protected bool IsFloorBeneath() => GetDistanceToNextSprite(Direction.Down, true, false) is 0;
    private void HandleGravity()
    {
        if (!Gravity)
            throw new InvalidOperationException("Gravity is set to false.");
        if (IsFloorBeneath())
            return;
        AddVelocity(Direction.Down, DefaultGravity, 150);
    }
    private void ApplyDeceleration()
    {
        if (_velocityX > 0)
            _velocityX = Math.Max(_velocityX - DefaultDeceleration * Sprites.DeltaTime, 0);
        else if (_velocityX < 0)
            _velocityX = Math.Min(_velocityX + DefaultDeceleration * Sprites.DeltaTime, 0);
        
        if (_velocityY > 0)
            _velocityY = Math.Max(_velocityY - DefaultDeceleration * Sprites.DeltaTime, 0);
        else if (_velocityY < 0)
            _velocityY = Math.Min(_velocityY + DefaultDeceleration * Sprites.DeltaTime, 0);
    }
    private double[] GetVelocitySteps(double velocity)
    {
        double[] velocitySteps = new double[Math.Abs((int)velocity) + 1];
        double velocityPerStep = velocity / velocitySteps.Length;
        for (int i = 0; i < velocitySteps.Length; i++)
        {
            velocitySteps[i] = velocityPerStep;
        }
        return velocitySteps;
    }
}