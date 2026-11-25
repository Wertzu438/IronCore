using System.Text;
using MCR_Engine.Core;
using MCR_Engine.ObjectSystem;

namespace MCR_Engine.Rendering.UI;

public class InfoIndicator : UIElement
{
    private readonly GameObject _followingObject;
    private readonly RGB _backgroundColor, _foregroundColor;
    private readonly string _emoji;
    private readonly Func<bool> _showWhen;
    public InfoIndicator(GameObject followingObject, string emoji, Func<bool> showWhen) : base(1, 1, 0, 0, true)
    {
        _followingObject = followingObject;
        _emoji = IsEmoji(emoji) ? emoji : "??";
        _foregroundColor = new RGB(255, 255, 255);
        _backgroundColor = new RGB(40, 30, 20);
        Buffer = CreateBuffer(1, 1);
        _showWhen = showWhen;
    }
    private int ObjX => _followingObject.X + _followingObject.Width/2 - UI.LastXOffset;
    private int ObjY => _followingObject.Y + _followingObject.Height/2 - UI.LastYOffset;
    internal override void Update()
    {
        Visible = !(ObjX >= 0 && ObjY / 2 >= 0 && ObjX < UI.UIMapWidth && ObjY / 2 < UI.UIMapHeight) && _followingObject.Visible && _showWhen(); 
        X = Math.Clamp(ObjX, 0, UI.UIMapWidth - Width);
        Y = Math.Clamp(ObjY / 2, 0, UI.UIMapHeight - Height - 0);
    }
    protected override UIPixel[,] CreateBuffer(int width, int height)
    {
        if (width != 1 || height != 1)
            throw new ArgumentException("Width and height must be 1.");
        return new UIPixel[,] { {new(_emoji ?? "  ", _foregroundColor, _backgroundColor)}, {new("", _foregroundColor, _backgroundColor)} };
    }
    private static bool IsEmoji(string symbol)
    {
        if (string.IsNullOrEmpty(symbol))
            return false;
        List<Rune> runes = symbol.EnumerateRunes().ToList();
        if (runes.Count == 0)
            return false;
        return !runes.Any(a => a.IsAscii);
    }
}