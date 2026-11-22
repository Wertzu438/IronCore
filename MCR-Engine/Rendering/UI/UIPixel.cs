using MCR_Engine.Core;

namespace MCR_Engine.Rendering.UI;

public class UIPixel
{
    public UIPixel(string output, RGB foregroundColor, RGB backgroundColor)
    {
        Output = output ?? throw new ArgumentNullException(nameof(output));
        ForegroundColor = foregroundColor;
        BackgroundColor = backgroundColor;
    }
    public string Output { get; private set; }
    public RGB ForegroundColor { get; private set; }
    public RGB BackgroundColor { get; private set; }
    public void SetOutput(string output, RGB foregroundColor, RGB backgroundColor)
    {
        Output = output ?? throw new ArgumentNullException(nameof(output));
        ForegroundColor = foregroundColor;
        BackgroundColor = backgroundColor;
    }
}