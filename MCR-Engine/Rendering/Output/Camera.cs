using System.Diagnostics;
using MCR_Engine.Core;
using MCR_Engine.Objects;

namespace MCR_Engine.Rendering.Output;

public class Camera
{
    private const double MinMaxVelocityX = 20, MinMaxVelocityY = 20;
    private const double DefaultAccelerationX = 220, DefaultAccelerationY = 300;
    private const double DefaultDecelerationX = 40, DefaultDecelerationY = 150;
    private readonly Renderer _render;
    private Sprite? _followedSprite;
    private double _xOffset, _yOffset;
    private double _currentMaxVelocityX, _currentMaxVelocityY;
    private double _velocityX, _velocityY;
    private bool _cinematicCamera;
    internal Camera(Renderer render)
    {
        _render = render;
        _xOffset = 0; _yOffset = 0;
        _velocityX = 0; _velocityY = 0;
        _followedSprite = null;
        _cinematicCamera = false;
    }
    public bool CinematicCamera
    {
        get => _cinematicCamera;
        set => _cinematicCamera = value;
    }
    public Sprite FollowedSprite => _followedSprite ?? throw new InvalidOperationException("Followed sprite is not set");
    public int XOffset => (int)Math.Round(_xOffset);
    public int YOffset => (int)Math.Round(_yOffset);
    private double SpriteVelocityX => _followedSprite is null ? 0 : Math.Abs(_followedSprite.VelocityX) * 1.05;
    private double SpriteVelocityY => _followedSprite is null ? 0 : Math.Abs(_followedSprite.VelocityY) / 2;
    public void RecalculateOffset(double deltaTime)
    {
        if (_followedSprite is null)
            return;
        double newXOffset = _xOffset + _velocityX * deltaTime;
        double newYOffset = _yOffset + _velocityY * deltaTime;
        SetOffsetIntoMapBorder(ref newXOffset, ref newYOffset);
        _xOffset = newXOffset; _yOffset = newYOffset;
        _currentMaxVelocityX = GetMaxVelocityX(deltaTime);
        _currentMaxVelocityY = GetMaxVelocityY(deltaTime);
        ReduceCameraSpeed(deltaTime);
        List<Direction> directions = GetCameraDirection(_followedSprite);

        if (InDeadZone(_followedSprite))
            return;

        _velocityX += GetVelocityXAdd(directions, deltaTime);
        _velocityY += GetVelocityYAdd(directions, deltaTime);

        if (!InView(GetSpriteX(_followedSprite), GetSpriteY(_followedSprite)))
        {
            if (!_cinematicCamera)
                SetFollowedSprite(_followedSprite, _currentMaxVelocityX, _currentMaxVelocityY);
            return;
        }
        
        if (Math.Abs(_velocityX) > _currentMaxVelocityX)
            _velocityX = (_velocityX < 0 ? -SpriteVelocityX : SpriteVelocityX);
        if (Math.Abs(_velocityY) > _currentMaxVelocityY)
            _velocityY = (_velocityY < 0 ? -SpriteVelocityY : SpriteVelocityY);
    }
    public void SetOffset(double xOffset, double yOffset)
    {
        SetOffsetIntoMapBorder(ref xOffset, ref yOffset);
        _velocityX = _velocityY = 0;
        _xOffset = xOffset;
        _yOffset = yOffset;
    }
    public void ResetOffset()
    {
        _xOffset = _yOffset = _velocityX = _velocityY = 0;
        _followedSprite = null;
    }
    public bool InView(int x, int y) => _render.ValidPairCoordinates(x, y) && x >= XOffset && y >= YOffset && x < XOffset + Console.WindowWidth && y < YOffset + Console.WindowHeight;
    public int GetRenderBorderX(int xOffset) => Math.Min(_render.PixelPairWidth, xOffset + Console.WindowWidth);
    public int GetRenderBorderY(int yOffset) => Math.Min(_render.PixelPairHeight, yOffset + Console.WindowHeight);
    private List<Direction> GetCameraDirection(Sprite sprite)
    {
        List<Direction> directions = [];
        int spriteX = GetSpriteX(sprite), spriteY = GetSpriteY(sprite);
        int width = Console.WindowWidth, height = Console.WindowHeight;
        int leftBorder = XOffset + CalculatePercentageDistance(width, 21), rightBorder = XOffset + width - 20 - CalculatePercentageDistance(width, 18);
        int upBorder = YOffset + CalculatePercentageDistance(height, 35), downBorder = YOffset + height - CalculatePercentageDistance(height, 30);
        
        if (spriteX < leftBorder && _xOffset > 0)
            directions.Add(Direction.Left);
        else if (spriteX > rightBorder && _xOffset + width < _render.PixelPairWidth - 1)
            directions.Add(Direction.Right);
        
        if (spriteY < upBorder && _yOffset > 0)
            directions.Add(Direction.Up);
        else if (spriteY > downBorder && _yOffset + height < _render.PixelPairHeight - 1)
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
        int width = Console.WindowWidth, height = Console.WindowHeight;
        
        if (xOffset >= _render.PixelPairWidth - width)
        {
            xOffset = _render.PixelPairWidth - width;
            _velocityX = 0;
        }
        if (xOffset < 0)
            xOffset = _velocityX = 0;
        
        if (yOffset >= _render.PixelPairHeight - height)
        {
            yOffset = _render.PixelPairHeight - height;
            _velocityY = 0;
        }
        if (yOffset < 0)
            yOffset = _velocityY = 0;
    }
    private double GetMaxVelocityX(double deltaTime)
    {
        if (_currentMaxVelocityX < MinMaxVelocityX)
            return MinMaxVelocityX;
        return _currentMaxVelocityX + (SpriteVelocityX - _currentMaxVelocityX) * 3 * deltaTime;
    }
    private double GetMaxVelocityY(double deltaTime)
    {
        if (_currentMaxVelocityY < MinMaxVelocityY)
            return MinMaxVelocityY;
        return _currentMaxVelocityY + (SpriteVelocityY - _currentMaxVelocityY) * 3 * deltaTime;
    }
    private bool InDeadZone(Sprite sprite) => GetCameraDirection(sprite).Count is 0;
    private static double GetVelocityXAdd(List<Direction> directions, double deltaTime)
    {
        if (directions.Contains(Direction.Left))
            return -(DefaultAccelerationX * deltaTime);
        if (directions.Contains(Direction.Right))
            return +(DefaultAccelerationX * deltaTime);
        return 0;
    }
    private static double GetVelocityYAdd(List<Direction> directions, double deltaTime)
    {
        if (directions.Contains(Direction.Up))
            return -(DefaultAccelerationY * deltaTime);
        if (directions.Contains(Direction.Down))
            return +(DefaultAccelerationY * deltaTime);
        return 0;
    }
    private static int GetSpriteX(Sprite sprite) => sprite.X + sprite.Width / 2;
    private static int GetSpriteY(Sprite sprite) => (sprite.Y + sprite.Height / 2) / 2;
    private static int CalculatePercentageDistance(int distance, int percentage) => (int)(distance * percentage / 100.0);
    public async Task SetFollowedSprite(Sprite sprite, double maxVelocityX, double maxVelocityY)
    {
        if (_cinematicCamera)
            return;
        _cinematicCamera = true;
        _followedSprite = sprite;
        _currentMaxVelocityX = _currentMaxVelocityY = double.PositiveInfinity; 
        while (!InDeadZone(_followedSprite))
            await Task.Delay(50);
        _currentMaxVelocityX = maxVelocityX; _currentMaxVelocityY = maxVelocityY;
        _velocityX = _velocityY = 0;
        _cinematicCamera = false;
    }
}