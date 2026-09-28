using MCR_Engine.Core;
using MCR_Engine.Objects;

namespace JumpOn;

public class SolidObject : Sprite
{
    public SolidObject(int x, int y, int layer, RGB?[,] baseTexture, Direction? textureExpansion = null, int? amount = null) : base(x, y, GetTexture(baseTexture, textureExpansion, amount), layer, true, false)
    {
    }
    private static RGB?[,] GetTexture(RGB?[,] baseTexture, Direction? textureExpansion, int? amount)
    {
        if (!(textureExpansion.HasValue && amount.HasValue) || amount.Value <= 1)
            return baseTexture;
        return Textures.CreateExpansion(baseTexture, textureExpansion.Value, amount.Value);
    }
}
public class TrapChildObject : Sprite
{
    public TrapChildObject(int x, int y, RGB?[,] texture) : base(x, y, texture, 0, false, false)
    {
        
    }
}

public class DecorationObject : Sprite
{
    public DecorationObject(int x, int y, int layer, RGB?[,] baseTexture, Direction? textureExpansion = null, int? amount = null) : base(x, y, GetTexture(baseTexture, textureExpansion, amount), layer, false, false)
    {
    }
    private static RGB?[,] GetTexture(RGB?[,] baseTexture, Direction? textureExpansion, int? amount)
    {
        if (!(textureExpansion.HasValue && amount.HasValue) || amount.Value <= 1)
            return baseTexture;
        return Textures.CreateExpansion(baseTexture, textureExpansion.Value, amount.Value);
    }
}