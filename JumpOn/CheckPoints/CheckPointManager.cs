using JumpOn.Collectables;
using JumpOn.Entities;
using JumpOn.UI.InfoBoxes;
using MCR_Engine.Core;
using MCR_Engine.Objects;
using MCR_Engine.Rendering.Output;
using MCR_Engine.Rendering.UI;

namespace JumpOn.CheckPoints;

public class CheckPointManager
{
    private const int SavingOffsetSeconds = 3;
    private readonly List<CheckPoint> _checkPoints;
    private readonly CenteredInfoBox _centeredInfoBox;
    private readonly UIElement _infoBox;
    private readonly Engine _engine;
    private readonly Scene _scene;
    private readonly Clock _clock;
    private int _lastSecondSaved;
    private SpriteManager _checkPointSnapShot;
    public CheckPointManager(Engine engine, Scene scene, Clock clock, CenteredInfoBox centeredInfoBox, UIElement infoBox)
    {
        _engine = engine;
        _scene = scene;
        _infoBox = infoBox;
        _checkPoints = _scene.Sprites.GetAllSpritesOfType<CheckPoint>();
        foreach (CheckPoint checkPoint in _checkPoints)
            checkPoint.CheckPoints = this;
        _checkPointSnapShot = _scene.Sprites.Clone(_engine);
        _centeredInfoBox = centeredInfoBox;
        _clock = clock;
        _lastSecondSaved = int.MinValue;
    }
    public SpriteManager SnapShot => _checkPointSnapShot;
    private bool OneCollected => Collectable.CollectedAmount(_checkPoints) != 0;
    public void ResetPlayerToCheckPoint(Player player)
    {
        if (_checkPoints.Count <= 0)
            throw new ArgumentException("No player spawn points found");
        if (OneCollected)
        {
            player.AddRespawn();
        }
        else _checkPoints[0].IsCollected = true;
        CheckPoint nearestCheckPoint = GetNearestUnlockedCheckPoint(player.X, player.Y);
        player.StopMovingX(); player.StopMovingY();
        player.X = nearestCheckPoint.X;
        player.Y = nearestCheckPoint.Y;
        player.Heal(double.MaxValue);
    }
    public void ShowInfoBox() => _centeredInfoBox.Show(21, 5, [["!RESPAWNEN"], [$"-Respawnen mit {CenteredInfoBox.ConfirmKeyName}"]], true);
    private CheckPoint GetNearestUnlockedCheckPoint(int x, int y)
    {
        CheckPoint? nearestCheckPoint = null;
        int distance = int.MaxValue;
        foreach (var checkPoint in _checkPoints)
        {
            if (!checkPoint.IsCollected)
                continue;
            int currentDistance = Math.Abs(checkPoint.X - x) + Math.Abs(checkPoint.Y - y) * 3;
            if (currentDistance < distance)
            {
                nearestCheckPoint = checkPoint;
                distance = currentDistance;
            }
        }
        if (nearestCheckPoint == null)
            throw new ArgumentException("No player spawn point found");
        return nearestCheckPoint;
    }
    public async Task MakeSnapshot()
    {
        if (_lastSecondSaved + SavingOffsetSeconds > _clock.PassedSeconds)
            return;
        _checkPointSnapShot = _scene.Sprites.Clone(_engine);
        _lastSecondSaved = _clock.PassedSeconds;
        _infoBox.UpdateSize(25, 3);
        _infoBox.X = 2;
        _infoBox.Y = Console.WindowHeight - 2 - _infoBox.Height;
        _infoBox.UpdateContent([[$"-💾 Gespeichert: {DateTime.Now.ToShortTimeString()}"]]);
        _infoBox.Visible = true;
        await Task.Delay(SavingOffsetSeconds * 1000);
        _infoBox.Visible = false;
    }
}