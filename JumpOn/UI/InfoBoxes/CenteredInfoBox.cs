using MCR_Engine.InputSystem;
using MCR_Engine.Rendering.UI;

namespace JumpOn.UI.InfoBoxes;

public class CenteredInfoBox : InfoBox
{
    public const string ConfirmKeyName = "Enter";
    private readonly Key _confirmKey;
    public CenteredInfoBox(Input input) : base(10, 4, 0, 0, true, false)
    {
        _confirmKey = input.Get(ConfirmKeyName);
    }
    private int CenterX => Console.WindowWidth / 2 - Width / 2;
    private int CenterY => Console.WindowHeight / 2 - Height / 2;
    public void Show(int width, int height, string[][] content, bool confirmKey)
    {
        UpdateSize(width, height);
        UpdateContent(content);
        X = CenterX; Y = CenterY;
        if (!confirmKey) return;
        Visible = true;
        WaitForConfirm();
        Visible = false;
    }
    private void WaitForConfirm()
    {
        while (!_confirmKey.Pressed)
            Thread.Sleep(50);
    }
}