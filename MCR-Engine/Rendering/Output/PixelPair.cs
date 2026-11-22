using MCR_Engine.Core;

namespace MCR_Engine.Rendering.Output;

internal class PixelPair //TODO: Löschen und das als UI-Pixel verwenden
{
    public const char Output = '▀';
    private readonly Pixel _upperPixel;
    private readonly Pixel _lowerPixel;
    public PixelPair(Pixel upperPixel, Pixel lowerPixel)
    {
        _upperPixel = upperPixel;
        _lowerPixel = lowerPixel;
    }
    public RGB UpperRGB => _upperPixel.GetRGB();
    public RGB LowerRGB => _lowerPixel.GetRGB();
}