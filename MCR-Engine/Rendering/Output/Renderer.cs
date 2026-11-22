using System.Diagnostics;
using System.Text;
using MCR_Engine.Core;
using MCR_Engine.InputSystem;
using MCR_Engine.Rendering.UI;

namespace MCR_Engine.Rendering.Output;

internal class Renderer : ProcessBase
{
    private readonly StringBuilder _output;
    private readonly StreamWriter _writer;
    private readonly Stopwatch _stopwatch;
    private readonly Thread _thread;
    private readonly UIManager _ui;
    private readonly Camera _camera;
    private Pixel[,]? _pairs;
    private RGB _currentForegroundColor;
    private RGB _currentBackgroundColor;
    private float _lastDeltaTime;
    public Renderer(Input input)
    {
        _output = new StringBuilder();
        _stopwatch = new Stopwatch();
        _writer =  new StreamWriter(Console.OpenStandardOutput());
        _thread = new Thread(Loop);
        _camera = new Camera(this, input);
        _ui = new UIManager(this);
        _currentBackgroundColor = default;
        _currentForegroundColor = default;
        _pairs = null;
        _thread.Priority = ThreadPriority.AboveNormal;
        _lastDeltaTime = 0;
    }
    public UIManager UI => _ui;
    public Camera Camera => _camera;
    public int PixelMapWidth => (_pairs is null ? Console.WindowWidth : _pairs.GetLength(0));
    public int PixelMapHeight => (_pairs is null ? Console.WindowHeight : _pairs.GetLength(1));
    public int FramesPerSecond => (int)(1 / _lastDeltaTime); 
    public Scene SetScene
    {
        set
        {
            Camera.SetFollowedObject(null);
            _pairs = CreatePairs(value);
        }
    }
    public bool ValidPairCoordinates(int x, int y) => x >= 0 && y >= 0 && x < PixelMapWidth && y < PixelMapHeight;
    protected override void StartLoop() => _thread.Start();
    private void Loop()
    {
        while (Action is not ProcessAction.Stop)
        {
            _lastDeltaTime = (float)_stopwatch.ElapsedTicks / Stopwatch.Frequency;
            _stopwatch.Restart();
            if (!Engine.TerminalInForeground)
            {
                Console.ResetColor(); Console.Clear(); 
            }
            while (Action is ProcessAction.Pause || _pairs is null || !Engine.TerminalInForeground)
            {
                State = ProcessState.Paused;
                Thread.Sleep(100);
            }
            State = ProcessState.Running;
            _camera.RecalculateOffset(_lastDeltaTime);
            _output.Clear();
            _ui.UpdateMap();
            int startX, startY, maxX, maxY;
            lock (_camera)
            {
                startX = _camera.XOffset; startY = _camera.YOffset;
                maxX = _camera.GetRenderBorderX(startX); maxY = _camera.GetRenderBorderY(startY);
            }
            for (int y = startY; y < maxY; y += 2)
            {
                for (int x = startX; x < maxX; x++)
                {
                    UIPixel? uiPixel = _ui.GetUIPixelOutput(x - startX, y / 2 - startY / 2);
                    if (uiPixel is not null)
                    {
                        AppendUIPixel(uiPixel);
                    }
                    else AppendPair(_pairs[x, y], _pairs[x, y + 1]);
                }
                if (y != maxY - 1)
                    _output.AppendLine();
            }
            RenderStringBuilder();
        }
        State = ProcessState.Stopped;
    }
    private void RenderStringBuilder()
    {
        _output.Append("\e[H\e[3J");
        _writer.Write(_output.ToString());
        _writer.Flush();
    }
    private void AppendOutput(RGB foregroundColor, RGB backgroundColor, string output)
    {
        RGB rgb = foregroundColor;
        if (!rgb.Equals(_currentForegroundColor))
        {
            _output.Append($"\e[38;2;{rgb.Red};{rgb.Green};{rgb.Blue}m");
            _currentForegroundColor = rgb;
        }
        rgb = backgroundColor;
        if (!rgb.Equals(_currentBackgroundColor))
        {
            _output.Append($"\e[48;2;{rgb.Red};{rgb.Green};{rgb.Blue}m");
            _currentBackgroundColor = rgb;
        }
        _output.Append(output);
    }
    private void AppendPair(Pixel upperPixel, Pixel lowerPixel) => AppendOutput(upperPixel.GetRGB(), lowerPixel.GetRGB(), Pixel.Output.ToString());
    private void AppendUIPixel(UIPixel uiPixel) => AppendOutput(uiPixel.ForegroundColor, uiPixel.BackgroundColor, uiPixel.Output);
    private static Pixel[,] CreatePairs(Scene scene)
    {
        var newPixelMap = new Pixel[scene.Width, scene.Height];
        for (int y = 0; y < scene.Height; y++)
        for (int x = 0; x < scene.Width; x++)
            newPixelMap[x, y] = scene.GetPixel(x, y);
        return newPixelMap;
    }
}