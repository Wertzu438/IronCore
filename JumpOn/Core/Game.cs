using System.Diagnostics;
using JumpOn.CheckPoints;
using JumpOn.Collectables;
using JumpOn.Entities;
using JumpOn.Entities.LivingEntities;
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
    private const string PauseKeyName = "P", LeaveKeyName = "L", RespawnKeyName = "R";
    private readonly Engine _engine;
    private readonly Scene _scene;
    private readonly Account _account;
    private readonly Clock _clock;
    private readonly UIElement[] _sideBar;
    private readonly Player _player;
    private readonly CenteredInfoBox _centeredInfoBox;
    private readonly MobileInfoBox _mobileInfoBox;
    private readonly UIElement[] _otherInfoBoxes;
    private readonly PathFinding _pathFinding;
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
        _centeredInfoBox = (CenteredInfoBox)UI.Add(new CenteredInfoBox(Input));
        _mobileInfoBox = (MobileInfoBox)UI.Add(new MobileInfoBox());
        _otherInfoBoxes = [
            UI.Add(new UIElement(0, 0, 0, 0, true)),
        ];
        _sideBar = [
            UI.Add(new UIElement(20, 6, 2, 1, true)), 
            UI.Add(new UIElement(20, 6, 2, 8, true)), 
            UI.Add(new UIElement(20, 7, 2, 15, true))
        ];
        _clock = new Clock();
        _engine.SetRenderScene(_scene = SceneLoader.Read($"level{level}", _engine, _mobileInfoBox, _clock));
        _player = (Player)_scene.Objects.Add(new Player(0, 0, Textures, Input, _clock));
        _pathFinding = new PathFinding(_scene);
        LoadContent();
        Camera.SetFollowedObject(_player, 10, 10);
    }
    private Input Input => _engine.Input;
    private UIManager UI => _engine.UI;
    private Textures Textures => _engine.Textures;
    private Camera Camera => _engine.Camera;
    public bool Play(out int? passedSeconds, out PlayerData? gameData)
    {
        passedSeconds = null;
        gameData = null;
        Stopwatch stopwatch = Stopwatch.StartNew();
        do
        {
            float deltaTime = (float)stopwatch.ElapsedTicks / Stopwatch.Frequency;
            _clock.Update(deltaTime);
            stopwatch.Restart();
            Trap.Update(_traps);
            Interactable.Update(_interactables, _entities);
            Collectable.Update(_collectables);
            Entity.Update(_entities, _scene);
            _scene.Update(deltaTime);
            UpdateSideBar();
            bool respawnPressed = Input.Get(RespawnKeyName).Pressed;
            if (!_player.Alive || respawnPressed)
            {
                _scene.Update(0.2f, true);
                if (!respawnPressed)
                    _checkPoints.ShowInfoBox();
                _scene.SetSpriteSnapshot(_checkPoints.SnapShot);
                _scene.Objects.Add(_player);
                LoadContent();
                _scene.Update(0, true);
                if (respawnPressed)
                    Thread.Sleep(400);
            }
            else if (Input.Get(LeaveKeyName).Pressed)
            {
                return false;
            }
            else if (Input.Get(PauseKeyName).Pressed)
            {
                _centeredInfoBox.Show(24, 5, [["!⏸️ PAUSIERT"], [$"-Fortfahren mit {CenteredInfoBox.ConfirmKeyName}"]], true);
                stopwatch.Restart();
            }
        } while (!_goal.EndGame);
        passedSeconds = _clock.PassedSeconds;
        gameData = _player.GameData;
        return true;
    }
    private void LoadContent()
    {
        _mobileInfoBox.Visible = false;
        _interactables = Interactable.LoadInteractables(_scene.Objects);
        _collectables = Collectable.LoadCollectables(_scene.Objects);
        _traps = Trap.LoadTraps(_scene.Objects);
        _entities = Entity.LoadEntities(_scene.Objects);
        _goal = Goal.LoadGoal(_scene.Objects);
        _checkPoints = new CheckPointManager(_scene, _clock, _centeredInfoBox, _otherInfoBoxes[0]);
        _checkPoints.ResetPlayerToCheckPoint(_player);
    }
    private void UpdateSideBar()
    {
        _sideBar[0].UpdateContent([["!SPIELINFO"], [$"🎯 Level: {_account.CurrentLevel+1}", $"🔐 Schlüssel: {_goal.CollectedKeysAmount}/{_goal.KeysAmount} "]]);
        _sideBar[1].UpdateContent([["!SPIELER"], [$"👤 Name: {_account.Name}", $"❤️ Leben: {Math.Round(_player.Health, 2)}/{Player.MaxHealth}"]]);
        _sideBar[2].UpdateContent([["!TASTENINFO"], [$"[{PauseKeyName}] ⏸️ Pause", $"[{RespawnKeyName}] 🔄 Respawnen", $"[{LeaveKeyName}] 🏠 Verlassen"]]);
    }
}