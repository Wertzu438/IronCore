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
    private PixelPair[,]? _pairs;
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
    public int PixelPairWidth => (_pairs is null ? Console.WindowWidth : _pairs.GetLength(0));
    public int PixelPairHeight => (_pairs is null ? Console.WindowHeight : _pairs.GetLength(1));
    public int FramesPerSecond => (int)(1 / _lastDeltaTime); 
    public Scene SetScene
    {
        set
        {
            Camera.SetFollowedObject(null);
            _pairs = CreatePairs(value);
        }
    }
    public PixelPair GetPixelPair(int x, int y)
    {
        if (_pairs is null || !ValidPairCoordinates(x, y))
            throw new ArgumentException("Invalid pair coordinates or no scene set.");
        return _pairs[x, y];
    }
    public bool ValidPairCoordinates(int x, int y) => x >= 0 && y >= 0 && x < PixelPairWidth && y < PixelPairHeight;
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
            int startX, startY, maxX, maxY;
            lock (_camera)
            {
                _ui.UpdateMap();
                startX = _camera.XOffset; startY = _camera.YOffset;
                maxX = _camera.GetRenderBorderX(startX); maxY = _camera.GetRenderBorderY(startY);
            }
            for (int y = startY; y < maxY; y++)
            {
                for (int x = startX; x < maxX; x++)
                    AppendPair(_pairs[x, y]);
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
    private void AppendPair(PixelPair pair)
    {
        RGB rgb = pair.UpperRGB;
        if (!rgb.Equals(_currentForegroundColor))
        {
            _output.Append($"\e[38;2;{rgb.Red};{rgb.Green};{rgb.Blue}m");
            _currentForegroundColor = rgb;
        }
        rgb = pair.LowerRGB;
        if (!rgb.Equals(_currentBackgroundColor))
        {
            _output.Append($"\e[48;2;{rgb.Red};{rgb.Green};{rgb.Blue}m");
            _currentBackgroundColor = rgb;
        }
        _output.Append(pair.Output);
    }
    private static PixelPair[,] CreatePairs(Scene scene)
    {
        int height = scene.Height / 2 + scene.Height % 2;
        PixelPair[,] newPairs = new PixelPair[scene.Width, height];
        for (int y = 0; y < height; y++)
        for (int x = 0; x < scene.Width; x++)
            newPairs[x, y] = new PixelPair(scene.GetPixel(x, y * 2), scene.GetPixel(x, y * 2 + 1));
        return newPairs;
    }
}