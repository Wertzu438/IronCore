using JumpOn.Entities;
using MCR_Engine.Core;
using MCR_Engine.ObjectSystem;
using MCR_Engine.ObjectSystem.BaseObjectTypes;

namespace JumpOn.Traps;

public abstract class Trap : NonSolidObject
{
    private readonly bool _criticalDamage;
    protected Trap(int x, int y, RGB?[,] texture, bool criticalDamage) : base(x, y, 1, false, texture)
    {
        _criticalDamage = criticalDamage;
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
        entity!.Damage(Damage, _criticalDamage, true);
    }
    public static void Update(Trap[] traps)
    {
        foreach (Trap trap in traps)
            trap.Update();
    }
    public static Trap[] LoadTraps(Objects objects) => objects.GetAllObjectsOfType<Trap>();
}