namespace MCR_Engine.Core;

public class Textures
{
    private readonly Dictionary<string, RGB?[,]> _textures;
    public Textures()
    {
        _textures = [];
    }
    public int TextureCount => _textures.Count;
    public void AddTexture(string textureName, RGB?[,] texture) => _textures.Add(textureName, texture);
    public RGB?[,] Get(string textureName) => _textures.TryGetValue(textureName, out RGB?[,] texture) ? texture : throw new KeyNotFoundException($"Texture {textureName} was not found");
    public int GetTextureWidth(string textureName) => Get(textureName).GetLength(0);
    public int GetTextureHeight(string textureName) => Get(textureName).GetLength(1);
    public static RGB[,] ConvertNullableTexture(RGB?[,] texture)
    {
        int width = texture.GetLength(0), height = texture.GetLength(1);
        RGB[,] newTexture = new RGB[width, height];
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                newTexture[x, y] = (texture[x, y].HasValue ? texture[x, y]!.Value : throw new ArgumentException("Pixel was null"));
            }
        }
        return newTexture;
    }
    public static T[,] MirrorLeftRight<T>(T[,] array)
    {
        int width = array.GetLength(0);
        int height = array.GetLength(1);
        T[,] texture = new T[width, height];
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                texture[width - 1 - x, y] = array[x, y];
            }
        }
        return texture;
    }
    public static RGB?[,] DebuggingTexture(int width, int height)
    {
        RGB?[,] texture = new RGB?[width, height];
        for (int y = 0; y < height; y++)
            for (int x = 0; x < width; x++)
                texture[x, y] = ((x % 2 == 0 && y % 2 == 0) || (x % 2 != 0 && y % 2 != 0)) ? new RGB(0, 0, 0) : new RGB(255, 255, 255);
        return texture;
    }
    public static RGB?[,] CreateExpansion(RGB?[,] texture, Direction direction, int amount)
    {
        if (direction is Direction.Left or Direction.Up or Direction.None)
            throw new InvalidOperationException($"{direction} is not supported");
        int width = texture.GetLength(0) * (direction == Direction.Right ? amount : 1), height = texture.GetLength(1) * (direction == Direction.Down ? amount : 1);
        RGB?[,] newTexture = new RGB?[width, height];
        for (int i = 0; i < amount; i++)
            for (int y = 0; y < height; y++)
                for (int x = 0; x < width; x++)
                    newTexture[x, y] = texture[x % texture.GetLength(0), y % texture.GetLength(1)];
        return newTexture;
    }
}