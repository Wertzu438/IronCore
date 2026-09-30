using MCR_Engine.Core;
using MCR_Engine.Rendering.Output;

namespace MCR_Engine.Rendering.UI;

public class UIManager
    {
        private const char FrameOutput = '▀';
        private readonly RGB _textColor, _specialTextColor, _borderColor, _backgroundColor;
        private readonly List<UIElement> _elements;
        private readonly Renderer _render;
        internal UIManager(Renderer render)
        {
            _render = render;
            _specialTextColor = new RGB(255, 255, 150);
            _textColor = new RGB(255, 255, 255);
            _borderColor = new RGB(120, 120, 120);
            _backgroundColor = new RGB(0, 0, 0);
            _elements = [];
        }
        public void UpdateMap()
        {
            lock (_elements)
            {
                ResetUIPairs();
                foreach (UIElement element in _elements)
                {
                    int x = element.StaticBox ? _render.Camera.XOffset + element.X : element.X;
                    int y = element.StaticBox ? _render.Camera.YOffset + element.Y : element.Y;
                    if (!(_render.Camera.InView(x, y) && _render.Camera.InView(x + element.Width, y + element.Height)) || !element.Visible)
                        continue;
                    SetUIBorder(element, x, y);
                    UpdateLettersInUIArea(element, x, y);
                }
            }
        }
        public UIElement Add(UIElement uiElement)
        {
            lock (_elements)
            {
                _elements.Add(uiElement);
            }
            return uiElement;
        }
        public void Clear()
        {
            lock  (_elements)
            {
                _elements.Clear();
            }
        }
        private void SetUIBorder(UIElement ui, int uiX, int uiY)
        {
            for (int y = 0; y < ui.Height; y++)
            {
                _render.GetPixelPair(uiX, uiY + y).SetUIPair(_borderColor, _borderColor, FrameOutput);
                _render.GetPixelPair(uiX + ui.Width - UIElement.Border, uiY + y).SetUIPair(_borderColor, _borderColor, FrameOutput);
                if (y != 0 && y != ui.Height - UIElement.Border)
                    continue;
                for (int x = 1; x < ui.Width - UIElement.Border; x++)
                {
                    _render.GetPixelPair(uiX + x, uiY).SetUIPair(_borderColor, _backgroundColor, FrameOutput);
                    _render.GetPixelPair(uiX + x, uiY + ui.Height - UIElement.Border).SetUIPair(_backgroundColor, _borderColor, FrameOutput);
                }
            }
        }
        private void UpdateLettersInUIArea(UIElement ui, int uiX, int uiY)
        {
            for (int y = 0; y < ui.Height - UIElement.Border * 2; y++)
            {
                string line = ui.GetLine(y, out bool specialText);
                int yMap = uiY + y + UIElement.Border;
                for (int x = 0; x < ui.Width - UIElement.Border * 2; x++)
                {
                    int xMap = uiX + x + UIElement.Border;
                    if (line[0] == UIElement.Separator)
                    {
                        _render.GetPixelPair(xMap, yMap).SetUIPair(_borderColor, _backgroundColor, UIElement.Separator);
                    }
                    else if (x < line.Length)
                    {
                        _render.GetPixelPair(xMap, yMap).SetUIPair(specialText ? _specialTextColor : _textColor, _backgroundColor, line[x]);
                    }
                    else _render.GetPixelPair(xMap, yMap).SetUIPair(_backgroundColor, _backgroundColor, ' ');
                }
            }
        }
        private void ResetUIPairs()
        {
            int xStart = _render.Camera.XOffset, xEnd = _render.Camera.GetRenderBorderX(xStart);
            int yStart = _render.Camera.YOffset, yEnd = _render.Camera.GetRenderBorderY(yStart);
            for (int y = yStart; y < yEnd; y++)
                for (int x = xStart; x < xEnd; x++)
                    _render.GetPixelPair(x, y).ResetUIPair();
        }
    }