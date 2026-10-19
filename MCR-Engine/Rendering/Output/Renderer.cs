using System.Diagnostics;
using System.Text;
using MCR_Engine.Core;
using MCR_Engine.Rendering.UI;

namespace MCR_Engine.Rendering.Output;

internal class Renderer
{
    private readonly StringBuilder _output;
    private readonly StreamWriter _writer;
    private readonly Thread _thread;
    private readonly UIManager _ui;
    private readonly Camera _camera;
    private PixelPair[,]? _pairs;
    private RGB _currentForegroundColor;
    private RGB _currentBackgroundColor;
    private ProcessAction? _action;
    private ProcessState _state;
    public Renderer()
    {
        _output = new StringBuilder();
        _writer =  new StreamWriter(Console.OpenStandardOutput());
        _thread = new Thread(Loop);
        _camera = new Camera(this);
        _ui = new UIManager(this);
        _currentBackgroundColor = default;
        _currentForegroundColor = default;
        _state = ProcessState.Ready;
        _pairs = null;
        _action = null;
        _thread.Priority = ThreadPriority.AboveNormal;
    }
    public UIManager UI => _ui;
    public Camera Camera => _camera;
    public int PixelPairWidth => (_pairs is null ? Console.WindowWidth : _pairs.GetLength(0));
    public int PixelPairHeight => (_pairs is null ? Console.WindowHeight : _pairs.GetLength(1));
    public Scene SetScene
    {
        set
        {
            _camera.ResetOffset();
            _pairs = CreatePairs(value);
        }
    }
    public void ChangeAction(ProcessAction newAction)
    {
        if (newAction is ProcessAction.Start && _state is ProcessState.Ready or ProcessState.Paused)
        {
            _action = newAction;
            if (_state is not ProcessState.Paused)
                StartLoop();
        }
        else if ((newAction is ProcessAction.Stop && _state is ProcessState.Running or ProcessState.Paused) || (newAction is ProcessAction.Pause && _state is ProcessState.Running))
        {
            _action = newAction;
        }
        else throw new ArgumentException("Invalid action");
    }
    public PixelPair GetPixelPair(int x, int y)
    {
        if (_pairs is null || !ValidPairCoordinates(x, y))
            throw new ArgumentException("Invalid pair coordinates or no scene set.");
        return _pairs[x, y];
    }
    public bool ValidPairCoordinates(int x, int y) => x >= 0 && y >= 0 && x < PixelPairWidth && y < PixelPairHeight;
    private void Loop()
    {
        int frames = 0;
        Stopwatch fpsWatch = Stopwatch.StartNew();
        while (_action is not ProcessAction.Stop)
        {
            while (_action is ProcessAction.Pause || _pairs is null)
            {
                _state = ProcessState.Paused;
                Thread.Sleep(1000);
            }
            _state = ProcessState.Running;
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
            frames++;
            if (fpsWatch.ElapsedMilliseconds >= 1000)
            {

                Console.Title = $"FPS: {frames}";
                frames = 0;

                fpsWatch.Restart();
            }
        }
        _state = ProcessState.Stopped;
    }
    private void RenderStringBuilder()
    {
        Console.SetCursorPosition(0, 0);
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
    private void StartLoop() => _thread.Start();
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