using MCR_Engine.Objects;
using MCR_Engine.Rendering.UI;

namespace JumpOn.UI.InfoBoxes;

public class MobileInfoBox : UIElement
{
    private Sprite? _usedBy;
    public MobileInfoBox() : base(3, 3, 0, 0, false, false)
    {
        _usedBy = null;
    }
    public Sprite? UsedBy
    {
        get => _usedBy;
        set
        {
            if (value is null)
                Visible = false;
            _usedBy = value;
        }
    }
    public void Show(Sprite usedBy, int x, int y, int width, int height, string[][] content)
    {
        UsedBy = usedBy;
        X = x; Y = y;
        UpdateSize(width, height + 2);
        UpdateContent(content);
        Visible = true;
    }
}