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
    public int CenterPlayer(int playerWidth)
    {
        int offset = (Width - playerWidth) / 2;
        if (offset < 0)
            throw new Exception("player ate too many burgers");
        return X + offset;
    }
    public bool OnTopOfIt(Entity entity) => entity.Y + entity.Height <= Y+1;
    private static RGB?[,] CreateTexture(Textures textures, int amount) =>
        Textures.CreateExpansion(textures.Get(TextureName), Direction.Down, amount);
}