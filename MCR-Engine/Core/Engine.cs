using MCR_Engine.InputSystem;
using MCR_Engine.Menus;
using MCR_Engine.Objects;
using MCR_Engine.Rendering.Output;
using MCR_Engine.Rendering.UI;

namespace MCR_Engine.Core;

public class Engine
{
    private readonly Renderer _renderer;
    private readonly Input _input;
    private readonly Textures _textures;
    private readonly MenuManager _menus;
    public Engine()
    {
        _renderer = new Renderer();
        _input = new Input();
        _textures = new Textures();
        _menus = new MenuManager();
        FileManager.LoadKeys(_input);
        FileManager.LoadTextures(_textures);
    }
    public int FramesPerSecond => _renderer.FramesPerSecond;
    public Input Input => _input;
    public Textures Textures => _textures;
    public MenuManager Menus => _menus;
    public UIManager UI => _renderer.UI;
    public Camera Camera => _renderer.Camera;
    public void Run(IGameProject game)
    {   
        ProgramState state = ProgramState.Menu;
        UpdateRenderAction(ProcessAction.Start);
        UpdateInputAction(ProcessAction.Start);
        do
        {
            Thread.Sleep(200);
            state = (state) switch 
            {
                ProgramState.Menu => game.OpenMenu(),
                ProgramState.Game => game.PlayGame(),
                ProgramState.Editor => game.OpenEditor(),
                _ => throw new NotImplementedException($"Unknown ProgramState: {state}")
            };
        } while (state is not ProgramState.Quit);
        UpdateInputAction(ProcessAction.Stop);
        UpdateRenderAction(ProcessAction.Stop);
    }
    public void SetRenderScene(Scene scene)
    {
        scene.Update(0, true);
        _renderer.SetScene = scene;
    }
    public void UpdateRenderAction(ProcessAction newAction) => _renderer.ChangeAction(newAction);
    public void UpdateInputAction(ProcessAction newAction) => _input.ChangeAction(newAction);
    public static bool Contains<T>(T searchedType, T[] types) => types.Contains(searchedType);
    public static void Main() => throw new ArgumentException("Should never be called => Start with game project");
}
