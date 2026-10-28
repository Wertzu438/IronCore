using MCR_Engine.Objects;

namespace MCR_Engine.Core.BaseObjectTypes;

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