using JumpOn.Entities;
using JumpOn.Traps;
using MCR_Engine.Core;
using MCR_Engine.Objects;

namespace JumpOn.Collectables;

public abstract class Collectable : Sprite
{
    protected bool Collected;
    protected Collectable(int x, int y, RGB?[,] texture) : base(x, y, texture, 3, false, true)
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
        if (IsBordersCollision(out Sprite? sprite, [typeof(Player)]))
        {
            player = sprite as Player;
            return true;
        }
        player = null;
        return false;
    }
    public static void Update(List<Collectable> collectables)
    {
        foreach (Collectable collectable in collectables)
            collectable.Update();
    }
    public static bool AreAllCollected<T>(List<T> collectables) where T : Collectable
    {
        foreach (Collectable collectable in collectables)
            if (!collectable.Collected)
                return false;
        return true;
    }
    public static List<Collectable> LoadCollectables(SpriteManager sprites) => sprites.GetAllSpritesOfType<Collectable>();
}