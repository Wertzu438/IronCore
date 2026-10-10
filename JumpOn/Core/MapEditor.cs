using System.Diagnostics;
using MCR_Engine.Core;
using MCR_Engine.InputSystem;
using MCR_Engine.Objects;
using MCR_Engine.Rendering.Output;

namespace JumpOn.Core;

public class MapEditor
{
    private readonly Scene _scene;
    private readonly Camera _camera;
    private readonly SpriteManager _sprites;
    private readonly Key[] _movementKeys;
    private Sprite _selectedSprite;
    private int _selectedSpriteIndex;
    public MapEditor(Engine engine)
    {
        _selectedSpriteIndex = 3;
        //_sprites = engine.Sprites;
        _movementKeys = [engine.Input.Get("A"), engine.Input.Get("D"), engine.Input.Get("W"), engine.Input.Get("S")];
        _selectedSprite = GetSelectedSprite(_selectedSpriteIndex);
        _camera = engine.Camera;
        //_scene = engine.Scene;
    }
    public void Run()
    {
        _camera.SetFollowedSprite(_selectedSprite, 0, 0);
        Stopwatch stopwatch = Stopwatch.StartNew();
        do
        {
            double deltaTime = stopwatch.Elapsed.TotalSeconds;
            stopwatch.Restart();
            Direction direction = GetMovingDirection();
            if (direction is not Direction.None)
                _selectedSprite.AddVelocity(direction, 200, 80);
            _scene.Update(deltaTime, true);
        } while (true);
    }
    private Direction GetMovingDirection()
    {
        if (_movementKeys[0].Pressed)
            return Direction.Left;
        if (_movementKeys[1].Pressed)
            return Direction.Right;
        if (_movementKeys[2].Pressed)
            return Direction.Up;
        if (_movementKeys[3].Pressed)
            return Direction.Down;
        return Direction.None;
    }
    private Sprite GetSelectedSprite(int index)
    {
        if (index < _sprites.Count)
            return _sprites.Get(index);
        throw new IndexOutOfRangeException();
        //return _sprites.Add(new Sprite());
    }
}