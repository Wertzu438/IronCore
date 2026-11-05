using JumpOn.Entities;
using JumpOn.Entities.LivingEntities;
using JumpOn.UI.InfoBoxes;
using MCR_Engine.Core;
using MCR_Engine.InputSystem;
using MCR_Engine.Objects;
using MCR_Engine.Rendering.Output;

namespace JumpOn.Interactables;

public class Switch : Interactable
{
    private const double WaitingIntervalMs = 500;
    private const string TextureName = "switch", KeyName = "E";
    private readonly MobileInfoBox _infoBox;
    private readonly RGB?[,] _pulledTexture;
    private readonly RGB?[,] _unpulledTexture;
    private readonly Key _interactingKey;
    private readonly GameObject _connectedGameObject;
    private readonly Camera _camera;
    private readonly Clock _clock;
    private int _lastMsPulled;
    private bool _pulled;
    public Switch(int x, int y, GameObject connectedGameObject, Input input, Camera camera, MobileInfoBox infoBox, Textures textures, Clock clock) : base(x, y, textures.Get(TextureName), true)
    {
        _infoBox = infoBox;
        _unpulledTexture = textures.Get(TextureName);
        _pulledTexture = Textures.MirrorLeftRight(textures.Get(TextureName));
        _interactingKey = input.Get(KeyName);
        _connectedGameObject = connectedGameObject;
        _camera = camera;
        _clock = clock;
        _lastMsPulled = 0;
        _pulled = false;
    }
    private Switch(int x, int y, GameObject connectedGameObject, Key interactingKey, Camera camera, MobileInfoBox infoBox, RGB?[,] pulledTexture, RGB?[,] unpulledTexture, Clock clock) : base(x, y, unpulledTexture, true)
    {
        _connectedGameObject = connectedGameObject;
        _interactingKey = interactingKey;
        _camera = camera;
        _infoBox = infoBox;
        _pulledTexture = pulledTexture;
        _unpulledTexture = unpulledTexture;
        _clock = clock;
        _lastMsPulled = 0;
        _pulled = false;
    }
    public override GameObject? Clone(Objects objects) => new Switch(X, Y, objects.Get(objects.Count - 1), _interactingKey, _camera, _infoBox, _pulledTexture, _unpulledTexture, _clock);
    protected override bool IsObjectInteracting(GameObject gameObject)
    {
        bool playerInteracting = base.IsObjectInteracting(gameObject) && gameObject is Player;
        UpdateInfoBox(playerInteracting);
        if (!playerInteracting || !_interactingKey.Pressed || _lastMsPulled + WaitingIntervalMs > _clock.PassedMilliSeconds)
            return false;
        SwipeCameraOnConnectedObject();
        _lastMsPulled = _clock.PassedMilliSeconds;
        _pulled = !_pulled;
        SetNewTexture(GetCurrentTexture());
        return true;
    }
    private void UpdateInfoBox(bool playerInteracting)
    {
        if (playerInteracting)
        {
            _infoBox.Show(this, X + 5, Y / 2 - 2, 14, 1, [[$"!{KeyName} - Benutzen"]]);
        }
        else if (_infoBox.UsedBy == this)
        {
            _infoBox.UsedBy = null;
        }
    }
    private RGB?[,] GetCurrentTexture() => _pulled ? _pulledTexture : _unpulledTexture;
    private async Task SwipeCameraOnConnectedObject()
    {
        if (_camera.CinematicCamera)
            return;
        GameObject lastGameObject = _camera.FollowedObject;
        await _camera.SetFollowedObject(_connectedGameObject, 0, 0);
        _camera.CinematicCamera = true;
        await Task.Delay(300); 
        _connectedGameObject.ChangeVisibility(); 
        await Task.Delay(300); 
        _camera.CinematicCamera = false;
        await _camera.SetFollowedObject(lastGameObject, 0, 0);
    }
}