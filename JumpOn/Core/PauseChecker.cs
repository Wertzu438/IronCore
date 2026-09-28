using JumpOn.UI.InfoBoxes;
using MCR_Engine.InputSystem;

namespace JumpOn.Core;

public class PauseChecker
{
    private const string KeyName = "P";
    private readonly CenteredInfoBox _infoBox;
    private readonly Key _pauseKey;

    public PauseChecker(Input input, CenteredInfoBox infoBox)
    {
        _infoBox = infoBox;
        _pauseKey = input.Get("P");
    }

    public void Check()
    {
        if (!_pauseKey.Pressed)
            return;
        _infoBox.Show(24, 5, [["!PAUSIERT"], [$"-Fortfahren mit {CenteredInfoBox.ConfirmKeyName}"]], true);
    }
}