using JumpOn.Core;
using MCR_Engine.Core;
using MCR_Engine.InputSystem;
using MCR_Engine.Menus;
using MCR_Engine.Rendering.UI;

namespace JumpOn.UI.Menus;

public class AccountMenu : Menu
{
    private readonly Account _account;
    private readonly Action<ProcessAction> _renderAction;
    public AccountMenu(Account account, Input input, Action<ProcessAction> changeRenderAction) : base(input, [input.Get("Enter"), input.Get("R")])
    {
        _account = account;
        _renderAction = changeRenderAction;
    }
    protected override string[][] UpdateContent()
    {
        if (_account.ValidName)
            return [Application.GetGameHeading(), ["!ACCOUNTMANAGER"], ["💾 Account vorhanden: Ja", $"👤 Name: {_account.Name}", $" ", " "], [$"-[R] Account löschen        [ENTER] Hauptmenü"]];
        return [Application.GetGameHeading(), ["!ACCOUNTMANAGER"], [$"💾 Account vorhanden: Nein", "👤 Name: ", " ", " "], [$"-[EINGABE] Name eingeben        [ENTER] Bestätigen"]];
    }
    protected override void AfterUpdateContent(UIElement infoBox)
    {
        if (_account.ValidName)
            return;
        Thread.Sleep(1200);
        _renderAction(ProcessAction.Pause);
        Thread.Sleep(200);
        Console.SetCursorPosition(infoBox.X + 10, infoBox.Y + 11);
        Console.ResetColor(); Console.ForegroundColor = ConsoleColor.DarkRed;
        _account.Name = Console.ReadLine();
        _renderAction(ProcessAction.Start);
    }
    protected override ProgramState OpenNewWindow(Key key, out Type? nextScreen)
    {
        if ((!_account.ValidName && key != Keys[0]) || key == Keys[1])
        {
            _account.Name = null;
            nextScreen = typeof(AccountMenu);
            return ProgramState.Menu;
        }
        if (key == Keys[0])
        {
            nextScreen = typeof(MainMenu);
            return ProgramState.Menu;
        }
        throw new NotImplementedException("no action for key set");
    }
}