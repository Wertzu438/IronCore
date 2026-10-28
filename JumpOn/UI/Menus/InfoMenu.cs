using JumpOn.Core;
using JumpOn.Interactables;
using MCR_Engine.Core;
using MCR_Engine.InputSystem;
using MCR_Engine.Menus;
using MCR_Engine.Rendering.UI;

namespace JumpOn.UI.Menus;

public class InfoMenu : Menu
{
    public InfoMenu(Input input) : base(input, [input.Get("Enter")])
    {

    }
    protected override ProgramState OpenNewWindow(Key key, out Type? nextScreen)
    {
        if (key != Keys[0])
            throw new NotImplementedException("no action for key set");
        nextScreen = typeof(MainMenu);
        return ProgramState.Menu;
    }
    protected override string[][] UpdateContent()
    {
        return [Application.GetGameHeading(), ["!PROGRAMMINFO"], ["-Developer: Simon Straubinger        Co-Developer: Uzair Abbas"], ["📝 Erstellt am: 15.08.2026", "🏁 Fertig am: 15.10.2026"], [$"-[ENTER] Hauptmenü"]];
    }
}