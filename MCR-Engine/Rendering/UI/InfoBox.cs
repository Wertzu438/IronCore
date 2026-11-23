using MCR_Engine.Core;

namespace MCR_Engine.Rendering.UI;

public class InfoBox : UIElement
{
    private const int Border = 1;
    private const char FrameOutput = '▀', Separator = '▬';
    private const char CenterChar = '-', SpecialTextAndCenterChar = '!';
    private readonly RGB _textColor, _specialTextColor, _borderColor, _backgroundColor;
    private string[][] _content;
    public InfoBox(int width, int height, int x, int y, bool staticBox, bool visible = true, bool dontFlipX = true, bool dontFlipY = true) : 
        base(width, height, x, y, staticBox, visible, dontFlipX, dontFlipY)
    {
        if (width < 0 || height < 0)
            throw new ArgumentException("Width and height must be positive");
        Visible = visible;
        _specialTextColor = new RGB(255, 255, 150);
        _textColor = new RGB(255, 255, 255);
        _borderColor = new RGB(120, 120, 120);
        _backgroundColor = new RGB(0, 0, 0);
        _content = [];
    }
    public void UpdateContent(string[][] content) => _content = content;
    internal override void UpdateBuffer()
    {
        SetUIBorderInBuffer();
        SetContentInBuffer(_content);
    }
    protected override UIPixel[,] CreateBuffer(int width, int height)
    {
        var design = new UIPixel[width, height];
        for (int y = 0; y < height; y++)
        for (int x = 0; x < width; x++)
            design[x, y] = new UIPixel(" ", _textColor, _backgroundColor);
        return design;
    }
    protected override void ClearBuffer()
    {
        for (int y = 1; y < Height - 1; y++)
        for (int x = 1; x < Width - 1; x++)
            Buffer[x, y].SetOutput(" ", _textColor, _backgroundColor);
    }
    private void SetUIBorderInBuffer()
    {
        int width = Width, height = Height;
        for (int y = 0; y < height; y++)
        {
            Buffer[0, y].SetOutput(FrameOutput.ToString(), _borderColor, _borderColor);
            Buffer[width - 1, y].SetOutput(FrameOutput.ToString(), _borderColor, _borderColor);
            if (y != 0 && y != height - 1)
                continue;
            for (int x = 1; x < width - 1; x++)
            {
                Buffer[x, 0].SetOutput(FrameOutput.ToString(), _borderColor, _backgroundColor);
                Buffer[x, height - 1].SetOutput(FrameOutput.ToString(), _backgroundColor, _borderColor);
            }
        }
    }
    private void SetContentInBuffer(string[][] content)
    {
        ClearBuffer();
        int y = 1;
        for (int paragraphIndex = 0; paragraphIndex < content.Length; paragraphIndex++)
        {
            RGB textColor = _textColor;
            for (int lineIndex = 0; lineIndex < content[paragraphIndex].Length; lineIndex++)
            {
                if (y >= Height - 1)
                    return;
                string line = content[paragraphIndex][lineIndex];
                int x = 2;
                if (line.StartsWith(CenterChar))
                {
                    line = Center(line);
                }
                else if (line.StartsWith(SpecialTextAndCenterChar))
                {
                    line = Center(line);
                    textColor = _specialTextColor;
                }
                foreach (char letter in line)
                {
                    if (x - 2 >= Math.Min(line.Length, Width - 4))
                        break;
                    Buffer[x, y].SetOutput(letter.ToString(), textColor, _backgroundColor);
                    x++;
                }
                y++;
            }
            if (paragraphIndex >= content.Length - 1 || y >= Height - 1) 
                return;
            for (int x = 1; x < Width - 1; x++)
                Buffer[x, y].SetOutput(Separator.ToString(), _borderColor, _backgroundColor);
            y++;
        }
    }
    private string Center(string line)
    {
        line = line.Trim([CenterChar, SpecialTextAndCenterChar]);
        int buffer = Width - Border * 2 - line.Length;
        if (buffer < 0)
            return line;
        buffer /= 2;
        if (--buffer < 0)
            buffer = 0;
        return new string(' ', buffer) + line;
    }
}