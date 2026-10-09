namespace MCR_Engine.Core;

public struct RGB : IEquatable<RGB>
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
    public bool Equals(RGB other) => GetHashCode() == other.GetHashCode();
    public override bool Equals(object? obj)
    {
        return obj is RGB other && Equals(other);
    }
}