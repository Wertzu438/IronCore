using System.Diagnostics;
using MCR_Engine.Core;
using MCR_Engine.Objects;

namespace MCR_Engine.Rendering.Output;

public class Camera
{
    private const double DefaultDecelerationX = 50, DefaultDecelerationY = 120;
    private readonly Renderer _render;
    private Sprite? _followedSprite;
    private double _xOffset, _yOffset;
    private double _velocityX, _velocityY;
    private double _maxVelocityX;
    internal Camera(Renderer render)
    {
        _render = render;
        _xOffset = 0; _yOffset = 0;
        _velocityX = 0; _velocityY = 0;
        _followedSprite = null;
    }
    public int XOffset => (int)Math.Round(_xOffset);
    public int YOffset => (int)Math.Round(_yOffset);
    public Sprite FollowedSprite
    {
        set =>  _followedSprite = value;
    }
    private int SpriteX => _followedSprite is null ? 0 : _followedSprite.X + _followedSprite.Width / 2;
    private int SpriteY => _followedSprite is null ? 0 : (_followedSprite.Y + _followedSprite.Height / 2) / 2;
    private double SpriteVelocityX => _followedSprite is null ? 0 : Math.Abs(_followedSprite.VelocityX) * 1.1;
    private double SpriteVelocityY => _followedSprite is null ? 0 : Math.Abs(_followedSprite.VelocityY) * 1.1;
    public void RecalculateOffset(double deltaTime)
    {
        if (_followedSprite is null)
            return;
        if (_followedSprite.VelocityX > _maxVelocityX)
            _maxVelocityX = _followedSprite.VelocityX;
        List<Direction> directions = GetCameraDirection();
        ReduceCameraSpeed(deltaTime);
        if (InDeadZone(directions))
        {
            return;
        }
        if (directions.Contains(Direction.Left))
            _velocityX -= 120 * deltaTime;
        else if (directions.Contains(Direction.Right))
            _velocityX += 120 * deltaTime;
        if (directions.Contains(Direction.Up))
            _velocityY -= 100 * deltaTime;
        else if (directions.Contains(Direction.Down))
            _velocityY += 100 * deltaTime;
        if (Math.Abs(_velocityX) > _maxVelocityX)
        {
            _velocityX = (_velocityX < 0 ? -SpriteVelocityX : SpriteVelocityX);
        }
        if (Math.Abs(_velocityY) > SpriteVelocityY)
        {
            _velocityY = (_velocityY < 0 ? -SpriteVelocityY : SpriteVelocityY);
        }
        double newXOffset = _xOffset + _velocityX * deltaTime;
        double newYOffset = _yOffset + _velocityY * deltaTime;
        SetOffsetIntoMapBorder(ref newXOffset, ref newYOffset);
        _xOffset = newXOffset;
        _yOffset = newYOffset;
    }
    public int GetRenderBorderX(int xOffset) => Math.Min(_render.PixelPairWidth, xOffset + Console.WindowWidth);
    public int GetRenderBorderY(int yOffset) => Math.Min(_render.PixelPairHeight, yOffset + Console.WindowHeight - 1);
    public bool InView(int x, int y) => x >= XOffset && y >= YOffset && x < XOffset + Console.WindowWidth && y < YOffset + Console.WindowHeight;
    private bool InDeadZone(List<Direction> directions) => directions.Count is 0;
    private List<Direction> GetCameraDirection()
    {
        List<Direction> directions = [];
        int width = Console.WindowWidth, height = Console.WindowHeight;
        int leftBorder = XOffset + CalculatePercentageDistance(width, 20), rightBorder = XOffset + width - 20 - CalculatePercentageDistance(width, 20);
        int upBorder = YOffset + CalculatePercentageDistance(height, 30), downBorder = YOffset + height - CalculatePercentageDistance(height, 15);
        if (SpriteX < leftBorder)
            directions.Add(Direction.Left);
        else if (SpriteX > rightBorder)
            directions.Add(Direction.Right);
        
        if (SpriteY < upBorder)
            directions.Add(Direction.Up);
        else if (SpriteY > downBorder)
            directions.Add(Direction.Down);
        return directions;
    }
    private void ReduceCameraSpeed(double deltaTime)
    {
        if (_velocityY > 0)
            _velocityY = Math.Max(_velocityY - DefaultDecelerationY * deltaTime, 0);
        else if (_velocityY < 0)
            _velocityY = Math.Min(_velocityY + DefaultDecelerationY * deltaTime, 0);
        
        if (_velocityX > 0)
            _velocityX = Math.Max(_velocityX - DefaultDecelerationX * deltaTime, 0);
        else if (_velocityX < 0)
            _velocityX = Math.Min(_velocityX + DefaultDecelerationX * deltaTime, 0);
    }
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
    private static int CalculatePercentageDistance(int distance, int percentage) => (int)(distance * percentage / 100.0);
}