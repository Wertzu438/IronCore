using MCR_Engine.Core;
using MCR_Engine.ObjectSystem;

namespace MCR_Engine.Rendering.UI;

public class InfoIndicator : UIElement //TODO: Pfeile löschen in game + methode in uimanager zum nur pfiele löschen
{
    private readonly GameObject _followingObject;
    private readonly RGB _backgroundColor, _foregroundColor;
    private readonly string _emoji;
    public InfoIndicator(GameObject followingObject, string emoji) : base(1, 1, 0, 0, true)
    {
        _followingObject = followingObject;
        _emoji = emoji;
        _foregroundColor = new RGB(255, 255, 150);
        _backgroundColor = new RGB(0, 0, 0);
    }
    private int ObjX => _followingObject.X - UI.LastXOffset;
    private int ObjY => _followingObject.Y - UI.LastYOffset;
    internal override void Update()
    {
        Visible = !(ObjX >= 0 && ObjY / 2 >= 0 && ObjX < UI.UIMapWidth && ObjY / 2 < UI.UIMapHeight); 
        
        X = Math.Clamp(ObjX, 0, UI.UIMapWidth - Width);
        Y = Math.Clamp(ObjY / 2, 0, UI.UIMapHeight - Height - 0);
        Buffer = CreateBuffer(1, 1);
    }
    protected override UIPixel[,] CreateBuffer(int width, int height)
    {
        if (width != 1 || height != 1)
            throw new ArgumentException("Width and height must be 1.");
        return new UIPixel[,] { {new(_emoji ?? "  ", _foregroundColor, _backgroundColor)}, {new("", _foregroundColor, _backgroundColor)} };
    }
}