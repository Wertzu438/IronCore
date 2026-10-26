using JumpOn.Entities;
using MCR_Engine.Core;
using MCR_Engine.Objects;

namespace JumpOn.Interactables;

public abstract class Interactable : Sprite
{
    private readonly bool _ignoreHalfCoverage;
    public Interactable(int x, int y, RGB?[,] texture, bool ignoreHalfCoverage) : base(x, y, texture, 0, false, false)
    {
        _ignoreHalfCoverage = ignoreHalfCoverage;
    }
    protected virtual bool IsEntityInteracting(Entity entity)
    {
        if (!Visible || !IsExactCollision(entity))
            return false;
        return _ignoreHalfCoverage || EntityCoveringHalf(entity);
    }
    private bool EntityCoveringHalf(Entity entity)
    {
        int overlap =
            Math.Min(entity.X + entity.Width, X + Width) -
            Math.Max(entity.X, X);

        return overlap >= entity.Width / 1.9;
    }
    public static void Update(List<Interactable> interactables, List<Entity> entities)
    {
        foreach (Interactable interactable in interactables)
        {
            foreach (Entity entity in entities)
            {
                if (interactable.IsEntityInteracting(entity))
                    entity.SetInteractable(interactable);
            }
        }
            
    }
    public static List<Interactable> LoadInteractables(SpriteManager sprites) => sprites.GetAllSpritesOfType<Interactable>();
}