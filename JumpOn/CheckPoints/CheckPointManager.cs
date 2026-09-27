using JumpOn.Entities;
using JumpOn.UI.InfoBoxes;
using MCR_Engine.Objects;

namespace JumpOn.CheckPoints;

public class CheckPointManager
{
    private readonly List<CheckPoint> _checkPoints;
    private readonly CenteredInfoBox _infoBox;
    public CheckPointManager(SpriteManager sprites, CenteredInfoBox infoBox)
    {
        _checkPoints = sprites.GetAllSpritesOfType<CheckPoint>();
        _infoBox = infoBox;
    }
    public (int x, int y) GetPlayerSpawnPosition()
    {
        foreach (var checkPoint in _checkPoints)
        {
            if (checkPoint.IsCollected)
                return (checkPoint.X, checkPoint.Y);
        }
        throw new ArgumentException("No player spawn point found");
    }
    public void ResetPlayerToChekPoint(Player player)
    {
        ShowInfoBox();
        CheckPoint nearestCheckPoint = GetNearestUnlockedCheckPoint(player.X, player.Y);
        player.X = nearestCheckPoint.X;
        player.Y = nearestCheckPoint.Y;
        player.Heal(double.MaxValue);
    }
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