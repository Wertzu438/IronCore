using MCR_Engine.Core;
using MCR_Engine.ObjectSystem;
using MCR_Engine.ObjectSystem.BaseObjectTypes;

namespace JumpOn.Traps;

public class TrapChildObject : MovingObject
{
    public TrapChildObject(int x, int y, RGB?[,] texture) : base(x, y, 1, false, false, texture)
    {
        
    }
    public override GameObject? Clone(Objects objects) => null;
}
