using System.Diagnostics;
using JumpOn.CheckPoints;
using JumpOn.Collectables;
using JumpOn.Entities;
using JumpOn.Interactables;
using JumpOn.Traps;
using JumpOn.UI.InfoBoxes;
using MCR_Engine.Core;
using MCR_Engine.InputSystem;
using MCR_Engine.Objects;
using MCR_Engine.Rendering.Output;
using MCR_Engine.Rendering.UI;

namespace JumpOn.Core;

public class Game
{
    private readonly Engine _engine;
    private readonly Account _account;
    private readonly Clock _clock;
    private readonly List<Interactable> _interactables;
    private readonly List<Trap> _traps;
    private readonly List<Collectable> _collectables;
    private readonly List<Entity> _entities;
    private readonly CheckPointManager _checkPoints;
    private readonly PauseChecker _pause;
    private readonly UIElement[] _sideBar;
    private readonly Player _player;
    private readonly Goal _goal;
    public Game(Engine engine, Account account, int level)
    {
        _engine = engine;
        _account = account;
        CenteredInfoBox centeredInfoBox = (CenteredInfoBox)UI.Add(new CenteredInfoBox(Input));
        MobileInfoBox mobileInfoBox = (MobileInfoBox)UI.Add(new MobileInfoBox());
        _clock = new Clock(); _pause = new PauseChecker(Input, centeredInfoBox);
        _engine.SetScene(LevelLoader.Read($"Scenes/level{level}.txt", _engine, mobileInfoBox, _clock));
        _engine.Scene.Update(1, true);
        _interactables = Sprites.GetAllSpritesOfType<Interactable>();
        _traps = Sprites.GetAllSpritesOfType<Trap>();
        _collectables = Sprites.GetAllSpritesOfType<Collectable>();
        _checkPoints = new CheckPointManager(Sprites, centeredInfoBox, Camera);
        _checkPoints.GetPlayerSpawnPosition(out int playerX, out int playerY);
        _player = (Player)Sprites.Add(new Player(playerX, playerY, Textures, Input, _clock));
        _sideBar = [UI.Add(new UIElement(19, 6, 2, 1, true)), UI.Add(new UIElement(19, 6, 2, 8, true)), UI.Add(new UIElement(19, 6, 2, 15, true))];
        _entities = Sprites.GetAllSpritesOfType<Entity>();
        _goal = GetGoal();
        Camera.SetFollowedSprite(_player, 10, 10);
    }
    private Scene Scene => _engine.Scene;
    private SpriteManager Sprites => _engine.Sprites;
    private Input Input => _engine.Input;
    private UIManager UI => _engine.UI;
    private Textures Textures => _engine.Textures;
    private Camera Camera => _engine.Camera;
    public int Play()
    {
        Stopwatch stopwatch = Stopwatch.StartNew();
        do
        {
            double deltaTime = stopwatch.Elapsed.TotalSeconds;
            if (!_pause.Check() && deltaTime < 1)
                _clock.Update((int)(deltaTime * 1000));
            stopwatch.Restart();
            Trap.Update(_traps);
            Interactable.Update(_interactables);
            Collectable.Update(_collectables);
            Entity.Update(_entities);
            Scene.Update(deltaTime);
            Entity.ResetInteractable(_entities);
            UpdateSideBar();
            if (!_player.Alive || Input.Get("L").Pressed)
                _checkPoints.ResetPlayerToChekPoint(_player);
        } while (!_goal.EndGame && !Input.Get("Up").Pressed);
        return _clock.PassedSeconds;
    }
    private Goal GetGoal()
    {
        List<Goal> goal = Sprites.GetAllSpritesOfType<Goal>();
        if (goal.Count != 1)
            throw new ArgumentException("Invalid amount of goals");
        goal[0].Keys = Sprites.GetAllSpritesOfType<GameKey>();
        return goal[0];
    }
    private void UpdateSideBar()
    {
        _sideBar[0].UpdateContent([["!SPIELINFO"], [$"Level: {_account.CurrentLevel+1}", $"Schlüssel: {_goal.CollectedKeysAmount}/{_goal.KeysAmount}"]]);
        _sideBar[1].UpdateContent([["!SPIELER"], [$"Name: {_account.Name}", $"Leben: {_player.Health}/{Player.MaxHealth}"]]);
        _sideBar[2].UpdateContent([["!TASTENINFO"], [$"[{'P'}] - Pause", $"[{'L'}] - Verlassen"]]);
    }
}