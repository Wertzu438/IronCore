using JumpOn.Core;
using MCR_Engine.Core;
using MCR_Engine.InputSystem;
using MCR_Engine.Menus;
using MCR_Engine.Rendering.UI;

namespace JumpOn.UI.Menus;

public class MainMenu : Menu
{
    private readonly Account _account;
    public MainMenu(Account account, Input input) : base(input, [input.Get("L"), input.Get("I"), input.Get("A"), input.Get("B"), input.Get("E")])
    {
        _account = account;
    }
    protected override ProgramState OpenNewWindow(Key key, out Type? nextScreen)
    {
        nextScreen = null;
        if (key == Keys[0] || key ==  Keys[1] || key == Keys[2])
        {
            if (key == Keys[0])
            {
                nextScreen = typeof(LevelMenu);
            }
            else if (key == Keys[1])
            {
                nextScreen = typeof(InfoMenu);
            }
            else nextScreen = typeof(AccountMenu);
            return ProgramState.Menu;
        }
        if (key == Keys[3])
            return ProgramState.Quit;
        if (key == Keys[4])
            return ProgramState.Editor;
        throw new NotImplementedException("no action for key set");
    }
    protected override string[][] UpdateContent()
    {
        return [Application.GetGameHeading(), [$"!HAUPTMENÜ"], [$"-Name: {_account.Name}            Höchstes Level: {_account.MaxLevel+1}"], ["[L] - Levelauswahl", "[I] - Programminfo", $"[A] - Account", $"[B] - Verlassen"]];
    }
}