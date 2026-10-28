using MCR_Engine.Core;
using MCR_Engine.Objects;

namespace JumpOn.Traps;

public class TrapChildObject : GameObject
{
    public TrapChildObject(int x, int y, RGB?[,] texture) : base(x, y, texture, 1, false, false)
    {
        
    }
    public override GameObject? Clone(ObjectManager objects) => null;
}
