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

    public override string ToString() => $"{Red},{Green},{Blue}";

    public static bool SameValue(RGB? rgb1, RGB? rgb2)
    {
        if (rgb1 == null || rgb2 == null)
            return (rgb1 == null && rgb2 == null);

        return (rgb1.Value.Red == rgb2.Value.Red && rgb1.Value.Green == rgb2.Value.Green && rgb1.Value.Blue == rgb2.Value.Blue);
    }
}