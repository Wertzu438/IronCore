using JumpOn.Entities;
using JumpOn.Traps;
using MCR_Engine.Core;
using MCR_Engine.Objects;

namespace JumpOn.Collectables;

public abstract class Collectable : Sprite
{
    private bool _isCollected;
    protected Collectable(int x, int y, RGB?[,] texture) : base(x, y, texture, 0, false, true)
    {
        
    }
    public bool IsCollected
    {
        protected get => _isCollected;
        set
        {
            _isCollected |= value;
            ChangeVisibility(!_isCollected);
        }
    }
    public abstract void Update();
    protected bool IsPlayerTouching(out Player? player)
    {
        if (IsCollided(out Sprite? sprite, [typeof(Player)]))
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
    public static List<Collectable> LoadCollectables(SpriteManager sprites) => sprites.GetAllSpritesOfType<Collectable>();
}