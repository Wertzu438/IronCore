using System.Text;
using JumpOn.UI.InfoBoxes;
using JumpOn.UI.Menus;
using MCR_Engine.Core;
using MCR_Engine.Menus;

namespace JumpOn.Core;

public class Application : IGameProject
{
    private readonly Engine _engine;
    private readonly MenuManager _menus;
    private readonly Account _account;
    public Application(Engine engine)
    {
        _account = new Account("Wertzu", 6);
        _engine = engine;
        engine.Menus.Add(new MainMenu(_account, engine.Input));
        engine.Menus.Add(new LevelMenu(_account, engine.Input));
        engine.Menus.Add(new InfoMenu(engine.Input));
        _menus = engine.Menus;
    }
    public ProgramState OpenEditor()
    {
        throw new NotImplementedException();
    }
    public ProgramState PlayGame()
    {
        _engine.UI.Clear();
        Game game = new Game(_engine, _account, _account.CurrentLevel);
        try
        {
            _account.UpdateLevelStats(_account.CurrentLevel, game.Play());

        }
        catch (Exception e)
        {
            _engine.UpdateRenderAction(ProcessAction.Pause);
            _engine.UpdateInputAction(ProcessAction.Pause);
            Thread.Sleep(500);
            Console.WriteLine(e);
            Console.ReadKey(true);
        }
        _account.CurrentLevel++;
        return ProgramState.Menu;
    }
    public ProgramState OpenMenu()
    {
        _engine.SetScene(LevelLoader.Read("Scenes/level0.txt", _engine, new MobileInfoBox(), new Clock()));
        return _menus.Launch(_engine.UI);
    }
    public static void Main(string[] args)
    {
        Console.CursorVisible = false;
        Console.OutputEncoding = Encoding.UTF8;
        Engine engine = new Engine();
        Application app = new Application(engine);
        engine.Run(app);
    }
    public static string[] GetGameHeading()
    {
        return 
        [
            "!     ██╗██╗   ██╗███╗   ███╗██████╗    ██████╗ ███╗   ██╗",
            "!     ██║██║   ██║████╗ ████║██╔══██╗  ██╔═══██╗████╗  ██║",
            "!     ██║██║   ██║██╔████╔██║██████╔╝  ██║   ██║██╔██╗ ██║",
            "!██   ██║██║   ██║██║╚██╔╝██║██╔═══╝   ██║   ██║██║╚██╗██║",
            "!╚█████╔╝╚██████╔╝██║ ╚═╝ ██║██║       ╚██████╔╝██║ ╚████║",
            "! ╚════╝  ╚═════╝ ╚═╝     ╚═╝╚═╝        ╚═════╝ ╚═╝  ╚═══╝"
        ];
    }
}