using MCR_Engine.Objects;

namespace MCR_Engine.Rendering.Output;

public class Camera
{
    private const double SlowSpeedX = 0.7, FastSpeedX = 2.2;
    private const double SlowSpeedY = 0.5, FastSpeedY = 1.0;
    private readonly Renderer _render;
    private Sprite? _followedSprite;
    private double _xOffset, _yOffset;
    internal Camera(Renderer render)
    {
        _render = render;
        _xOffset = 0; _yOffset = 0;
        _followedSprite = null;
    }
    public int XOffset => (int)_xOffset;
    public int YOffset => (int)_yOffset;
    public int RenderBorderX => Math.Min(_render.PixelPairWidth, XOffset + Console.WindowWidth);
    public int RenderBorderY => Math.Min(_render.PixelPairHeight, YOffset + Console.WindowHeight - 1);
    public Sprite FollowedSprite
    {
        set =>  _followedSprite = value;
    }
    private int SpriteX => _followedSprite is null ? 0 : (_followedSprite.X - XOffset);
    private int SpriteY => _followedSprite is null ? 0 : ((_followedSprite.Y / 2) - YOffset);
    public void RecalculateOffset()
    {
        int width = Console.WindowWidth, height = Console.WindowHeight;
        int spriteX = SpriteX, spriteY = SpriteY;
        int firstBorderX = CalculateDistance(width, 30), secondBorderX = CalculateDistance(width, 28);
        int firstBorderY = CalculateDistance(height, 25), secondBorderY = CalculateDistance(height, 10);
        int deadZoneX = CalculateDistance(width, 15), deadZoneY = CalculateDistance(width, 28);
        double newX = _xOffset, newY = _yOffset;
        if (spriteX > width - firstBorderX + deadZoneX)
        {
            newX += spriteX > width - secondBorderX ? SlowSpeedX : FastSpeedX;
        }
        else if (spriteX < firstBorderX - deadZoneX)
        {
            newX -= spriteX < secondBorderX ? SlowSpeedX : FastSpeedX;
        }
        
        if (spriteY > height - firstBorderY)
        {
            newY += spriteY > Console.WindowHeight - secondBorderY ? FastSpeedY : SlowSpeedY;
        }
        else if (spriteY < firstBorderY)
        {
            newY -= spriteY < secondBorderY ? FastSpeedY : SlowSpeedY;
        }
        SetOffsetIntoMapBorder(ref newX, ref newY);
        _xOffset = newX; _yOffset = newY;
    }
    public bool InView(int x, int y) => x >= XOffset && y >= YOffset && x < XOffset + Console.WindowWidth && y < YOffset + Console.WindowHeight;
    private void SetOffsetIntoMapBorder(ref double xOffset, ref double yOffset)
    {
        if (xOffset < 0)
        {
            xOffset = 0;
        }
        else if (xOffset >= _render.PixelPairWidth - Console.WindowWidth)
        {
            xOffset = _render.PixelPairWidth - Console.WindowWidth;
        }

        if (yOffset < 0)
        {
            yOffset = 0;
        }
        else if (yOffset >= _render.PixelPairHeight - Console.WindowHeight)
        {
            yOffset = _render.PixelPairHeight - Console.WindowHeight;
        }
    }
    private static int CalculateDistance(int distance, int percentage) => (int)(distance * percentage / 100.0);
}