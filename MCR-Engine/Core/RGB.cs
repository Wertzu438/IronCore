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
    public override bool Equals(object? obj)
    {
        return obj is RGB other && Equals(other);
    }
    public bool Equals(RGB other) => Red == other.Red && Green == other.Green && Blue == other.Blue;
    public static bool operator ==(RGB a, RGB b) => a.Equals(b);
    public static bool operator !=(RGB a, RGB b) => !a.Equals(b);
}