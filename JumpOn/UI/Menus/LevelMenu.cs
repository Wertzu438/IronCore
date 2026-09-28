using JumpOn.Core;
using MCR_Engine.Core;
using MCR_Engine.InputSystem;
using MCR_Engine.Menus;
using MCR_Engine.Rendering.UI;

namespace JumpOn.UI.Menus;

public class LevelMenu : Menu
{
    private readonly Account _account;
    public LevelMenu(Account account, Input input) : base(input, [input.Get("G"), input.Get("Enter"), input.Get("Up"), input.Get("Down")])
    {
        _account = account;
    }
    private int Level => _account.CurrentLevel + 1;
    protected override ProgramState OpenNewWindow(Key key, out Type? nextScreen)
    {
        Thread.Sleep(100);
        if (key == Keys[0])
        {
            nextScreen = null;
            return ProgramState.Game;
        }
        if (key == Keys[1])
        {
            nextScreen = typeof(MainMenu);
            return ProgramState.Menu;
        }
        nextScreen = typeof(LevelMenu);
        if (key == Keys[2])
        {
            _account.CurrentLevel--;
            return ProgramState.Menu;
        }
        if (key == Keys[3])
        {
            _account.CurrentLevel++;
            return ProgramState.Menu;
        }
        throw new NotImplementedException("no action for key set");
    }
    protected override string[][] UpdateContent()
    {
        LevelStats? stats = _account.GetStats(_account.CurrentLevel);
        string[] menuHeading = ["!LEVELAUSWAHL"];
        string[] levelInfo = [$"-Aktuelles Level: {Level}          Höchstes Level: {_account.MaxLevel+1}"];
        string[] buttonInfo = [$"-[Up]/[Down] Level wechseln     [G] Level starten     [ENTER] Hauptmenü"];
        if (!stats.HasValue)
        {
            return [Application.GetGameHeading(), menuHeading, levelInfo, [$"Beste Zeit: -", $"Anzahl Versuche: 0"], buttonInfo];
        }
        if (!stats.Value.LevelFinished)
        {
            return [Application.GetGameHeading(), menuHeading, levelInfo, [$"Beste Zeit: - (Level nicht geschafft)", $"Anzahl Versuche: {stats.Value.Tries}"], buttonInfo];
        }
        return [Application.GetGameHeading(), menuHeading, levelInfo, [$"Beste Zeit: {stats.Value.Time} Sekunden", $"Anzahl Versuche: {stats.Value.Tries}"], buttonInfo];
    }
}