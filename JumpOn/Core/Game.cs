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
    private Scene _scene;
    private readonly Account _account;
    private readonly Clock _clock;
    private  List<Interactable> _interactables;
    private  List<Trap> _traps;
    private  List<Collectable> _collectables;
    private  List<Entity> _entities;
    private  CheckPointManager _checkPoints;
    private  PauseChecker _pause;
    private  UIElement[] _sideBar;
    private  Player _player;
    private  Goal _goal;
    public Game(Engine engine, Account account, int level)
    {
        _engine = engine;
        _account = account;
        CenteredInfoBox centeredInfoBox = (CenteredInfoBox)UI.Add(new CenteredInfoBox(Input));
        MobileInfoBox mobileInfoBox = (MobileInfoBox)UI.Add(new MobileInfoBox());
        _clock = new Clock(); _pause = new PauseChecker(Input, centeredInfoBox);
        _engine.SetRenderScene(_scene = LevelLoader.Read($"Scenes/level{level}.txt", _engine, mobileInfoBox, _clock));
        //_scene.Update(1, true);
        _interactables = _scene.Sprites.GetAllSpritesOfType<Interactable>();
        _traps = _scene.Sprites.GetAllSpritesOfType<Trap>();
        _collectables = _scene.Sprites.GetAllSpritesOfType<Collectable>();
        _checkPoints = new CheckPointManager(_engine, _scene, centeredInfoBox);
        _checkPoints.GetPlayerSpawnPosition(out int playerX, out int playerY);
        _player = (Player)_scene.Sprites.Add(new Player(playerX, playerY, Textures, Input, _clock));
        _sideBar = [UI.Add(new UIElement(19, 6, 2, 1, true)), UI.Add(new UIElement(19, 6, 2, 8, true)), UI.Add(new UIElement(19, 6, 2, 15, true)), 
            UI.Add(new UIElement(19, 10, 2, 24, true))
        ];
        _entities = _scene.Sprites.GetAllSpritesOfType<Entity>();
        _goal = GetGoal(_scene.Sprites);
        Camera.SetFollowedSprite(_player, 10, 10);
    }
    private Input Input => _engine.Input;
    private UIManager UI => _engine.UI;
    private Textures Textures => _engine.Textures;
    private Camera Camera => _engine.Camera;
    public bool Play(out int? passedSeconds, out int? repawns)
    {
        passedSeconds = null;
        repawns = null;
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
            _scene.Update(deltaTime, true);
            Entity.ResetInteractable(_entities);
            UpdateSideBar();
            if (!_player.Alive || Input.Get("R").Pressed)
            {
                _scene.SetSpriteSnapshot(_checkPoints.SnapShot);
                _scene.Update(0, true);
                _scene.Sprites.Add(_player);
                _interactables = _scene.Sprites.GetAllSpritesOfType<Interactable>();
                _traps = _scene.Sprites.GetAllSpritesOfType<Trap>();
                _collectables = _scene.Sprites.GetAllSpritesOfType<Collectable>();
                _checkPoints = new CheckPointManager(_engine, _scene, new CenteredInfoBox(Input));
                _checkPoints.ResetPlayerToCheckPoint(_player);
                _entities = _scene.Sprites.GetAllSpritesOfType<Entity>();
                _goal = GetGoal(_scene.Sprites);
            }
            if (Input.Get("L").Pressed)
                return false;
        } while (!_goal.EndGame);
        passedSeconds = _clock.PassedSeconds;
        repawns = _player.Respawns;
        return true;
    }
    private Goal GetGoal(SpriteManager sprites)
    {
        List<Goal> goal = sprites.GetAllSpritesOfType<Goal>();
        if (goal.Count != 1)
            throw new ArgumentException("Invalid amount of goals");
        goal[0].Keys = sprites.GetAllSpritesOfType<GameKey>();
        return goal[0];
    }
    private void UpdateSideBar()
    {
        _sideBar[0].UpdateContent([["!SPIELINFO"], [$"Level: {_account.CurrentLevel+1}", $"Schlüssel: {_goal.CollectedKeysAmount}/{_goal.KeysAmount}"]]);
        _sideBar[1].UpdateContent([["!SPIELER"], [$"Name: {_account.Name}", $"Leben: {_player.Health}/{Player.MaxHealth}"]]);
        _sideBar[2].UpdateContent([["!TASTENINFO"], [$"[{'P'}] - Pause", $"[{'L'}] - Verlassen"]]);
        _sideBar[3].UpdateContent([[$"XS: {_player.VelocityX}", $"YS: {_player.VelocityY}", $"DT: {_scene.Sprites.DeltaTime}"]]);
    }
}