using System.Diagnostics;
using JumpOn.CheckPoints;
using JumpOn.Collectables;
using JumpOn.Entities;
using JumpOn.Interactables;
using JumpOn.Traps;
using JumpOn.UI.InfoBoxes;
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
        MobileInfoBox mB = new MobileInfoBox();
        CenteredInfoBox cB = new CenteredInfoBox(_input);
        _ui.AddUIElement(mB);
        _ui.AddUIElement(cB);
        _sprites.Add(new Spike(30, 113, _textures));
        _sprites.Add(new Laser(0, 0, 2000, 700, _textures, _clock));
        _sprites.Add(new ArrowMachine(100, 5, Direction.Left, 1200, _textures, _clock));
        _sprites.Add(new Medkit(200, 0, _textures, _input, mB));
        _sprites.Add(new CheckPoint(150, 0, _textures));
        _sprites.Add(new GameKey(350, 0, _textures));
        _sprites.Add(new GameKey(360, 0, _textures));
        _sprites.Add(new Goal(260, 104, _textures, _sprites, _input, mB));
        //_sprites.Add(new SolidObject(50, 3, _textures.Get("brickBlock")));
        Ladder l = _sprites.GetAllSpritesOfType<Ladder>()[0];
        _sprites.Add(new Switch(80, 44, l, _input, _camera, mB, _textures, _clock));
        //SolidObject b = _sprites.GetAllSpritesOfType<SolidObject>()[0];
        Player p = (Player)_sprites.Add(new Player(10, 0, _textures, _input, _clock));
        List<Interactable> interactables = Interactable.LoadInteractables(_sprites);
        List<Trap> traps = Trap.LoadTraps(_sprites);
        List<Collectable> collectables = Collectable.LoadCollectables(_sprites);
        CheckPointManager checkPointManager = new CheckPointManager(_sprites, cB);
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
            Trap.Update(traps);
            Collectable.Update(collectables);
            _scene.Update(deltaTime);
            p.Update();
            if (!p.Alive)
                checkPointManager.ResetPlayerToChekPoint(p);
            myUi.UpdateContent([[$"{p.Health}"]]);
            _clock.Update((int)sw2.ElapsedMilliseconds);
            sw2.Restart();
        } while (true);
    }
}