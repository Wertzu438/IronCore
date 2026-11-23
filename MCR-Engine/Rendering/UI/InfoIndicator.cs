using System.Data;
using MCR_Engine.Core;
using MCR_Engine.ObjectSystem;

namespace MCR_Engine.Rendering.UI;

public class InfoIndicator : UIElement
{
    private GameObject _followingObject;
    public InfoIndicator(GameObject followingObject) : base(6, 3, 0, 0, true)
    {
        _followingObject = followingObject;
    }
    internal override void Update()
    {
        X = GetX();
        Y = GetY();
        Visible = !(ObjX >= 0 && ObjY / 2 >= 0 && ObjX < UI.UIMapWidth && ObjY / 2 < UI.UIMapHeight); 
    }
    private int ObjX => _followingObject.X - UI.LastXOffset;
    private int ObjY => _followingObject.Y - UI.LastYOffset;
    protected override UIPixel[,] CreateBuffer(int width, int height)
    {
        UIPixel[,] buffer = base.CreateBuffer(width, height);
        for (int y = 0; y < height; y++)
        for (int x = 0; x < width; x++)
            buffer[x, y] = new UIPixel(" ", new RGB(), new RGB());
        return buffer;
    }
    private int GetX() => Math.Clamp(ObjX, 0, UI.UIMapWidth - Width);
    private int GetY() => Math.Clamp(ObjY / 2, 0, UI.UIMapHeight - Height);
}