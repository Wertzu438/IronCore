using MCR_Engine.Core;

namespace MCR_Engine.Rendering.Output;

internal class PixelPair
{
    private const char BasicOutput = '▀';
    private readonly Pixel _upperPixel;
    private readonly Pixel _lowerPixel;
    private (RGB Up, RGB Down, char Output)? _uiPair;
    public PixelPair(Pixel upperPixel, Pixel lowerPixel)
    {
        _upperPixel = upperPixel;
        _lowerPixel = lowerPixel;
        _uiPair = null;
    }
    public char Output => IsUI ? _uiPair!.Value.Output : BasicOutput;
    public RGB UpperRGB => IsUI ? _uiPair!.Value.Up : _upperPixel.GetRGB();
    public RGB LowerRGB => IsUI ? _uiPair!.Value.Down : _lowerPixel.GetRGB();
    private bool IsUI => _uiPair is not null;
    public void SetUIPair(RGB upper, RGB lower, char newOutput) => _uiPair = (upper, lower, newOutput);
    public void ResetUIPair() => _uiPair = null;
}