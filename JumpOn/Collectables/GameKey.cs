using MCR_Engine.Core;

namespace JumpOn.Collectables;

public class GameKey : Collectable
{
    private const string TextureName = "key";
    public GameKey(int x, int y, Textures textures) : base(x, y, textures.Get(TextureName))
    {
        
    }
    public override void Update()
    {
        if (IsPlayerTouching(out _) && !IsCollected)
        {
            IsCollected = true;
        }
    }
}