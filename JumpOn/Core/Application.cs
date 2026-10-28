using System.Text;
using JumpOn.Entities;
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
        _account = Account.GetAccount(SceneLoader.GetSceneAmount());
        _engine = engine;
        engine.Menus.Add(new MainMenu(_account, engine.Input));
        engine.Menus.Add(new LevelMenu(_account, engine.Input));
        engine.Menus.Add(new InfoMenu(engine.Input));
        engine.Menus.Add(new AccountMenu(_account, engine.Input, engine.UpdateRenderAction));
        _menus = engine.Menus;
    }
    public ProgramState OpenEditor()
    {
        new MapEditor(_engine).Run();
        return ProgramState.Menu;
    }
    public ProgramState PlayGame()
    {
        _engine.UI.Clear();
        Game game = new Game(_engine, _account, _account.CurrentLevel);
        if (game.Play(out int? passedSeconds, out EntityData? gameData))
        {
            _account.UpdateLevelStats(_account.CurrentLevel, passedSeconds, gameData);
            if (_account.CurrentLevel == _account.MaxLevel)
                return ProgramState.Menu;
            _account.CurrentLevel++;
            _account.SaveAccount();
            return ProgramState.Game;
        }
        _account.UpdateLevelStats(_account.CurrentLevel, passedSeconds, gameData);
        _account.SaveAccount();
        return ProgramState.Menu;
    }
    public ProgramState OpenMenu()
    {
        _engine.SetRenderScene(SceneLoader.Read("level0", _engine, new MobileInfoBox(), new Clock()));
        return _menus.Launch(_engine.UI, (_account.ValidName ? typeof(MainMenu) : typeof(AccountMenu)));
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