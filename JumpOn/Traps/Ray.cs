using MCR_Engine.Core;
using MCR_Engine.ObjectSystem;
using MCR_Engine.ObjectSystem.BaseObjectTypes;

namespace JumpOn.Traps;

public class Ray : NonSolidObject
{
    private readonly RGB?[,] _baseTexture;
    public Ray(int x, int y, RGB?[,] baseTexture) : base(x, y, 1, baseTexture, Direction.Down)
    {
        _baseTexture = baseTexture;
    }
    public override GameObject? Clone(Objects objects) => null;
    public void UpdateTexture(int height)
    {
        SetNewTexture(Textures.CreateExpansion(_baseTexture, Direction.Down, height));
    }
}