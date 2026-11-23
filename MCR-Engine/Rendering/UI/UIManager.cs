using MCR_Engine.Core;
using MCR_Engine.Rendering.Output;

namespace MCR_Engine.Rendering.UI;

public class UIManager
    {
        private readonly Renderer _render;
        private List<UIElement> _elements;
        private UIPixel?[,] _output;
        internal UIManager(Renderer render)
        {
            _render = render;
            _elements = [];
            _output = new UIPixel[Console.WindowWidth, Console.WindowHeight];
            LastXOffset = LastYOffset = 0;
        }
        public int Count => _elements.Count;
        internal int UIMapWidth => _output.GetLength(0);
        internal int UIMapHeight => _output.GetLength(1);
        internal int LastXOffset { get; private set; }
        internal int LastYOffset { get; private set; }
        public UIElement Add(UIElement uiElement)
        {
            _elements.Add(uiElement);
            uiElement.SetUIManager(this);
            return uiElement;
        }
        public void Clear(Type? type = null)
        {
            if (type == null)
            {
                _elements.Clear();
            }
            else if (type == typeof(InfoBox))
            {
                _elements = _elements.Where(e => e is not InfoBox).ToList();
            }
            else if (type == typeof(InfoIndicator))
            {
                _elements = _elements.Where(e => e is not InfoIndicator).ToList();
            }
            else throw new ArgumentException("Invalid type");
        }
        public UIPixel? GetUIPixelOutput(int x, int y) => !OnOutput(x, y) ? null : _output[x, y];
        internal void UpdateMap()
        {
            LastXOffset = _render.Camera.XOffset; LastYOffset = _render.Camera.YOffset;
            ClearUIPairs();
            foreach (UIElement element in _elements)
            {
                element.Update();
                if (!(OnOutput(element.X, element.Y) && OnOutput(element.X + element.Width - 1, element.Y + element.Height - 1) && element.Visible))
                    continue;
                AddElementToOutput(element);
            }
        }
        private void AddElementToOutput(UIElement element)
        {
            for (int y = 0; y < element.Height; y++)
            for (int x = 0; x < element.Width; x++)
                _output[element.X + x, element.Y + y] = element.GetUIPixel(x, y);
        }
        private bool OnOutput(int x, int y) => x >= 0 && x < UIMapWidth && y >= 0 && y < UIMapHeight;
        private void ClearUIPairs() => _output = new UIPixel[Console.WindowWidth, Console.WindowHeight];
    }