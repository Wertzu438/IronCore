using JumpOn.Entities;
using MCR_Engine.Core;
using MCR_Engine.Objects;

namespace JumpOn.Interactables;

public class Bar : Interactable
{
    private const string TextureName = "barPart";
    private readonly RGB?[,] _baseTexture;
    private readonly int _amount;
    public Bar(int x, int y, Textures textures, int partsCount) : base(x, y, Textures.DebuggingTexture(1, 1))
    {
        if (partsCount <= 0)
            throw new ArgumentException("partsCount <= 0");
        _baseTexture = textures.Get(TextureName);
        _amount = partsCount;
        SetNewTexture(CreateTexture());
    }
    private Bar(int x, int y, RGB?[,] baseTexture, int amount) : base(x, y, Textures.DebuggingTexture(1, 1))
    {
        _baseTexture = baseTexture;
        _amount = amount;
        SetNewTexture(CreateTexture());
    }
    public override Sprite? Clone(Engine engine, SpriteManager sprites) => new Bar(X, Y, _baseTexture, _amount);
    public int HangOnIt() => Y;
    private RGB?[,] CreateTexture() => Textures.CreateExpansion(_baseTexture, Direction.Right, _amount);
}