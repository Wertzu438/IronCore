using MCR_Engine.Core;

namespace MCR_Engine.Objects;

public abstract class Sprite
{
    private const double DefaultGaravity = 0.4;
    private double _velocityX, _velocityY;
    private SpriteManager? _sprites;
    private readonly bool _collision;
    private readonly bool _gravity;
    private int _layer;
    private bool _visible;
    private double _x,  _y;
    private RGB?[,] _texture;
    public Sprite(int x, int y, RGB?[,] texture, int layer, bool collision, bool gravity)
    {
        X  = x; Y = y;
        _texture = texture;
        Layer = layer;
        Visible = true;
        _collision = collision;
        _gravity = gravity;
        _velocityX = _velocityY = 0;
    }
    public RGB?[,] Texture => (RGB?[,])_texture.Clone();
    public virtual bool Gravity => _gravity;
    public bool IsMoving => Math.Abs(_velocityX) + Math.Abs(_velocityY) > 0;
    public int Width => _texture.GetLength(0);
    public int Height => _texture.GetLength(1);
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
        get => _visible; set => _visible = value;
    }
    public int Layer
    {
        get => _layer; private set => _layer = (value >= 0 ? value : throw new ArgumentException("Value must be equal or greater than 0."));
    }
    public SpriteManager Sprites
    {
        protected get => _sprites ?? throw new InvalidOperationException("Sprite manager is not set."); set => _sprites = value;
    }
    public void UpdatePosition(double deltaTime)
    {
        double[] velocityStepsX = GetVelocitySteps(_velocityX * deltaTime);
        double[] velocityStepsY = GetVelocitySteps(_velocityY * deltaTime);
        for (int i = 0; i < Math.Max(velocityStepsX.Length, velocityStepsY.Length); i++)
        {
            double x = _x, y = _y;
            if (i < velocityStepsX.Length)
            {
                x += velocityStepsX[i];
            }
            if (i < velocityStepsY.Length && !(IsFloorBeneath() && _velocityY > 0))
            {
                y += velocityStepsY[i];
            }
            if (IsCollided(out _))
            {
                return;
            } 
            _x = x;
            _y = y;
        }
        ApplyDeceleration();
        if (Gravity)
            HandleGravity();
    }
    public RGB? GetColor(int x, int y) => _texture[x, y];
    public bool CoordinatesOnSprite(int x, int y) => (x >= X && y >= Y && x < X + Width && y < Y + Height);
    protected void SetNewTexture(RGB?[,] texture) => _texture = texture;
    protected void AddVelocity(Direction direction, double acceleration, double maxVelocity)
    {
        if (acceleration <= 0 || maxVelocity <= 0)
            throw new ArgumentException("Invalid acceleration or maxVelocity.");
        switch (direction)
        {
            case Direction.Up: _velocityY = Math.Max(_velocityY - acceleration, -maxVelocity); break;
            case Direction.Down: _velocityY = Math.Min(_velocityY + acceleration, maxVelocity); break;
            case Direction.Left: _velocityX = Math.Max(_velocityX - acceleration, -maxVelocity); break;
            case Direction.Right: _velocityX = Math.Min(_velocityX + acceleration, maxVelocity); break;
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
        AddVelocity(Direction.Down, 1, 40);
    }
    
    
    private void ApplyDeceleration()
    {
        if (_velocityX > 0)
            _velocityX = Math.Max(_velocityX - 0.35, 0);
        else if (_velocityX < 0)
            _velocityX = Math.Min(_velocityX + 0.35, 0);
        
        if (_velocityY > 0)
            _velocityY = Math.Max(_velocityY - 0.35, 0);
        else if (_velocityY < 0)
            _velocityY = Math.Min(_velocityY + 0.35, 0);
    }
    private double[] GetVelocitySteps(double velocity)
    {
        double[] velocitySteps = new double[(int)velocity + 1];
        double velocityPerStep = velocity / velocitySteps.Length;
        for (int i = 0; i < velocitySteps.Length; i++)
        {
            velocitySteps[i] = velocityPerStep;
        }
        return velocitySteps;
    }
}