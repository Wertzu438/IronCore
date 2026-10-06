namespace MCR_Engine.Core;

public struct RGB
{
    public byte Red;
    public byte Green;
    public byte Blue;

    internal RGB(byte red, byte green, byte blue)
    {
        Red = red;
        Green = green;
        Blue = blue;
    }
    public override int GetHashCode() => HashCode.Combine(Red, Green, Blue);
    public static bool SameValue(RGB? rgb1, RGB? rgb2) => rgb1.GetHashCode() == rgb2.GetHashCode();
}