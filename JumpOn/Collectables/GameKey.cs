using MCR_Engine.Core;
using MCR_Engine.Objects;

namespace JumpOn.Collectables;

public class GameKey : Collectable
{
    private const string TextureName = "key";
    private readonly RGB?[,] _texture;
    public GameKey(int x, int y, Textures textures) : base(x, y, textures.Get(TextureName))
    {
        _texture = textures.Get(TextureName);
    }
    private GameKey(int x, int y, RGB?[,] texture, bool collected) : base(x, y, texture)
    {
        _texture = texture;
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
    public override GameObject? Clone(Objects objects) => new GameKey(X, Y, _texture, Collected);
}