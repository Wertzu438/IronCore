using JumpOn.Entities;
using MCR_Engine.Core;
using MCR_Engine.Objects;

namespace JumpOn.Traps;

public class Spike : Trap
{
    private const double EntityDamage = 1.2;
    private const string TextureName = "spike";
    public Spike(int x, int y, Textures textures, int textureExpansionAmount = 1) : base(x, y, GetTexture(textures.Get(TextureName), textureExpansionAmount))
    {
    }
    protected override double Damage => EntityDamage;
    protected override bool IsSomeoneInTrap(out Entity? entity)
    {
        entity = null;
        if (IsExactCollision(out Sprite? sprite, [typeof(Player)]))
            entity = sprite as Entity;
        else return false;
        return true;
    }
    private static RGB?[,] GetTexture(RGB?[,] baseTexture, int amount)
    {
        return Textures.CreateExpansion(baseTexture, Direction.Right, amount);
    }
}