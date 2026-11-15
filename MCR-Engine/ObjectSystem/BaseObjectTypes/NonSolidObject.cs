using MCR_Engine.Core;

namespace MCR_Engine.ObjectSystem.BaseObjectTypes;

public class NonSolidObject : GameObject
{
    protected readonly RGB?[,] BaseTexture;
    protected readonly Direction? TextureExpansion;
    protected readonly int? Amount;
    public NonSolidObject(int x, int y, int layer, RGB?[,] baseTexture, Direction? textureExpansion = null, int? amount = null, bool visible = true) : base(x, y, GetTexture(baseTexture, textureExpansion, amount), layer, false)
    {
        BaseTexture = baseTexture;
        TextureExpansion = textureExpansion;
        Amount = amount;
        ChangeVisibility(visible);
    }
    public override GameObject? Clone(ObjectSystem.Objects objects) => new NonSolidObject(X, Y, Layer, BaseTexture, TextureExpansion, Amount, Visible);
    private static RGB?[,] GetTexture(RGB?[,] baseTexture, Direction? textureExpansion, int? amount)
    {
        if (!(textureExpansion.HasValue && amount.HasValue) || amount.Value <= 1)
            return baseTexture;
        return Textures.CreateExpansion(baseTexture, textureExpansion.Value, amount.Value);
    }
}