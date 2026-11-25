using MCR_Engine.Core;
using MCR_Engine.InputSystem;
using MCR_Engine.ObjectSystem;
using MCR_Engine.ObjectSystem.BaseObjectTypes;

namespace MCR_Engine.Rendering.Output;

public class Camera
{
    private const double CameraInputSpeedX = 110, CameraInputSpeedY = 110;
    private const double CameraDeadZoneLeft = 0.23, CameraDeadZoneRight = 0.30;
    private const double CameraDeadZoneTop = 0.25, CameraDeadZoneBottom = 0.20;
    private const double CinematicSpeedX = 80, CinematicSpeedY = 80;
    private const double MinMaxVelocityX = 20, MinMaxVelocityY = MinMaxVelocityX;
    private const string UpKey = "Up", DownKey = "Down", LeftKey = "Left", RightKey = "Right";
    private readonly Renderer _render;
    private readonly Key[] _movementKeys;
    private GameObject? _followedObject;
    private double _xVelocity, _yVelocity;
    private double _xOffset, _yOffset;
    private double _currentMaxVelocityX, _currentMaxVelocityY;
    private int _lastWindowWidth, _lastWindowHeight;
    private int? _moveToX, _moveToY;
    internal Camera(Renderer render, Input input)
    {
        _xOffset = _yOffset = 0;
        _xVelocity = _yVelocity = 0;
        _currentMaxVelocityX = _currentMaxVelocityY = 0;
        _render = render;
        _movementKeys = [input.Get(UpKey), input.Get(DownKey), input.Get(LeftKey), input.Get(RightKey)];
        _moveToX = _moveToY = null;
        _followedObject = null;
        LockPosition = false;
        _lastWindowWidth = Console.WindowWidth - 1;
        _lastWindowHeight = Console.WindowHeight * 2 - 3;
    }
    public int XOffset => (int)Math.Round(_xOffset);
    public int YOffset => (int)Math.Round(_yOffset);
    public bool CinematicCamera => _moveToX is  not null || _moveToY is not null;
    public bool LockPosition { private get; set; }
    private int ObjX
    {
        get
        {
            if (_followedObject is null)
                return 0;
            return (_followedObject.X + _followedObject.Width / 2) * Renderer.Zoom;
        }
    }
    private int ObjY
    {
        get
        {
            if (_followedObject is null)
                return 0;
            return (_followedObject.Y + _followedObject.Height / 2) * Renderer.Zoom;
        }
    }
    private double ObjVelocityX
    {
        get
        {
            if (_followedObject is not MovingObject mO)
                return 0;
            return Math.Abs(mO.GetVelocity().X);
        }
    }
    private double ObjVelocityY
    {
        get
        {
            if (_followedObject is not MovingObject mO)
                return 0;
            return Math.Abs(mO.GetVelocity().Y);
        }
    }
    public void RecalculateOffset(float deltaTime)
    {
        _lastWindowWidth = Console.WindowWidth; _lastWindowHeight = Console.WindowHeight * 2 - 1;
        if (_followedObject is null || LockPosition)
            return;
        int x = ObjX, y = ObjY;
        int renderWidth = Math.Min(_lastWindowWidth, _render.PixelMapWidth), renderHeight = Math.Min(_lastWindowHeight, _render.PixelMapHeight);
        MoveCameraWithInput(deltaTime);
        (Direction leftRight, Direction upDown) = GetCameraMoveDirections(_moveToX ?? x, _moveToY ?? y, renderWidth, renderHeight);
        
        _currentMaxVelocityX = GetMaxVelocityX(deltaTime);
        _currentMaxVelocityY = GetMaxVelocityY(deltaTime);
        
        _xVelocity = GetUpdatedVelocityX(leftRight, deltaTime);
        _yVelocity = GetUpdatedVelocityY(upDown, deltaTime);
        
        MoveOnX(deltaTime, renderWidth);
        MoveOnY(deltaTime, renderHeight);
        
        UpdateCinematicCamera(renderWidth, renderHeight);
    }
    public void GoToCoordinates(int x, int y)
    {
        _moveToX = x;
        _moveToY = y;
        while (CinematicCamera)
        {
            Thread.Sleep(10);
        }
    }
    public void SetFollowedObject(GameObject? followedObject)
    {
        ResetOffset();
        _followedObject = followedObject;
    }
    public void SetOffset(int x, int y)
    {
        _xOffset = x;
        _yOffset = y;
    }
    public void ResetOffset() => _xOffset = _yOffset = 0;
    public bool InView(int x, int y) => _render.ValidPairCoordinates(x, y) && x >= XOffset && y >= YOffset && x < XOffset + _lastWindowWidth && y < YOffset + _lastWindowHeight;
    public int GetRenderBorderX(int xOffset) => Math.Min(_render.PixelMapWidth, xOffset + _lastWindowWidth);
    public int GetRenderBorderY(int yOffset) => Math.Min(_render.PixelMapHeight, yOffset + _lastWindowHeight);
    private void MoveCameraWithInput(float deltaTime)
    {
        if (_movementKeys[0].Pressed)
            _yVelocity -= CameraInputSpeedY * deltaTime;
        else if (_movementKeys[1].Pressed)
            _yVelocity += CameraInputSpeedY * deltaTime;

        if (_movementKeys[2].Pressed)
            _xVelocity -= CameraInputSpeedX * deltaTime;
        else if (_movementKeys[3].Pressed)
            _xVelocity += CameraInputSpeedX * deltaTime;
    }
    private bool CanCameraMoveInDirection(Direction direction, int width, int height)
    {
        return (direction) switch
        {
            Direction.Left => _xOffset > 0,
            Direction.Right => _xOffset + width < _render.PixelMapWidth - 1,
            Direction.Up => _yOffset > 0,
            Direction.Down => _yOffset + height < _render.PixelMapHeight - 1,
            _ => false
        };
    }
    private (Direction LeftRight, Direction UpDown) GetCameraMoveDirections(int x, int y, int width, int height)
    {
        Direction leftRight, upDown;
        if (x <= GetDeadZoneBorder(Direction.Left, width, height) && CanCameraMoveInDirection(Direction.Left, width, height))
            leftRight = Direction.Left;
        else if (x >= GetDeadZoneBorder(Direction.Right, width, height) && CanCameraMoveInDirection(Direction.Right, width, height))
            leftRight = Direction.Right;
        else leftRight = Direction.None;
        
        if (y <= GetDeadZoneBorder(Direction.Up, width, height) && CanCameraMoveInDirection(Direction.Up, width, height))
            upDown = Direction.Up;
        else if (y >= GetDeadZoneBorder(Direction.Down, width, height) && CanCameraMoveInDirection(Direction.Down, width, height))
            upDown = Direction.Down;
        else upDown = Direction.None;
        
        return (leftRight, upDown);
    }
    private int GetDeadZoneBorder(Direction direction, int width, int height)
    {
        return (direction) switch
        {
            Direction.Left => (int)(_xOffset + width * CameraDeadZoneLeft),
            Direction.Right => (int)(_xOffset + width - width * CameraDeadZoneRight),
            Direction.Up => (int)(_yOffset + height * CameraDeadZoneTop),
            Direction.Down => (int)(_yOffset + height - height * CameraDeadZoneBottom),
            _ => throw new ArgumentException($"Invalid direction: {direction}"),
        };
    }
    private double GetMaxVelocityX(double deltaTime)
    {
        if (CinematicCamera || !InView(ObjX, ObjY))
            return CinematicSpeedX;
        if (_currentMaxVelocityX < MinMaxVelocityX)
            return MinMaxVelocityX;
        return _currentMaxVelocityX + (ObjVelocityX - _currentMaxVelocityX) * 3 * deltaTime;
    }
    private double GetMaxVelocityY(double deltaTime)
    {
        if (CinematicCamera || !InView(ObjX, ObjY))
            return CinematicSpeedY;
        if (_currentMaxVelocityY < MinMaxVelocityY)
            return MinMaxVelocityY;
        return _currentMaxVelocityY + (ObjVelocityY - _currentMaxVelocityY) * 3 * deltaTime;
    }
    private double ReduceCameraSpeed(double velocity, double reduction, double deltaTime)
    {
        if (reduction < 0)
            throw new ArgumentException("Reduction cannot be negative");
        double result = velocity * Math.Exp(-reduction * deltaTime);
        return Math.Abs(result) < 0.2 ? 0 : result;
    }
    private double GetUpdatedVelocityX(Direction leftRight, double deltaTime)
    {
        return leftRight switch
        {
            Direction.Left => _xVelocity > 0 ? 0 : _currentMaxVelocityX * -1,
            Direction.Right => _xVelocity < 0 ? 0 : _currentMaxVelocityX,
            _ => ReduceCameraSpeed(_xVelocity, 2.5, deltaTime)
        };
    }
    private double GetUpdatedVelocityY(Direction upDown, double deltaTime)
    {
        return (upDown) switch
        {
            Direction.Up => _yVelocity > 0 ? 0 : _currentMaxVelocityY * -1,
            Direction.Down => _yVelocity < 0 ? 0 : _currentMaxVelocityY,
            _ => ReduceCameraSpeed(_yVelocity, 2.5, deltaTime)
        };
    }
    private void MoveOnX(float deltaTime, int windowWidth)
    {
        if (_xVelocity == 0)
            return;
        _xOffset += _xVelocity * deltaTime * Renderer.Zoom;
        if (_xOffset + windowWidth >= _render.PixelMapWidth - 1)
            _xOffset = _render.PixelMapWidth - windowWidth - 1;
        if (_xOffset < 0)
            _xOffset = 0;
    }
    private void MoveOnY(float deltaTime, int windowHeight)
    {
        if (_yVelocity == 0)
            return;
        _yOffset += _yVelocity * deltaTime * Renderer.Zoom;
        if (_yOffset + windowHeight >= _render.PixelMapHeight - 1)
            _yOffset = _render.PixelMapHeight - windowHeight - 1;
        if (_yOffset < 0)
            _yOffset = 0;
    }
    private void UpdateCinematicCamera(int width, int height)
    {
        if (!CinematicCamera)
            return;
        var directions = GetCameraMoveDirections(_moveToX ?? 0, _moveToY ?? 0, width, height);
        bool reachedX = directions.LeftRight == Direction.None, reachedY = directions.UpDown == Direction.None;

        if (reachedX)
            _xVelocity = 0;
        if (reachedY)
            _yVelocity = 0;
        if (reachedX && reachedY)
        {
            _moveToX = _moveToY = null;
        }
    }
}