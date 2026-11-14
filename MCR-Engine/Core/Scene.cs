using MCR_Engine.ObjectSystem;
using MCR_Engine.Rendering.Output;

namespace MCR_Engine.Core;

public enum PathType
{
    Walk, Ladder, Drop
}
public class Scene
{
    private readonly Pixel[,] _pixels;
    private readonly RGB[,] _backgroundTexture;
    private readonly Camera _camera;
    private Objects _objects;
    public Scene(Camera camera, int width, int height, RGB[,] backgroundTexture)
    {
        if (height % 2 != 0)
            throw new ArgumentException("Height must be dividable by 2.");
        _camera = camera;
        _backgroundTexture = backgroundTexture;
        _pixels = Create(width, height);
        _objects = new Objects(this);
    }
    public Objects Objects => _objects;
    public int Width => _pixels.GetLength(0);
    public int Height => _pixels.GetLength(1);
    private int BackgroundWidth => _backgroundTexture.GetLength(0);
    private int BackgroundHeight => _backgroundTexture.GetLength(1);
    public void Update(float deltaTime, bool renderAll = false)
    {
        int xStart, yStart, xEnd, yEnd;
        lock (_camera)
        {
            (xStart, xEnd, yStart, yEnd) = GetVisibleArea(renderAll);
        }
        Objects.Update(deltaTime);
        int backgroundWidth = BackgroundWidth, backgroundHeight = BackgroundHeight;
        for (int y = yStart; y < yEnd; y++)
        {
            for (int x = xStart; x < xEnd; x++)
            {
                RGB? rgb = null;
                GameObject? gameObject = _objects.GetObjectMapPixel(x, y);
                if (gameObject is not null && gameObject.Visible)
                {
                    rgb = gameObject.GetColor(x - gameObject.X, y - gameObject.Y);
                }
                rgb ??= _backgroundTexture[x % backgroundWidth, y % backgroundHeight];
                _pixels[x, y].SetNewRGB((RGB)rgb);
            }
        }
    }
    public void SetSpriteSnapshot(Objects objects)
    {
        if (ReferenceEquals(objects, _objects))
            throw new ArgumentException("Sprites cannot be the same.");
        _objects = objects;
    }
    public bool ValidPixelCoordinates(int x, int y) => x >= 0 && y >= 0 && x < Width && y < Height;
    internal Pixel GetPixel(int x, int y)
    {
        if (!ValidPixelCoordinates(x, y))
            throw new InvalidDataException("X or Y invalide");
        return _pixels[x, y];
    }
    private Pixel[,] Create(int width, int height)
    {
        Pixel[,] newMap = new Pixel[width, height];
        for (int y = 0; y < height; y++)
            for (int x = 0; x < width; x++)
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