using JumpOn.Entities;
using MCR_Engine.Core;
using MCR_Engine.Objects;

namespace JumpOn.Traps;

public abstract class Trap : GameObject
{
    private readonly bool _criticalDamage;
    protected Trap(int x, int y, RGB?[,] texture, bool criticalDamage) : base(x, y, texture, 1, false, false)
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
    public static void Update(List<Trap> traps)
    {
        foreach (Trap trap in traps)
            trap.Update();
    }
    public static List<Trap> LoadTraps(ObjectManager objects) => objects.GetAllObjectsOfType<Trap>();
}