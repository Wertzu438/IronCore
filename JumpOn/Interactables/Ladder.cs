using JumpOn.Entities;
using MCR_Engine.Core;
using MCR_Engine.Objects;

namespace JumpOn.Interactables;

public class Ladder : Interactable
{
    private const string TextureName = "ladderPart";
    public Ladder(int x, int y, Textures textures, int partsCount) : base(x, y, Textures.DebuggingTexture(1, 1))
    {
        SetNewTexture(CreateTexture(textures, partsCount));
    }
    public void CenterEntity(Entity entity)
    {
        int offset = (Width - entity.Width) / 2;
        if (offset < 0)
            throw new ArgumentException("offset < 0");
        entity.X = X + offset;
    }
    public static bool IsEntityOnTopOfLadder(Entity entity)
    {
        int distance = entity.GetDistanceToNextSprite(Direction.Down, false, true, [typeof(Ladder)]);
        return distance == entity.Height - 1 || distance == entity.Height - 2;
    }
    private static RGB?[,] CreateTexture(Textures textures, int amount) => Textures.CreateExpansion(textures.Get(TextureName), Direction.Down, amount);
}