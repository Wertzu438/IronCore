using MCR_Engine.Core;

namespace MCR_Engine.Rendering.UI;

public abstract class UIElement
{
    protected UIPixel[,] Buffer;
    private readonly bool _dontFlipX, _dontFlipY;
    private readonly bool _staticElement;
    private UIManager? _ui;
    private int _x, _y;
    protected UIElement(int width, int height, int x, int y, bool staticElement, bool visible = true, bool dontFlipX = true, bool dontFlipY = true)
    {
        if (width < 0 || height < 0)
            throw new ArgumentException("Width and height must be positive");
        _staticElement = staticElement;
        Visible = visible;
        X = x; Y = y;
        _dontFlipX = dontFlipX; _dontFlipY = dontFlipY;
        Buffer = CreateBuffer(width, height);
    }
    public int X
    {
        get => _staticElement ? (_dontFlipX ? _x : UI.UIMapWidth - _x - Width) : _x - UI.LastXOffset;
        set => _x = (value < 0 ? 0 : value);
    }
    public int Y
    {
        get => _staticElement ? (_dontFlipY ? _y : UI.UIMapHeight - _y - Height) : _y - UI.LastYOffset / 2;
        set => _y = (value < 0 ? 0 : value);
    }
    public int Width => Buffer.GetLength(0);
    public int Height => Buffer.GetLength(1);
    public bool Visible { get; set; }
    protected UIManager UI => _ui ?? throw new NullReferenceException("UI element has not been initialized");
    public UIPixel? GetUIPixel(int x, int y)
    {
        if (!OnDesign(x, y))
            throw new ArgumentException("Coordinates are out of bounds");
        return Buffer[x, y];
    }
    public void SetUIManager(UIManager ui) => _ui = ui;
    internal abstract void Update();
    protected virtual UIPixel[,] CreateBuffer(int width, int height) => new UIPixel[width, height];
    private bool OnDesign(int x, int y) => x >= 0 && x < Width && y >= 0 && y < Height;
}