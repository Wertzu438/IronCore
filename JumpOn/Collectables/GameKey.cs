using MCR_Engine.Core;
using MCR_Engine.Objects;

namespace JumpOn.Collectables;

public class GameKey : Collectable
{
    private const string TextureName = "key";
    public GameKey(int x, int y, Textures textures) : base(x, y, textures.Get(TextureName))
    {
    }
    private GameKey(int x, int y, Textures textures, bool collected) : base(x, y, textures.Get(TextureName))
    {
        Collected = collected;
        ChangeVisibility(!Collected);
    }
    public override void Update()
    {
        if (IsPlayerTouching(out _) && !Collected)
        {
            IsCollected = true;
        }
    }
    public override Sprite? Clone(Engine engine, SpriteManager sprites) => new GameKey(X, Y, engine.Textures, Collected);
}