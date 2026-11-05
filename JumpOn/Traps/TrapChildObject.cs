using MCR_Engine.Core;
using MCR_Engine.Core.BaseObjectTypes;
using MCR_Engine.Objects;

namespace JumpOn.Traps;

public class TrapChildObject : NonSolidObject
{
    public TrapChildObject(int x, int y, RGB?[,] texture) : base(x, y, 1, false, texture)
    {
        
    }
    public override GameObject? Clone(Objects objects) => null;
}
