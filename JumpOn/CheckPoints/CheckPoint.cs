using JumpOn.Collectables;
using JumpOn.Entities;
using MCR_Engine.Core;
using MCR_Engine.Objects;

namespace JumpOn.CheckPoints;

public class CheckPoint : Collectable
{
    private const string ActiveTextureName = "checkPoint_2", InactiveTextureName = "checkPoint_1";
    private readonly RGB?[,] _activeTexture;
    private CheckPointManager? _checkPoints;
    public CheckPoint(int x, int y, Textures textures) : base(x, y, textures.Get(InactiveTextureName))
    {
        _activeTexture = textures.Get(ActiveTextureName);
    }
    public CheckPointManager CheckPoints
    {
        set
        {
            if (_checkPoints is not null)
                throw new InvalidOperationException("CheckPoints cannot be set twice.");
            _checkPoints = value ?? throw new NullReferenceException("CheckPoints cannot be null.");
        }   
    }
    private CheckPoint(int x, int y, Textures textures, bool collected) : base(x, y, textures.Get(InactiveTextureName))
    {
        _activeTexture = textures.Get(ActiveTextureName);
        Collected = collected;
        if (Collected)
            SetNewTexture(_activeTexture);
    }
    public override bool IsCollected
    {
        get => base.IsCollected;
        set => Collected |= value;
    }
    public override void Update()
    {
        if (!IsPlayerTouching(out Player? player))
            return;
        Collected = true;
        SetNewTexture(_activeTexture);
        (_checkPoints ?? throw new NullReferenceException("Checkpoints was null.")).MakeSnapshot();
    }
    public override Sprite? Clone(Engine engine, SpriteManager sprites) => new CheckPoint(X, Y, engine.Textures, Collected);
}