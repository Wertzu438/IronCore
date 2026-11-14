using JumpOn.Entities;
using MCR_Engine.Core;
using MCR_Engine.ObjectSystem;

namespace JumpOn.Interactables;

public class Bar : Interactable
{
    private const string TextureName = "barPart";
    private readonly RGB?[,] _baseTexture;
    private readonly int _amount;
    public Bar(int x, int y, Textures textures, int partsCount) : base(x, y, Textures.DebuggingTexture(1, 1), false)
    {
        if (partsCount <= 0)
            throw new ArgumentException("partsCount <= 0");
        _baseTexture = textures.Get(TextureName);
        _amount = partsCount;
        SetNewTexture(CreateTexture());
    }
    private Bar(int x, int y, RGB?[,] baseTexture, int amount) : base(x, y, Textures.DebuggingTexture(1, 1), false)
    {
        _baseTexture = baseTexture;
        _amount = amount;
        SetNewTexture(CreateTexture());
    }
    public override GameObject? Clone(Objects objects) => new Bar(X, Y, _baseTexture, _amount);
    protected override bool IsObjectInteracting(GameObject gameObject) => gameObject is Entity && base.IsObjectInteracting(gameObject);
    public int HangOnIt() => Y;
    private RGB?[,] CreateTexture() => Textures.CreateExpansion(_baseTexture, Direction.Right, _amount);
}