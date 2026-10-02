using MCR_Engine.Core;
using MCR_Engine.Objects;

namespace MCR_Engine.Rendering.Output;

public class Camera
{
    private const double DefaultDecelerationX = 30, DefaultDecelerationY = 120;
    private readonly Renderer _render;
    private Sprite? _followedSprite;
    private double _xOffset, _yOffset;
    private double _velocityX, _velocityY;
    internal Camera(Renderer render)
    {
        _render = render;
        _xOffset = 0; _yOffset = 0;
        _velocityX = 0; _velocityY = 0;
        _followedSprite = null;
    }
    public int XOffset => (int)_xOffset;
    public int YOffset => (int)_yOffset;
    public Sprite FollowedSprite
    {
        set =>  _followedSprite = value;
    }
    private bool CameraMoving => Math.Abs(_velocityX) + Math.Abs(_velocityY) != 0;
    private int SpriteX => _followedSprite is null ? 0 : _followedSprite.X + _followedSprite.Width / 2;
    private int SpriteY => _followedSprite is null ? 0 : (_followedSprite.Y + _followedSprite.Height / 2) / 2;
    public void RecalculateOffset(double deltaTime)
    {
        if (_followedSprite is null)
            return;
        List<Direction> directions = GetCameraDirection();
        if (InDeadZone(directions))
        {
            if (!CameraMoving)   
                return;
        }
        if (directions.Contains(Direction.Left))
            _velocityX -= 70 * deltaTime;
        else if (directions.Contains(Direction.Right))
            _velocityX += 70 * deltaTime;
        if (directions.Contains(Direction.Up))
            _velocityY -= 100 * deltaTime;
        else if (directions.Contains(Direction.Down))
            _velocityY += 100 * deltaTime;
        if (Math.Abs(_velocityX) > Math.Abs(_followedSprite.VelocityX))
        {
            _velocityX = Math.Clamp(_velocityX, -50, 50);
            directions.Remove(Direction.Left);
            directions.Remove(Direction.Right);
        }
        if (Math.Abs(_velocityY) > 10)
        {
            _velocityY = Math.Clamp(_velocityY, -10, 10);
            directions.Remove(Direction.Up);
            directions.Remove(Direction.Down);
        }
        
        double newXOffset = _xOffset + _velocityX * deltaTime;
        double newYOffset = _yOffset + _velocityY * deltaTime;
        SetOffsetIntoMapBorder(ref newXOffset, ref newYOffset);
        _xOffset = newXOffset;
        _yOffset = newYOffset;
        ReduceCameraSpeed(deltaTime);

    }
    public int GetRenderBorderX(int xOffset) => Math.Min(_render.PixelPairWidth, xOffset + Console.WindowWidth);
    public int GetRenderBorderY(int yOffset) => Math.Min(_render.PixelPairHeight, yOffset + Console.WindowHeight - 1);
    public bool InView(int x, int y) => x >= XOffset && y >= YOffset && x < XOffset + Console.WindowWidth && y < YOffset + Console.WindowHeight;
    private bool InDeadZone(List<Direction> directions) => directions.Count is 0;
    private List<Direction> GetCameraDirection()
    {
        List<Direction> directions = [];
        int width = Console.WindowWidth, height = Console.WindowHeight;
        int leftBorder = XOffset + CalculatePercentageDistance(width, 35), rightBorder = XOffset + width - CalculatePercentageDistance(width, 40);
        int upBorder = YOffset + CalculatePercentageDistance(height, 15), downBorder = YOffset + height - CalculatePercentageDistance(height, 15);
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