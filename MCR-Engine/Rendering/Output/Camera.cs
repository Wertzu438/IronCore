using MCR_Engine.Objects;

namespace MCR_Engine.Rendering.Output;

public class CameraOld
{
    private const double SlowSpeedX = 0.7, FastSpeedX = 2.2;
    private const double SlowSpeedY = 0.2, FastSpeedY = 1.0;
    private readonly Renderer _render;
    private Sprite? _followedSprite;
    private double _xOffset, _yOffset;
    internal CameraOld(Renderer render)
    {
        _render = render;
        _xOffset = 0; _yOffset = 0;
        _followedSprite = null;
    }
    public int XOffset => (int)_xOffset;
    public int YOffset => (int)_yOffset;
    public Sprite FollowedSprite
    {
        set =>  _followedSprite = value;
    }
    private int SpriteX => _followedSprite is null ? 0 : (_followedSprite.X - XOffset);
    private int SpriteY => _followedSprite is null ? 0 : (((_followedSprite.Y + _followedSprite.Height / 2) / 2) - YOffset);
    public void RecalculateOffset()
    {
        int width = Console.WindowWidth, height = Console.WindowHeight;
        int spriteX = SpriteX, spriteY = SpriteY;
        int firstBorderX = CalculateDistance(width, 30), secondBorderX = CalculateDistance(width, 28);
        int firstBorderY = CalculateDistance(height, 40), secondBorderY = CalculateDistance(height, 28);
        int antiJitterZoneX = CalculateDistance(width, 15), antiJitterZoneY = CalculateDistance(height, 15);
        double newX = _xOffset, newY = _yOffset;
        if (spriteX > width - firstBorderX + antiJitterZoneX)
        {
            newX += spriteX > width - secondBorderX ? SlowSpeedX : FastSpeedX;
        }
        else if (spriteX < firstBorderX - antiJitterZoneX)
        {
            newX -= spriteX < secondBorderX ? SlowSpeedX : FastSpeedX;
        }
        
        if (spriteY > height - firstBorderY + antiJitterZoneY)
        {
            newY += spriteY > Console.WindowHeight - secondBorderY ? FastSpeedY : SlowSpeedY;
        }
        else if (spriteY < firstBorderY - antiJitterZoneY)
        {
            newY -= spriteY < secondBorderY ? FastSpeedY : SlowSpeedY;
        }
        SetOffsetIntoMapBorder(ref newX, ref newY);
        _xOffset = newX; _yOffset = newY;
    }
    public int GetRenderBorderX(int xOffset) => Math.Min(_render.PixelPairWidth, xOffset + Console.WindowWidth);
    public int GetRenderBorderY(int yOffset) => Math.Min(_render.PixelPairHeight, yOffset + Console.WindowHeight - 1);
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