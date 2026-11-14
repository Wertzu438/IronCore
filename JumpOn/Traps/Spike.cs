using JumpOn.Entities;
using JumpOn.Entities.LivingEntities;
using MCR_Engine.Core;
using MCR_Engine.ObjectSystem;

namespace JumpOn.Traps;

public class Spike : Trap
{
    private const double EntityDamage = 2.2;
    private const string TextureName = "spike";
    private readonly RGB?[,] _baseTexture;
    private readonly int _amount;
    public Spike(int x, int y, Textures textures, int partsCount) : base(x, y, textures.Get(TextureName), false)
    {
        if (partsCount <= 0)
            throw new ArgumentException("partsCount <= 0");
        _baseTexture = textures.Get(TextureName);
        _amount = partsCount;
        SetNewTexture(CreateTexture());
    }
    private Spike(int x, int y, RGB?[,] baseTexture, int partsCount) : base(x, y, baseTexture, false)
    {
        if (partsCount <= 0)
            throw new ArgumentException("partsCount <= 0");
        _baseTexture = baseTexture;
        _amount = partsCount;
        SetNewTexture(CreateTexture());
    }
    protected override double Damage => EntityDamage;
    public override GameObject? Clone(Objects objects) => new Spike(X, Y, _baseTexture, _amount);
    protected override bool IsSomeoneInTrap(out Entity? entity)
    {
        entity = null;
        if (IsExactCollision(out GameObject? sprite, [typeof(Player)]))
            entity = sprite as Entity;
        else return false;
        return true;
    }
    private RGB?[,] CreateTexture() => Textures.CreateExpansion(_baseTexture, Direction.Right, _amount);
}