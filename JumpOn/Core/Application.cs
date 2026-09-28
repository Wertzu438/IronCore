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
    public Application(Engine engine)
    {
        Account accountpp = new Account("Wertzu", 6);
        _engine = engine;
        engine.Menus.Add(new MainMenu(accountpp, engine.Input));
        engine.Menus.Add(new LevelMenu(accountpp, engine.Input));
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
        Game game = new Game(_engine);
        game.Play();
        return ProgramState.Game;
    }
    public ProgramState OpenMenu()
    {
        _engine.SetScene(LevelLoader.Read("test.txt", _engine, new MobileInfoBox(), new Clock()));
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