using JumpOn.Entities;
using MCR_Engine.Core;
using MCR_Engine.Objects;

namespace JumpOn.Traps;

public abstract class Trap : Sprite
{
    protected Trap(int x, int y, RGB?[,] texture) : base(x, y, texture, 0, false, false)
    {
    }
    protected abstract double Damage { get; }
    protected virtual void Update()
    {
        GiveEntityDamage();
    }
    protected abstract bool IsSomeoneInTrap(out Entity? entity);
    private void GiveEntityDamage()
    {
        if (!IsSomeoneInTrap(out Entity? entity))
            return;
        entity!.Damage(Damage, false);
    }
    public static void Update(List<Trap> traps)
    {
        foreach (Trap trap in traps)
            trap.Update();
    }
    public static List<Trap> LoadTraps(SpriteManager sprites) => sprites.GetAllSpritesOfType<Trap>();
}