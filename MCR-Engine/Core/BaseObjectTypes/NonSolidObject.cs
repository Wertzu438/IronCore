using MCR_Engine.Objects;

namespace MCR_Engine.Core.BaseObjectTypes;

public class NonSolidObject : GameObject
{
    private readonly RGB?[,] _baseTexture;
    private readonly Direction? _textureExpansion;
    private readonly int? _amount;
    public NonSolidObject(int x, int y, int layer, bool gravity, RGB?[,] baseTexture, Direction? textureExpansion = null, int? amount = null, bool visible = true) : base(x, y, GetTexture(baseTexture, textureExpansion, amount), layer, false, gravity)
    {
        _baseTexture = baseTexture;
        _textureExpansion = textureExpansion;
        _amount = amount;
        ChangeVisibility(visible);
    }
    public override GameObject? Clone(Objects.Objects objects) => new NonSolidObject(X, Y, Layer, Gravity, _baseTexture, _textureExpansion, _amount, Visible);
    private static RGB?[,] GetTexture(RGB?[,] baseTexture, Direction? textureExpansion, int? amount)
    {
        if (!(textureExpansion.HasValue && amount.HasValue) || amount.Value <= 1)
            return baseTexture;
        return Textures.CreateExpansion(baseTexture, textureExpansion.Value, amount.Value);
    }
}