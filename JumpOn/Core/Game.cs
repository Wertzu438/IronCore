using System.Diagnostics;
using JumpOn.Entities;
using JumpOn.Interactables;
using MCR_Engine;
using MCR_Engine.Core;
using MCR_Engine.InputSystem;
using MCR_Engine.Objects;
using MCR_Engine.Rendering.Output;
using MCR_Engine.Rendering.UI;

namespace JumpOn;

public class Game
{
    private readonly UIManager _ui;
    private readonly Scene _scene;
    private readonly Camera _camera;
    private readonly SpriteManager _sprites;
    private readonly Textures _textures;
    private readonly Input _input;
    private readonly Clock _clock;
    public Game(Engine engine)
    {
        _camera = engine.Camera;
        _scene = engine.Scene;
        _ui = engine.UI;
        _sprites = engine.Sprites;
        _textures = engine.Textures;
        _input = engine.Input;
        _clock = new Clock();
    }
    public void Play()
    {
        Ladder l = _sprites.GetAllSpritesOfType<Ladder>()[0];
        SolidObject b = _sprites.GetAllSpritesOfType<SolidObject>()[0];
        Player p = (Player)_sprites.Add(new Player(10, 0, _textures, _input, _clock));
        //b.AddMoveToQueue(0.7, 0, 1000);
        _camera.FollowedSprite = p;
        Stopwatch sw = Stopwatch.StartNew();
        double lastTime = sw.Elapsed.TotalSeconds;
        do
        {
            double currentTime = sw.Elapsed.TotalSeconds;
            double deltaTime = currentTime - lastTime;
            lastTime = currentTime;
            p.Update();
            _scene.Update(deltaTime);
            //Thread.Sleep(Math.Max(1, 10 - (int)sw.ElapsedMilliseconds));
        } while (true);
    }
}