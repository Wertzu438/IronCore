using JumpOn.Entities;
using JumpOn.Entities.LivingEntities;
using JumpOn.Traps;
using MCR_Engine.Core;
using MCR_Engine.ObjectSystem;
using MCR_Engine.ObjectSystem.BaseObjectTypes;

namespace JumpOn.Collectables;

public abstract class Collectable : NonSolidObject
{
    protected bool Collected;
    protected Collectable(int x, int y, RGB?[,] texture) : base(x, y, 3, texture)
    {
    }
    public virtual bool IsCollected
    {
        get => Collected;
        set
        {
            Collected |= value;
            ChangeVisibility(!Collected);
        }
    }
    public abstract void Update();
    protected bool IsPlayerTouching(out Player? player)
    {
        if (IsBordersCollision(out GameObject? sprite, [typeof(Player)]))
        {
            player = sprite as Player;
            return true;
        }
        player = null;
        return false;
    }
    public static void Update(Collectable[] collectables)
    {
        foreach (Collectable collectable in collectables)
            collectable.Update();
    }
    public static int CollectedAmount<T>(T[] collectables) where T : Collectable
    {
        int sum = 0;
        foreach (T collectable in collectables)
            if (collectable.IsCollected)
                sum++;
        return sum;
    }
    public static bool AreAllCollected<T>(T[] collectables) where T : Collectable => CollectedAmount(collectables) == collectables.Length;
    public static Collectable[] LoadCollectables(Objects objects) => objects.GetAllObjectsOfType<Collectable>();
}