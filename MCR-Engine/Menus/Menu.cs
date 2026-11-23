using MCR_Engine.Core;
using MCR_Engine.InputSystem;
using MCR_Engine.Rendering.UI;

namespace MCR_Engine.Menus;

public abstract class Menu
{
    protected readonly Key[] Keys;
    protected readonly Input Input;
    protected Menu(Input input, Key[] keys)
    {
        Keys = keys;
        Input = input;
    }
    public ProgramState Launch(InfoBox infoBox, out Type? nextScreen)
    {
        infoBox.UpdateContent(UpdateContent());
        AfterUpdateContent(infoBox);
        return OpenNewWindow(GetKey(infoBox), out nextScreen);
    }
    protected virtual void AfterUpdateContent(UIElement infoBox) { }
    protected abstract string[][] UpdateContent();
    protected abstract ProgramState OpenNewWindow(Key key, out Type? nextScreen);
    private Key? GetKey(UIElement infoBox)
    {
        while (Keys.Length > 0)
        {
            SetInfoBoxStats(infoBox);
            foreach (var key in Keys)
            {
                if (key.Pressed)
                    return key;
            }
            Thread.Sleep(30);
        }
        return null;
    }
    private static void SetInfoBoxStats(UIElement infoBox)
    {
        infoBox.X = MenuManager.InfoBoxX; infoBox.Y = MenuManager.InfoBoxY;
        infoBox.UpdateSize(MenuManager.MenuWidth, MenuManager.MenuHeight);
    }
}