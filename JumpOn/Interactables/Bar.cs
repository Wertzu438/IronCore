using JumpOn.Entities;
using MCR_Engine.Core;

namespace JumpOn.Interactables;

public class Bar : Interactable
{
    private const string TextureName = "barPart";
    public Bar(int x, int y, Textures textures, int partsCount) : base(x, y, Textures.DebuggingTexture(1, 1))
    {
        SetNewTexture(CreateTexture(textures, partsCount));
    }
    public int HangOnIt() => Y;
    private static RGB?[,] CreateTexture(Textures textures, int amount) => 
        Textures.CreateExpansion(textures.Get(TextureName), Direction.Right, amount);
}