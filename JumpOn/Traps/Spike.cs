using JumpOn.Entities;
using MCR_Engine.Core;
using MCR_Engine.Objects;

namespace JumpOn.Traps;

public class Spike : Trap
{
    private const double EntityDamage = 1.2;
    private const string TextureName = "spike";
    public Spike(int x, int y, Textures textures) : base(x, y, textures.Get(TextureName))
    {
    }
    protected override double Damage => EntityDamage;
    protected override bool IsSomeoneInTrap(out Entity? entity)
    {
        entity = null;
        if (IsCollided(out Sprite? sprite, [typeof(Player)]))
            entity = sprite as Entity;
        else return false;
        return true;
    }
}