using System.Diagnostics;
using JumpOn.Entities;
using JumpOn.Interactables;
using JumpOn.Traps;
using MCR_Engine;
using MCR_Engine.Core;
using MCR_Engine.InputSystem;
using MCR_Engine.Objects;
using MCR_Engine.Rendering.Output;
using MCR_Engine.Rendering.UI;
using Switch = JumpOn.Interactables.Switch;

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
        _sprites.Add(new Spike(30, 113, _textures));
        _sprites.Add(new Laser(0, 0, 2000, 700, _textures, _clock));
        _sprites.Add(new ArrowMachine(100, 5, Direction.Left, 700, _textures, _clock));
        //_sprites.Add(new SolidObject(50, 3, _textures.Get("brickBlock")));
        Ladder l = _sprites.GetAllSpritesOfType<Ladder>()[0];
        _sprites.Add(new Switch(80, 44, l, _input, _camera, _textures, _clock));
        //SolidObject b = _sprites.GetAllSpritesOfType<SolidObject>()[0];
        Player p = (Player)_sprites.Add(new Player(10, 0, _textures, _input, _clock));
        List<Interactable> interactables = Interactable.LoadInteractables(_sprites);
        List<Trap> traps = Trap.LoadTraps(_sprites);
        //b.AddMoveToQueue(0.7, 0, 1000);
        UIElement myUi = new UIElement(5, 3, 2, 1, true);
        _ui.AddUIElement(myUi);
        _camera.FollowedSprite = p;
        Stopwatch sw = Stopwatch.StartNew(), sw2 = Stopwatch.StartNew();
        double lastTime = sw.Elapsed.TotalSeconds;
        do
        {
            double currentTime = sw.Elapsed.TotalSeconds;
            double deltaTime = currentTime - lastTime;
            lastTime = currentTime;
            Interactable.Update(interactables);
            Trap.UpdateTraps(traps);
            _scene.Update(deltaTime);
            p.Update();
            myUi.UpdateContent([[$"{p.Health}"]]);
            _clock.Update((int)sw2.ElapsedMilliseconds);
            sw2.Restart();
        } while (true);
    }
}