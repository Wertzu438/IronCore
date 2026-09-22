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
    public void SetNewRGB(byte red, byte green, byte blue)
    {
        if (red == _rgb.Red && green == _rgb.Green && blue == _rgb.Blue)
            return;
        _rgb.Red = red;
        _rgb.Green = green;
        _rgb.Blue = blue;
    }
    public void SetNewRGB(RGB newRGB)
    {
        if (RGB.SameValue(_rgb, newRGB))
            return;
        _rgb = newRGB;
    }
}