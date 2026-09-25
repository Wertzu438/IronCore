using MCR_Engine.Objects;
using MCR_Engine.Rendering.Output;
using MCR_Engine.Core;

namespace MCR_Engine.Core;

public class Scene
{
    private readonly int _width, _height;
    private readonly Pixel[,] _pixels;
    private readonly RGB[,] _backgroundTexture;
    private readonly SpriteManager _sprites;
    private readonly Camera _camera;
    public Scene(Camera camera, int width, int height, RGB[,] backgroundTexture)
    {
        if (height % 2 != 0)
            throw new ArgumentException("Hight must be dividable by 2.");
        _camera = camera;
        this._width = width;
        this._height = height;
        this._backgroundTexture = backgroundTexture;
        _sprites = new SpriteManager(this);
        _pixels = Create();
    }
    public SpriteManager Sprites => _sprites;
    public int Width => _width;
    public int Height => _height;
    private int BackgroundWidth => _backgroundTexture.GetLength(0);
    private int BackgroundHeight => _backgroundTexture.GetLength(1);
    public void Update(double deltaTime, bool renderAll = false)
    {
        Sprites.Update(deltaTime);
        _camera.RecalculateOffset();
        (int xStart, int xEnd, int yStart, int yEnd) = GetVisibleArea(renderAll);
        int backgroundWidth = BackgroundWidth, backgroundHeight = BackgroundHeight;
        for (int y = yStart; y < yEnd; y++)
        {
            for (int x = xStart; x < xEnd; x++)
            {
                RGB? rgb = null;
                Sprite? sprite = _sprites.GetSpriteMapPixel(x, y);
                if (sprite is not null && sprite.Visible)
                {
                    rgb = sprite.GetColor(x - sprite.X, y - sprite.Y);
                }
                rgb ??= _backgroundTexture[x % backgroundWidth, y % backgroundHeight];
                _pixels[x, y].SetNewRGB((RGB)rgb);
            }
        }
    }
    internal Pixel GetPixel(int x, int y)
    {
        if (!ValidPixelCoordinates(x, y))
            throw new InvalidDataException("X or Y invalide");
        return _pixels[x, y];
    }
    public bool ValidPixelCoordinates(int x, int y) => x >= 0 && y >= 0 && x < _width && y < _height;
    private Pixel[,] Create()
    {
        Pixel[,] newMap = new Pixel[_width, _height];
        for (int y = 0; y < _height; y++)
            for (int x = 0; x < _width; x++)
                newMap[x, y] = new Pixel(_backgroundTexture[x % BackgroundWidth, y % BackgroundHeight]);
        return newMap;
    }
    private (int xStart, int xEnd, int yStart, int yEnd) GetVisibleArea(bool renderAll)
    {
        int xStart, xEnd, yStart, yEnd;
        if (renderAll)
        {
            xStart = 0; yStart = 0;
            xEnd = Width; yEnd = Height;
        }
        else
        {
            xStart = _camera.XOffset; xEnd = _camera.GetRenderBorderX(xStart);
            yStart = _camera.YOffset * 2; yEnd = _camera.GetRenderBorderY(yStart) * 2;
        }
        return (xStart, xEnd, yStart, yEnd);
    }
}