using MCR_Engine.Core;
using MCR_Engine.Objects;

namespace JumpOn;

public class SolidObject : Sprite
{
    public SolidObject(int x, int y, RGB?[,] texture) : base(x, y, texture, 0, true, false)
    {
        
    }
}