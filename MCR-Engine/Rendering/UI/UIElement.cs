namespace MCR_Engine.Rendering.UI;

public class UIElement
    {
        public const char Separator = '▬';
        private const char CenterChar = '-', SpecialTextAndCenterChar = '!';
        public const int Border = 1;
        private readonly bool _staticBox;
        private readonly bool _dontFlipX;
        private int _width, _height;
        private string[][] _content;
        private int _x, _y;
        private bool _visible;
        public UIElement(int width, int height, int x, int y, bool staticBox, bool visible = true, bool dontFlipX = false)
        {
            _content = [];
            _staticBox = staticBox;
            _visible = visible;
            _dontFlipX = dontFlipX;
            Width = width;
            Height = height;
            X = x;
            Y = y;
        }
        public bool StaticBox => _staticBox;
        public int X
        {
            get => (StaticBox && !_dontFlipX ? Console.WindowWidth - _width - _x : _x);
            set => _x = (value < 0 ? 0 : value);
        }
        public int Y
        {
            get => _y;
            set => _y = (value < 0 ? 0 : value);
        }
        public int Width
        {
            get => _width;
            private set => _width = value > Border * 2 ? value : Border * 2 + 1;
        }
        public int Height
        {
            get => _height;
            private set => _height = value > Border * 2 ? value : Border * 2 + 1;
        }
        public bool Visible
        {
            get => _visible;
            set => _visible = value;
        }
        public void UpdateContent(string[][]  content) => _content = content;
        public void UpdateSize(int width, int height)
        {
            _width = width;
            _height = height;
        }
        public string GetLine(int lineIndex, out bool specialColor)
        {
            int currentLine = 0;
            specialColor = false;
            foreach (string[] paragraph in _content)
            {
                foreach (string line in paragraph)
                {
                    if (currentLine++ != lineIndex)
                        continue;
                    switch (line[0])
                    {
                        case CenterChar: return Center(line);
                        case SpecialTextAndCenterChar: specialColor = true; return Center(line);
                        default: return line;
                    }
                }
                if (currentLine == lineIndex && !ReferenceEquals(paragraph, _content[^1]))
                    return new string(Separator, _width - Border * 2);
                currentLine++;
            }
            return new string(' ', _width - Border * 2);
        }
        private string Center(string line)
        {
            line = line.Trim([CenterChar, SpecialTextAndCenterChar]);
            int buffer = _width - Border * 2 - line.Length;
            if (buffer < 0)
                return line;
            buffer /= 2;
            return new string(' ', buffer) + line;
        }
    }