using MCR_Engine.Core;

namespace MCR_Engine.ObjectSystem;

public abstract class GameObject : IComparable<GameObject>
{
    public const int MinLayer = 0, MaxLayer = 8;
    protected Objects? Objects;
    protected double CurrentX,  CurrentY;
    protected readonly bool HasCollision;
    private int _layer;
    private RGB?[,] _texture;
    internal GameObject(int x, int y, RGB?[,] texture, int layer, bool collision, bool gravity)
    {
        CurrentX  = x; CurrentY = y;
        _texture = texture;
        Layer = layer;
        HasCollision = collision;
        Gravity = gravity;
        Visible = true;
    }
    public int Width => _texture.GetLength(0);
    public int Height => _texture.GetLength(1);
    public bool IsCollidable => HasCollision && Visible;
    public int X
    {
        get => (int)Math.Round(CurrentX);
        set => CurrentX = value;
    }
    public int Y
    {
        get => (int)Math.Round(CurrentY);
        set => CurrentY = value;
    }
    public int Layer
    {
        get => _layer; 
        set => _layer = (value is >= MinLayer and < MaxLayer ? value : throw new ArgumentException($"Value must >= 0 and < {MaxLayer}. ({value})"));
    }
    public bool Visible { get; private set; }
    protected virtual bool Gravity { get; }
    public virtual void TickUpdate() { }
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
    public void SetObjectManager(Objects objects) => Objects = objects ?? throw new InvalidOperationException("Sprite manager is not set.");
    public void ChangeVisibility(bool? value = null) => Visible = value ?? !Visible;
    public RGB? GetColor(int x, int y) => _texture[x, y];
    public void SetNewTexture(RGB?[,] texture) => _texture = texture;
    public bool CoordinatesOnObject(int x, int y) => (x >= X && y >= Y && x < X + Width && y < Y + Height);
    public abstract GameObject? Clone(Objects objects);
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