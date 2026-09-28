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

namespace JumpOn.Core;

public class Game
{
    private readonly UIManager _ui;
    private readonly Scene _scene;
    private readonly Camera _camera;
    private readonly SpriteManager _sprites;
    private readonly Textures _textures;
    private readonly Input _input;
    private readonly PauseChecker _pause;
    private readonly Clock _clock;
    private readonly UIElement[] sideBar;
    private readonly MobileInfoBox _mobileInfoBox;
    private readonly CenteredInfoBox _centeredInfoBox;
    public Game(Engine engine)
    {
        _input = engine.Input;
        _ui = engine.UI;
        _clock = new Clock();
        _ui.AddUIElement(_mobileInfoBox= new MobileInfoBox());
        _ui.AddUIElement(_centeredInfoBox = new CenteredInfoBox(_input)); 
        sideBar = 
        [
            _ui.AddUIElement(new UIElement(19, 8, 2, 1, true)), 
            _ui.AddUIElement(new UIElement(19, 6, 2, 10, true)), 
            _ui.AddUIElement(new UIElement(19, 6, 2, 17, true))
        ];
        engine.SetScene(LevelLoader.Read("test.txt", engine, _mobileInfoBox, _clock));
        _camera = engine.Camera;
        _scene = engine.Scene;
        _sprites = engine.Sprites;
        _textures = engine.Textures;
        _pause = new PauseChecker(_input, _centeredInfoBox);
    }
    public void Play()
    {
        Player p = (Player)_sprites.Add(new Player(10, 0, _textures, _input, _clock));
        List<Entity> entities = Entity.LoadEntities(_sprites);
        List<Interactable> interactables = Interactable.LoadInteractables(_sprites);
        List<Trap> traps = Trap.LoadTraps(_sprites);
        List<Collectable> collectables = Collectable.LoadCollectables(_sprites);
        CheckPointManager checkPointManager = new CheckPointManager(_sprites, _centeredInfoBox);
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
            Entity.Update(entities);
            _scene.Update(deltaTime);
            if (!p.Alive)
                checkPointManager.ResetPlayerToChekPoint(p);
            _clock.Update((int)sw2.ElapsedMilliseconds);
            sw2.Restart();
            _pause.Check();
        } while (true);
    }
    private void UpdateSideBar(Player player)
    {
        sideBar[0].UpdateContent([["!SPIELINFO"], [$"Name: {account.Name}", $"Level: {account.CurrentLevel+1}"], [$"Schlüssel: {goal.CollectedKeys}/{goal.KeysAmount}"]]);
        sideBar[1].UpdateContent([["!SPIELER"], [$"Leben: {player.Health}/{Player.MaxHealth}"]]);
        sideBar[2].UpdateContent([["!TASTENINFO"], [$"[{PauseButtonKeyName}] - Pause", $"[{LeaveButtonKeyName}] - Verlassen"]]);
    }
}