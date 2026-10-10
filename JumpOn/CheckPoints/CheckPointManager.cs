using JumpOn.Entities;
using JumpOn.UI.InfoBoxes;
using MCR_Engine.Core;
using MCR_Engine.Objects;
using MCR_Engine.Rendering.Output;

namespace JumpOn.CheckPoints;

public class CheckPointManager
{
    private readonly List<CheckPoint> _checkPoints;
    private readonly CenteredInfoBox _infoBox;
    private readonly Engine _engine;
    private readonly Scene _scene;
    private SpriteManager _checkPointSnapShot;
    public CheckPointManager(Engine engine, Scene scene, CenteredInfoBox infoBox)
    {
        _engine = engine;
        _scene = scene;
        _checkPoints = _scene.Sprites.GetAllSpritesOfType<CheckPoint>();
        foreach (CheckPoint checkPoint in _checkPoints)
            checkPoint.CheckPoints = this;
        _checkPointSnapShot = _scene.Sprites.Clone(_engine);
        _infoBox = infoBox;
    }
    public SpriteManager SnapShot => _checkPointSnapShot;
    public void GetPlayerSpawnPosition(out int x, out int y)
    {
        if (_checkPoints.Count < 1)
            throw new ArgumentException("No player spawn point found");
        _checkPoints[0].IsCollected = true;
        x = _checkPoints[0].X;
        y = _checkPoints[0].Y;
    }
    public void ResetPlayerToCheckPoint(Player player)
    {
        ShowInfoBox();
        CheckPoint nearestCheckPoint = GetNearestUnlockedCheckPoint(player.X, player.Y);
        player.StopMovingX(); player.StopMovingY();
        player.X = nearestCheckPoint.X;
        player.Y = nearestCheckPoint.Y - 7;
        player.Heal(double.MaxValue);
        player.Respawns++;
    }
    public void MakeSnapshot() => _checkPointSnapShot = _scene.Sprites.Clone(_engine);
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
    private void ShowInfoBox() => _infoBox.Show(21, 5, [["!RESPAWNEN"], [$"-Respawnen mit {CenteredInfoBox.ConfirmKeyName}"]], true);
}