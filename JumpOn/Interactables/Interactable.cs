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
        if (!IsCollided(out Sprite? sprite, [typeof(Player)]))
            return false;
        entity = sprite as Entity;
        return true;
    }
    public static void Update(List<Interactable> interactables)
    {
        foreach (Interactable interactable in interactables)
            if (interactable.IsEntityInteracting(out Entity? entity))
                entity!.SetInteractable(interactable);
    }
    public static List<Interactable> LoadInteractables(SpriteManager sprites) => sprites.GetAllSpritesOfType<Interactable>();
}