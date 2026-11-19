using JumpOn.Collectables;
using JumpOn.Entities;
using JumpOn.Entities.LivingEntities;
using JumpOn.UI.InfoBoxes;
using MCR_Engine.Core;
using MCR_Engine.ObjectSystem;
using MCR_Engine.Rendering.Output;
using MCR_Engine.Rendering.UI;

namespace JumpOn.CheckPoints;

public class CheckPointManager
{
    private const int SavingOffsetSeconds = 3;
    private readonly CheckPoint[] _checkPoints;
    private readonly CenteredInfoBox _centeredInfoBox;
    private readonly UIElement _infoBox;
    private readonly Scene _scene;
    private readonly Clock _clock;
    private int _lastSecondSaved;
    private int _lastCheckPointIndex;
    private Objects _checkPointSnapShot;
    public CheckPointManager(Scene scene, Clock clock, CenteredInfoBox centeredInfoBox, UIElement infoBox)
    {
        _scene = scene;
        _infoBox = infoBox;
        _checkPoints = _scene.Objects.GetAllObjectsOfType<CheckPoint>();
        foreach (CheckPoint checkPoint in _checkPoints)
            checkPoint.CheckPoints = this;
        _centeredInfoBox = centeredInfoBox;
        _clock = clock;
        _lastSecondSaved = int.MinValue;
        _infoBox.Visible = false;
        _lastCheckPointIndex = 0;
        _checkPointSnapShot = _scene.Objects.Clone();
    }
    public CheckPointManager(CheckPointManager manager)
    {
        _scene = manager._scene;
        _checkPoints = _scene.Objects.GetAllObjectsOfType<CheckPoint>();
        _infoBox = manager._infoBox;
        foreach (CheckPoint checkPoint in _checkPoints)
            checkPoint.CheckPoints = this;
        _centeredInfoBox = manager._centeredInfoBox;
        _clock = manager._clock;
        _lastSecondSaved = int.MinValue;
        _infoBox.Visible = false;
        _checkPointSnapShot = _scene.Objects.Clone();
        _lastCheckPointIndex = manager._lastCheckPointIndex;
    }
    public Objects SnapShot => _checkPointSnapShot;
    public void ResetPlayerToCheckPoint(Player player, bool firstLoad)
    {
        if (_checkPoints.Length <= 0)
            throw new ArgumentException("No player spawn points found");
        player.StopMovingX(); player.StopMovingY();
        player.X = _checkPoints[_lastCheckPointIndex].X;
        player.Y = _checkPoints[_lastCheckPointIndex].Y;
        player.SetLastFloorY(player.Y);
        player.Heal(double.MaxValue);
        if (firstLoad)
            return;
        player.AddRespawn();
    }
    public void ShowRespawnInfo() => _centeredInfoBox.Show(21, 5, [["!RESPAWNEN"], [$"-Respawnen mit {CenteredInfoBox.ConfirmKeyName}"]], true);
    private async Task ShowSavedInfo()
    {
        if (_lastSecondSaved + SavingOffsetSeconds > _clock.PassedSeconds)
            return;
        _infoBox.UpdateSize(25, 3);
        _infoBox.X = 2;
        _infoBox.Y = Console.WindowHeight - 2 - _infoBox.Height;
        _infoBox.UpdateContent([[$"-💾 Gespeichert: {DateTime.Now.ToShortTimeString()}"]]);
        _infoBox.Visible = true;
        await Task.Delay(SavingOffsetSeconds * 1000);
        _infoBox.Visible = false;
    }
    public async Task MakeSnapshot(CheckPoint checkPoint)
    {
        if (_lastSecondSaved + SavingOffsetSeconds > _clock.PassedSeconds)
            return;
        for (int i = 0; i < _checkPoints.Length; i++)
        {
            if (_checkPoints[i] != checkPoint)
                continue;
            _lastCheckPointIndex = i;
        }
        _checkPointSnapShot = _scene.Objects.Clone();
        ShowSavedInfo();
        _lastSecondSaved = _clock.PassedSeconds;
    }
}