using MCR_Engine.Core;

namespace MCR_Engine.Rendering.UI;

public class UIElement
    {
        private const int Border = 1;
        private const char FrameOutput = '▀', Separator = '▬';
        private const char CenterChar = '-', SpecialTextAndCenterChar = '!';
        private readonly RGB _textColor, _specialTextColor, _borderColor, _backgroundColor;
        private readonly bool _dontFlipX;
        private readonly bool _dontFlipY;
        private readonly bool _staticBox;
        private string[][] _content;
        private UIPixel[,] _buffer;
        private UIManager? _ui;
        private int _x, _y;
        public UIElement(int width, int height, int x, int y, bool staticBox, bool visible = true, bool dontFlipX = true, bool dontFlipY = true)
        {
            if (width < 0 || height < 0)
                throw new ArgumentException("Width and height must be positive");
            _staticBox = staticBox;
            Visible = visible;
            X = x; Y = y;
            _dontFlipX = dontFlipX; _dontFlipY = dontFlipY;
            _specialTextColor = new RGB(255, 255, 150);
            _textColor = new RGB(255, 255, 255);
            _borderColor = new RGB(120, 120, 120);
            _backgroundColor = new RGB(0, 0, 0);
            _buffer = CreateBuffer(width, height);
            _content = [];
        }
        public int X
        {
            get => _staticBox ? (_dontFlipX ? _x : UI.UIMapWidth - _x - Width) : _x - UI.LastXOffset;
            set => _x = (value < 0 ? 0 : value);
        }
        public int Y
        {
            get => _staticBox ? (_dontFlipY ? _y : UI.UIMapHeight - _y - Height) : _y - UI.LastYOffset / 2;
            set => _y = (value < 0 ? 0 : value);
        }
        public int Width => _buffer.GetLength(0);
        public int Height => _buffer.GetLength(1);
        public bool Visible { get; set; }
        private UIManager UI => _ui ?? throw new NullReferenceException("UI element has not been initialized");
        public void UpdateSize(int width, int height)
        {
            if (width < 0 || height < 0)
                throw new ArgumentException("Width and height must be positive");
            _buffer = CreateBuffer(width, height);
        }
        public UIPixel GetUIPixel(int x, int y)
        {
            if (!OnDesign(x, y))
                throw new ArgumentException("Coordinates are out of bounds");
            return _buffer[x, y];
        }
        public void UpdateContent(string[][] content) => _content = content;
        public void SetUIManager(UIManager ui) => _ui = ui;
        internal void UpdateBuffer()
        {
            SetUIBorderInBuffer();
            SetContentInBuffer(_content);
        }
        private void SetUIBorderInBuffer()
        {
            int width = Width, height = Height;
            for (int y = 0; y < height; y++)
            {
                _buffer[0, y].SetOutput(FrameOutput.ToString(), _borderColor, _borderColor);
                _buffer[width - 1, y].SetOutput(FrameOutput.ToString(), _borderColor, _borderColor);
                if (y != 0 && y != height - 1)
                    continue;
                for (int x = 1; x < width - 1; x++)
                {
                    _buffer[x, 0].SetOutput(FrameOutput.ToString(), _borderColor, _backgroundColor);
                    _buffer[x, height - 1].SetOutput(FrameOutput.ToString(), _backgroundColor, _borderColor);
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
                        _buffer[x, y].SetOutput(letter.ToString(), textColor, _backgroundColor);
                        x++;
                    }
                    y++;
                }
                if (paragraphIndex >= content.Length - 1 || y >= Height - 1) 
                    return;
                for (int x = 1; x < Width - 1; x++)
                    _buffer[x, y].SetOutput(Separator.ToString(), _borderColor, _backgroundColor);
                y++;
            }
        }
        private UIPixel[,] CreateBuffer(int width, int height)
        {
            var design = new UIPixel[width, height];
            for (int y = 0; y < height; y++)
            for (int x = 0; x < width; x++)
                design[x, y] = new UIPixel(" ", _textColor, _backgroundColor);
            return design;
        }
        private void ClearBuffer()
        {
            for (int y = 1; y < Height - 1; y++)
            for (int x = 1; x < Width - 1; x++)
                _buffer[x, y].SetOutput(" ", _textColor, _backgroundColor);
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
        private bool OnDesign(int x, int y) => x >= 0 && x < Width && y >= 0 && y < Height;
    }