using MCR_Engine.ObjectSystem;
using MCR_Engine.Rendering.UI;

namespace JumpOn.UI.InfoBoxes;

public class MobileInfoBox : InfoBox
{
    private GameObject? _usedBy;
    public MobileInfoBox() : base(3, 3, 0, 0, false, false)
    {
        _usedBy = null;
    }
    public GameObject? UsedBy
    {
        get => _usedBy;
        set
        {
            if (value is null)
                Visible = false;
            _usedBy = value;
        }
    }
    public void Show(GameObject usedBy, int x, int y, int width, int height, string[][] content)
    {
        UsedBy = usedBy;
        X = x; Y = y / 2 - 1;
        UpdateSize(width, height + 2);
        UpdateContent(content);
        Visible = true;
    }
}