using MCR_Engine.Core;
using MCR_Engine.InputSystem;
using MCR_Engine.Objects;

namespace MCR_Engine.Rendering.Output;

public class Camera
{
    private const double CameraDeadZoneLeft = 0.34, CameraDeadZoneRight = 0.34;
    private const double CameraDeadZoneTop = 0.35, CameraDeadZoneBottom = 0.30;
    private const double CinematicSpeedX = 80, CinematicSpeedY = CinematicSpeedX / 2;
    private const double MinMaxVelocityX = 20, MinMaxVelocityY = MinMaxVelocityX / 2;
    private const string UpKey = "Up", DownKey = "Down", LeftKey = "Left", RightKey = "Right";
    private readonly Renderer _render;
    private readonly Key[] _movementKeys;
    private double _xVelocity, _yVelocity;
    private double _xOffset, _yOffset;
    private double _currentMaxVelocityX, _currentMaxVelocityY;
    private int? _moveToX, _moveToY;
    internal Camera(Renderer render, Input input)
    {
        _xOffset = _yOffset = 0;
        _xVelocity = _yVelocity = 0;
        _currentMaxVelocityX = _currentMaxVelocityY = 0;
        _render = render;
        _movementKeys = [input.Get(UpKey), input.Get(DownKey), input.Get(LeftKey), input.Get(RightKey)];
        _moveToX = _moveToY = null;
        StopMoving = false;
    }
    public bool StopMoving { private get; set; }
    public GameObject? FollowedObject { get; private set; }
    public int XOffset => (int)Math.Round(_xOffset);
    public int YOffset => (int)Math.Round(_yOffset);
    public bool CinematicCamera => _moveToX is  not null || _moveToY is not null;
    private int ObjX
    {
        get
        {
            if (FollowedObject is null)
                return 0;
            return FollowedObject.X + FollowedObject.Width / 2;
        }
    }
    private int ObjY
    {
        get
        {
            if (FollowedObject is null)
                return 0;
            return (FollowedObject.Y + FollowedObject.Height / 2) / 2;
        }
    }
    public void RecalculateOffset(float deltaTime)
    {
        if (FollowedObject is null || StopMoving)
            return;
        int x = ObjX, y = ObjY;
        int width = Console.WindowWidth, height = Console.WindowHeight;
        MoveCameraWithInput();
        (Direction leftRight, Direction upDown) = GetCameraMoveDirections(_moveToX ?? x, _moveToY ?? y, width, height);
        
        _currentMaxVelocityX = GetMaxVelocityX(deltaTime);
        _currentMaxVelocityY = GetMaxVelocityY(deltaTime);
        
        _xVelocity = GetUpdatedVelocityX(leftRight, deltaTime);
        _yVelocity = GetUpdatedVelocityY(upDown, deltaTime);
        
        MoveOnX(deltaTime, width);
        MoveOnY(deltaTime, height);
        
        UpdateCinematicCamera(width, height);
    }
    public void GoToCoordinates(int x, int y)
    {
        _moveToX = x;
        _moveToY = y / 2;
        while (CinematicCamera)
        {
            Thread.Sleep(10);
        }
    }
    public void SetFollowedObject(GameObject followedObject)
    {
        ResetOffset();
        FollowedObject = followedObject;
    }
    public void SetOffset(int x, int y)
    {
        _xOffset = x;
        _yOffset = y;
    }
    public void ResetOffset() => _xOffset = _yOffset = 0;
    public bool InView(int x, int y) => _render.ValidPairCoordinates(x, y) && x >= XOffset && y >= YOffset && x < XOffset + Console.WindowWidth && y < YOffset + Console.WindowHeight;
    public int GetRenderBorderX(int xOffset) => Math.Min(_render.PixelPairWidth, xOffset + Console.WindowWidth);
    public int GetRenderBorderY(int yOffset) => Math.Min(_render.PixelPairHeight, yOffset + Console.WindowHeight);
    private void MoveCameraWithInput()
    {
        if (_movementKeys[0].Pressed)
            _yVelocity -= 2;
        else if (_movementKeys[1].Pressed)
            _yVelocity += 2;

        if (_movementKeys[2].Pressed)
            _xVelocity -= 2;
        else if (_movementKeys[3].Pressed)
            _xVelocity += 2;
    }
    private bool CanCameraMoveInDirection(Direction direction, int width, int height)
    {
        return (direction) switch
        {
            Direction.Left =>  _xOffset > 0,
            Direction.Right => _xOffset + width < _render.PixelPairWidth,
            Direction.Up => _yOffset > 0,
            Direction.Down => _yOffset + height < _render.PixelPairHeight,
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
        return _currentMaxVelocityX + (Math.Abs(FollowedObject.GetVelocity().X) - _currentMaxVelocityX) * 3 * deltaTime;
    }
    private double GetMaxVelocityY(double deltaTime)
    {
        if (CinematicCamera || !InView(ObjX, ObjY))
            return CinematicSpeedY;
        if (_currentMaxVelocityY < MinMaxVelocityY)
            return MinMaxVelocityY;
        return _currentMaxVelocityY + (Math.Abs(FollowedObject.GetVelocity().Y / 2) - _currentMaxVelocityY) * 3 * deltaTime;
    }
    private double ReduceCameraSpeed(double velocity, double reduction, double deltaTime)
    {
        double result = velocity * Math.Exp(-reduction * deltaTime);
        return Math.Abs(result) < 0.01 ? 0 : result;
    }
    private double GetUpdatedVelocityX(Direction leftRight, double deltaTime)
    {
        if (leftRight == Direction.Left)
            return _currentMaxVelocityX * -1;
        if (leftRight == Direction.Right)
            return _currentMaxVelocityX;
        return ReduceCameraSpeed(_xVelocity, 2.5, deltaTime);
    }
    private double GetUpdatedVelocityY(Direction upDown, double deltaTime)
    {
        if (upDown == Direction.Up)
            return _currentMaxVelocityY * -1;

        if (upDown == Direction.Down)
            return _currentMaxVelocityY;
        
        return ReduceCameraSpeed(_yVelocity, 5, deltaTime);
    }
    private void MoveOnX(float deltaTime, int windowWidth)
    {
        if (_xVelocity == 0)
            return;
        _xOffset += _xVelocity * deltaTime;
        if (_xOffset < 0)
            _xOffset = 0;
        else if (_xOffset + windowWidth >= _render.PixelPairWidth)
            _xOffset = _render.PixelPairWidth - windowWidth;
    }
    private void MoveOnY(float deltaTime, int windowHeight)
    {
        if (_yVelocity == 0)
            return;
        _yOffset += _yVelocity * deltaTime;
        if (_yOffset < 0)
            _yOffset = 0;
        else if (_yOffset + windowHeight >= _render.PixelPairHeight)
            _yOffset = _render.PixelPairHeight - windowHeight;
    }
    private void UpdateCinematicCamera(int width, int height)
    {
        if (!CinematicCamera)
            return;

        var directions = GetCameraMoveDirections(_moveToX ?? 0, _moveToY ?? 0, width, height);
        
        bool reachedX = directions.LeftRight == Direction.None;
        bool reachedY = directions.UpDown == Direction.None;

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