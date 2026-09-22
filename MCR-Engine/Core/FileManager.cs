using MCR_Engine.InputSystem;

namespace MCR_Engine.Core;

internal static class  FileManager
{
    private const string TexturesFilePath = "Textures";
    private const string InputFilePath = "Input";
    public static void LoadTextures(Textures textures)
    {
        string[] filePaths = Directory.GetFiles(TexturesFilePath);
        foreach (string filePath in filePaths)
        {
            textures.AddTexture(Path.GetFileNameWithoutExtension(filePath), ReadTexture(filePath));
        }
    }
    public static void LoadKeys(Input input)
    {
        string[] filePaths = Directory.GetFiles(InputFilePath);
        foreach (string filePath in filePaths)
        {
            ReadKeyFile(filePath, input);
        }
    }
    private static RGB?[,] ReadTexture(string filePath)
    {
        string[] lines = Read(filePath);
        string[] parts = lines[0].Split(';');
        if (parts.Length != 2 || !int.TryParse(parts[0], out int width) || !int.TryParse(parts[1], out int height))
            throw new ArgumentException("Invalid file format");
        RGB?[,] texture = new RGB?[width, height];
        for (int y = 0; y < height; y++)
        {
            int x = 0;
            parts = lines[y+1].Split(";");
            if (parts.Length != width)
                throw new ArgumentException("Invalid file format");
            foreach (string rgb in parts)
            {
                string[] rgbValues = rgb.Split(',');
                if (rgbValues.Length == 3 && byte.TryParse(rgbValues[0], out byte red) && byte.TryParse(rgbValues[1], out byte green) && byte.TryParse(rgbValues[2], out byte blue))
                    texture[x++, y] = new RGB(red, green, blue);
                else texture[x++, y] = null;
            }
        }
        return texture;
    }
    private static void ReadKeyFile(string filePath, Input input)
    {
        string[] lines = Read(filePath);
        for (int i = 0; i < lines.Length; i++)
        {
            string[] parts = lines[i].Split(',');
            if (parts.Length != 2 || !int.TryParse(parts[1], System.Globalization.NumberStyles.HexNumber, null, out int keyCode))
                throw new ArgumentException("Wrong format");
            input.AddKey(parts[0], keyCode);
        }
    }
    private static string[] Read(string path) => !File.Exists(path) ? throw new FileNotFoundException(path) : File.ReadAllLines(path);
}