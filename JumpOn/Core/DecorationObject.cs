using MCR_Engine.Core;
using MCR_Engine.ObjectSystem;
using MCR_Engine.ObjectSystem.BaseObjectTypes;

namespace JumpOn.Core;

public class DecorationObject : NonSolidObject
{
    public DecorationObject(int x, int y, int layer, RGB?[,] baseTexture, Direction? textureExpansion = null, int? amount = null) : base(x, y, layer, baseTexture, textureExpansion, amount)
    {
        
    }
    public override GameObject? Clone(Objects objects) => new DecorationObject(X, Y, Layer, BaseTexture, TextureExpansion, Amount);
}