using System.Diagnostics;
using JumpOn.CheckPoints;
using JumpOn.Collectables;
using JumpOn.Entities;
using JumpOn.Interactables;
using JumpOn.Traps;
using JumpOn.UI.InfoBoxes;
using MCR_Engine.Core;
using MCR_Engine.InputSystem;
using MCR_Engine.Rendering.Output;
using MCR_Engine.Rendering.UI;

namespace JumpOn.Core;

public class Game
{
    private readonly Engine _engine;
    private readonly Scene _scene;
    private readonly Account _account;
    private readonly Clock _clock;
    private readonly UIElement[] _sideBar;
    private readonly Player _player;
    private readonly PauseChecker _pause;
    private readonly CenteredInfoBox _centeredInfoBox;
    private readonly MobileInfoBox _mobileInfoBox;
    private readonly UIElement[] _otherInfoBoxes;
    private List<Interactable> _interactables;
    private List<Trap> _traps;
    private List<Collectable> _collectables;
    private List<Entity> _entities;
    private CheckPointManager _checkPoints;
    private Goal _goal;
    public Game(Engine engine, Account account, int level)
    {
        _engine = engine;
        _account = account;
        _otherInfoBoxes = [UI.Add(new UIElement(0, 0, 0, 0, true))];
        _centeredInfoBox = (CenteredInfoBox)UI.Add(new CenteredInfoBox(Input));
        _mobileInfoBox = (MobileInfoBox)UI.Add(new MobileInfoBox());
        _clock = new Clock(); _pause = new PauseChecker(Input, _centeredInfoBox);
        _engine.SetRenderScene(_scene = LevelLoader.Read($"level{level}", _engine, _mobileInfoBox, _clock));
        _player = (Player)_scene.Sprites.Add(new Player(0, 0, Textures, Input, _clock));
        LoadContent();
        _sideBar = [UI.Add(new UIElement(20, 6, 2, 1, true)), UI.Add(new UIElement(20, 6, 2, 8, true)), UI.Add(new UIElement(20, 7, 2, 15, true))];
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
            double deltaTime = Math.Min(stopwatch.Elapsed.TotalSeconds, 1);
            _clock.Update((int)(deltaTime * 1000));
            stopwatch.Restart();
            Trap.Update(_traps);
            Interactable.Update(_interactables);
            Collectable.Update(_collectables);
            Entity.Update(_entities, _scene);
            _scene.Update(deltaTime);
            UpdateSideBar();
            bool respawnPressed = Input.Get("R").Pressed;
            if (!_player.Alive || respawnPressed)
            {
                _scene.Update(0.2, true);
                if (!respawnPressed)
                    _checkPoints.ShowInfoBox();
                _scene.SetSpriteSnapshot(_checkPoints.SnapShot);
                _scene.Sprites.Add(_player);
                LoadContent();
                _scene.Update(0, true);
            }
            else if (Input.Get("L").Pressed)
            {
                return false;
            }
            else _pause.Check();
        } while (!_goal.EndGame);
        passedSeconds = _clock.PassedSeconds;
        repawns = _player.Respawns;
        return true;
    }
    private void LoadContent()
    {
        _mobileInfoBox.Visible = false;
        _interactables = Interactable.LoadInteractables(_scene.Sprites);
        _collectables = Collectable.LoadCollectables(_scene.Sprites);
        _traps = Trap.LoadTraps(_scene.Sprites);
        _entities = Entity.LoadEntities(_scene.Sprites);
        _goal = Goal.LoadGoal(_scene.Sprites);
        _checkPoints = new CheckPointManager(_engine, _scene, _clock, _centeredInfoBox, _otherInfoBoxes[0]);
        _checkPoints.ResetPlayerToCheckPoint(_player);
    }
    private void UpdateSideBar()
    {
        _sideBar[0].UpdateContent([["!SPIELINFO"], [$"🎯 Level: {_account.CurrentLevel+1}", $"🔐 Schlüssel: {_goal.CollectedKeysAmount}/{_goal.KeysAmount} "]]);
        _sideBar[1].UpdateContent([["!SPIELER"], [$"👤 Name: {_account.Name}", $"❤️ Leben: {Math.Round(_player.Health, 2)}/{Player.MaxHealth}"]]);
        _sideBar[2].UpdateContent([["!TASTENINFO"], [$"[{'P'}] ⏸️ Pause", $"[{'R'}] 🔄 Respawnen", $"[{'L'}] 🏠 Verlassen"]]);
    }
}