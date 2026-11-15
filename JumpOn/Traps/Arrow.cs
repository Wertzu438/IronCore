using MCR_Engine.Core;
using MCR_Engine.ObjectSystem;
using MCR_Engine.ObjectSystem.BaseObjectTypes;

namespace JumpOn.Traps;

public class Arrow : MovingObject
{
    public Arrow(int x, int y, RGB?[,] texture) : base(x, y, 1, false, false, texture)
    {
        
    }
    public override GameObject? Clone(Objects objects) => null;
}
