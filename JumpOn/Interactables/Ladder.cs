using JumpOn.Entities;
using MCR_Engine.Core;
using MCR_Engine.Objects;

namespace JumpOn.Interactables;

public class Ladder : Interactable
{
    private const string TextureName = "ladderPart";
    private readonly RGB?[,] _baseTexture;
    private readonly int _amount;
    public Ladder(int x, int y, Textures textures, int partsCount) : base(x, y, textures.Get(TextureName), false)
    {
        if (partsCount <= 0)
            throw new ArgumentException("partsCount <= 0");
        _baseTexture = textures.Get(TextureName);
        _amount = partsCount;
        SetNewTexture(CreateTexture());
    }
    private Ladder(int x, int y, RGB?[,] baseTexture, int amount) : base(x, y, Textures.DebuggingTexture(1, 1), false)
    {
        _baseTexture = baseTexture;
        _amount = amount;
        SetNewTexture(CreateTexture());
    }
    public override GameObject? Clone(ObjectManager objects) => new Ladder(X, Y, _baseTexture, _amount);
    protected override bool IsObjectInteracting(GameObject gameObject) => gameObject is Entity && base.IsObjectInteracting(gameObject);
    public void CenterEntity(Entity entity)
    {
        int offset = (Width - entity.Width) / 2;
        if (offset < 0)
            throw new ArgumentException("offset < 0");
        entity.X = X + offset;
    }
    private RGB?[,] CreateTexture() => Textures.CreateExpansion(_baseTexture, Direction.Down, _amount);
    public static bool IsEntityOnTopOfLadder(Entity entity)
    {
        if (!entity.IsBordersCollision(out GameObject? sprite, [typeof(Ladder)]))
            return false;
        Ladder ladder = (Ladder)sprite!;
        int distance = ladder.Y - (entity.Y + entity.Height);
        return distance == -1;
    }
}