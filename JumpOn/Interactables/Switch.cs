using JumpOn.Entities;
using MCR_Engine.Core;
using MCR_Engine.InputSystem;
using MCR_Engine.Objects;
using MCR_Engine.Rendering.Output;

namespace JumpOn.Interactables;

public class Switch : Interactable
{
    private const double WaitingIntervalMs = 500;
    private const string TextureName = "switch", KeyName = "E";
    private readonly RGB?[][,] _textures;
    private readonly Key _interactingKey;
    private readonly Sprite _connectedSprite;
    private readonly Camera _camera;
    private readonly Clock _clock;
    private int _lastMsPulled;
    private bool _pulled;
    public Switch(int x, int y, Sprite connectedSprite, Input input, Camera camera, Textures textures, Clock clock) : base(x, y, textures.Get(TextureName))
    {
        _textures = [textures.Get(TextureName), Textures.MirrorLeftRight(textures.Get(TextureName))];
        _interactingKey = input.GetKey(KeyName);
        _connectedSprite = connectedSprite;
        _camera = camera;
        _clock = clock;
        _lastMsPulled = 0;
        _pulled = false;
    }
    protected override bool IsEntityInteracting(out Entity? entity)
    {
        if (!base.IsEntityInteracting(out entity) || entity as Player is null || !_interactingKey.Pressed || _lastMsPulled + WaitingIntervalMs > _clock.PassedMilliSeconds)
            return false;
        _connectedSprite.ChangeVisibility();
        _lastMsPulled = _clock.PassedMilliSeconds;
        _pulled = !_pulled;
        SetNewTexture(GetCurrentTexture());
        return true;
    }
    private RGB?[,] GetCurrentTexture() => _pulled ? _textures[0] : _textures[1];
}