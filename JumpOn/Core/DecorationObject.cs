using MCR_Engine.Core;
using MCR_Engine.Core.BaseObjectTypes;
using MCR_Engine.Objects;

namespace JumpOn.Core;

public class DecorationObject : NonSolidObject
{
    public DecorationObject(int x, int y, int layer, RGB?[,] baseTexture, Direction? textureExpansion = null, int? amount = null) : base(x, y, layer, true, baseTexture, textureExpansion, amount)
    {
        
    }
    public override GameObject? Clone(Objects objects) => new DecorationObject(X, Y, Layer, BaseTexture, TextureExpansion, Amount);
}