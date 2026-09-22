using MCR_Engine.Core;
using MCR_Engine.InputSystem;
using MCR_Engine.Rendering.UI;

namespace MCR_Engine.Menus;

public abstract class Menu
{
    protected readonly Key[] Keys;
    protected readonly Input Input;
    private readonly UIElement _infoBox;
    protected Menu(Input input, UIElement infoBox, Key[] keys)
    {
        _infoBox = infoBox;
        _infoBox.UpdateContent([]);
        Keys = keys;
        Input = input;
    }
    public ProgramState Launch(out int? nextScreen)
    {
        _infoBox.UpdateContent(UpdateContent());
        return OpenNewWindow(GetKey(), out nextScreen);
    }
    protected abstract string[][] UpdateContent();
    protected abstract ProgramState OpenNewWindow(Key key, out int? nextScreen);
    private Key GetKey()
    {
        while (true)
        {
            SetInfoBoxStats();
            foreach (var key in Keys)
            {
                if (key.Pressed)
                    return key;
            }
            Thread.Sleep(10);
        }
    }
    private void SetInfoBoxStats()
    {
        _infoBox.X = MenuManager.InfoBoxX; _infoBox.Y = MenuManager.InfoBoxY;
        _infoBox.UpdateSize(MenuManager.MenuWidth, MenuManager.MenuHeight);
    }
}