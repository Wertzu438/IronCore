using MCR_Engine.Core;

namespace MCR_Engine.Rendering.Output;

internal class Pixel
{
    private RGB _rgb;
    public Pixel(RGB rgb)
    {
        _rgb = rgb;
    }
    public RGB GetRGB() => _rgb;
    public void SetNewRGB(RGB newRGB) => _rgb = newRGB;
}