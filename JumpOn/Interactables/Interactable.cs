using JumpOn.Entities;
using JumpOn.Entities.LivingEntities;
using MCR_Engine.Core;
using MCR_Engine.Core.BaseObjectTypes;
using MCR_Engine.Objects;

namespace JumpOn.Interactables;

public abstract class Interactable : GameObject
{
    private readonly bool _ignoreHalfCoverage;
    public Interactable(int x, int y, RGB?[,] texture, bool ignoreHalfCoverage) : base(x, y, texture, 0, false, false)
    {
        _ignoreHalfCoverage = ignoreHalfCoverage;
    }
    protected virtual bool IsObjectInteracting(GameObject gameObject)
    {
        if (!Visible || !IsExactCollision(gameObject))
            return false;
        return _ignoreHalfCoverage || ObjectCoveringHalf(gameObject);
    }
    private bool ObjectCoveringHalf(GameObject gameObject)
    {
        int overlap =
            Math.Min(gameObject.X + gameObject.Width, X + Width) -
            Math.Max(gameObject.X, X);

        return overlap >= gameObject.Width / 1.9;
    }
    public static void Update(List<Interactable> interactables, List<Entity> entities)
    {
        foreach (Interactable interactable in interactables)
        {
            foreach (Entity entity in entities)
            {
                if (entity is not LivingEntity)
                    continue;
                if (interactable.IsObjectInteracting(entity))
                    ((LivingEntity)entity).SetInteractable(interactable);
            }
        }
            
    }
    public static List<Interactable> LoadInteractables(Objects objects) => objects.GetAllObjectsOfType<Interactable>();
}