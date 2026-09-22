using JumpOn.Entities;
using MCR_Engine.Core;
using MCR_Engine.Objects;

namespace JumpOn.Interactables;

public abstract class Interactable : Sprite
{
    public Interactable(int x, int y, RGB?[,] texture) : base(x, y, texture, 0, false, false)
    {
        
    }
    protected virtual bool IsEntityInteracting(out Entity? entity)
    {
        entity = null;
        if (!Visible || !IsCollided(out Sprite? sprite, [typeof(Entity)]))
            return false;
        entity = sprite as Entity;
        return true;
    }
}