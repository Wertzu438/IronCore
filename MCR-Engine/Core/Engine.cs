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
    private Scene? _scene;
    public Engine()
    {
        _renderer = new Renderer();
        _input = new Input();
        _textures = new Textures();
        _menus = new MenuManager();
        FileManager.LoadKeys(_input);
        FileManager.LoadTextures(_textures);
    }
    public Input Input => _input;
    public Textures Textures => _textures;
    public MenuManager Menus => _menus;
    public Scene Scene => _scene ?? throw new InvalidOperationException("Scene is not initialized");
    public UIManager UI => _renderer.UI;
    public SpriteManager Sprites => Scene.Sprites;
    public Camera Camera => _renderer.Camera;
    public void Run(IGameProject game)
    {   
        ProgramState state = ProgramState.Menu;
        UpdateRenderAction(ProcessAction.Start);
        UpdateInputAction(ProcessAction.Start);
        do
        {
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
    public void SetScene(Scene scene)
    {
        _scene = scene;
        _renderer.SetScene = _scene;
    }
    public void UpdateRenderAction(ProcessAction newAction) => _renderer.ChangeAction(newAction);
    public void UpdateInputAction(ProcessAction newAction) => _input.ChangeAction(newAction);
    static void Main(string[] args)
    {
        throw new ArgumentException("Should never be called => Start with game project");
    }
}
