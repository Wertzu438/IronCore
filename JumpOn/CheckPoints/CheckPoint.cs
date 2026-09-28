using JumpOn.Collectables;
using JumpOn.Entities;
using MCR_Engine.Core;

namespace JumpOn.CheckPoints;

public class CheckPoint : Collectable
{
    private const string ActiveTextureName = "checkPoint_2", InactiveTextureName = "checkPoint_1";
    private readonly RGB?[,] _activeTexture;
    public CheckPoint(int x, int y, Textures textures) : base(x, y, textures.Get(InactiveTextureName))
    {
        _activeTexture = textures.Get(ActiveTextureName);
    }
    public override bool IsCollected
    {
        get => base.IsCollected;
        set 
        {
            base.IsCollected = value;
            ChangeVisibility(true);
        }
    }
    public override void Update()
    {
        if (!IsPlayerTouching(out Player? player))
            return;
        IsCollected = true;
        SetNewTexture(_activeTexture);
    }
}