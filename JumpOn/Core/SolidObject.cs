using MCR_Engine.Core;
using MCR_Engine.Objects;

namespace JumpOn.Core;

public class SolidObject : GameObject
{
    private readonly RGB?[,] _baseTexture;
    private readonly Direction? _textureExpansion;
    private readonly int? _amount;
    public SolidObject(int x, int y, int layer, RGB?[,] baseTexture, Direction? textureExpansion = null, int? amount = null, bool visible = true) : base(x, y, GetTexture(baseTexture, textureExpansion, amount), layer, true, false)
    {
        _baseTexture = baseTexture;
        _textureExpansion = textureExpansion;
        _amount = amount;
        ChangeVisibility(visible);
    }
    public override GameObject? Clone(ObjectManager objects) => new SolidObject(X, Y, Layer, _baseTexture, _textureExpansion, _amount, Visible);
    private static RGB?[,] GetTexture(RGB?[,] baseTexture, Direction? textureExpansion, int? amount)
    {
        if (!(textureExpansion.HasValue && amount.HasValue) || amount.Value <= 1)
            return baseTexture;
        return Textures.CreateExpansion(baseTexture, textureExpansion.Value, amount.Value);
    }
}
public class TrapChildObject : GameObject
{
    private readonly RGB?[,] _texture;
    public TrapChildObject(int x, int y, RGB?[,] texture) : base(x, y, texture, 1, false, false)
    {
        
    }
    public override GameObject? Clone(ObjectManager objects) => null;
}
public class DecorationObject : GameObject
{
    private readonly RGB?[,] _baseTexture;
    private readonly Direction? _textureExpansion;
    private readonly int? _amount;
    public DecorationObject(int x, int y, int layer, RGB?[,] baseTexture, Direction? textureExpansion = null, int? amount = null) : base(x, y, GetTexture(baseTexture, textureExpansion, amount), layer, false, false)
    {
        _baseTexture = baseTexture;
        _textureExpansion = textureExpansion;
        _amount = amount;
    }
    public override GameObject? Clone(ObjectManager objects) => new DecorationObject(X, Y, Layer, _baseTexture, _textureExpansion, _amount);
    private static RGB?[,] GetTexture(RGB?[,] baseTexture, Direction? textureExpansion, int? amount)
    {
        if (!(textureExpansion.HasValue && amount.HasValue) || amount.Value <= 1)
            return baseTexture;
        return Textures.CreateExpansion(baseTexture, textureExpansion.Value, amount.Value);
    }
}