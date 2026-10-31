using JumpOn.Collectables;
using JumpOn.Entities;
using JumpOn.Entities.LivingEntities;
using MCR_Engine.Core;
using MCR_Engine.Objects;

namespace JumpOn.CheckPoints;

public class CheckPoint : Collectable
{
    private const string ActiveTextureName = "checkPoint_2", InactiveTextureName = "checkPoint_1";
    private readonly RGB?[,] _activeTexture;
    private readonly RGB?[,] _inactiveTexture;
    private CheckPointManager? _checkPoints;
    public CheckPoint(int x, int y, Textures textures) : base(x, y, textures.Get(InactiveTextureName))
    {
        _activeTexture = textures.Get(ActiveTextureName);
        _inactiveTexture = textures.Get(InactiveTextureName);
    }
    private CheckPoint(int x, int y, RGB?[,] activeTexture, RGB?[,] inactiveTexture, bool collected) : base(x, y, inactiveTexture)
    {
        _activeTexture = activeTexture;
        _inactiveTexture = inactiveTexture;
        Collected = collected;
        if (Collected)
            SetNewTexture(_activeTexture);
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
    public override GameObject? Clone(ObjectManager objects) => new CheckPoint(X, Y, _activeTexture, _inactiveTexture, Collected);
}