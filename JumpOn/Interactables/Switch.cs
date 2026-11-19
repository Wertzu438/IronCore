using JumpOn.Entities;
using JumpOn.Entities.LivingEntities;
using JumpOn.UI.InfoBoxes;
using MCR_Engine.Core;
using MCR_Engine.InputSystem;
using MCR_Engine.ObjectSystem;
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
    private readonly GameObject _connectedObject;
    private readonly Camera _camera;
    private readonly Clock _clock;
    private int _lastMsPulled;
    private bool _pulled;
    public Switch(int x, int y, GameObject connectedObject, Input input, Camera camera, MobileInfoBox infoBox, Textures textures, Clock clock) : base(x, y, textures.Get(TextureName), true)
    {
        _infoBox = infoBox;
        _unpulledTexture = textures.Get(TextureName);
        _pulledTexture = Textures.MirrorLeftRight(textures.Get(TextureName));
        _interactingKey = input.Get(KeyName);
        _connectedObject = connectedObject;
        _camera = camera;
        _clock = clock;
        _lastMsPulled = 0;
        _pulled = false;
    }
    private Switch(int x, int y, GameObject connectedObject, Key interactingKey, Camera camera, MobileInfoBox infoBox, RGB?[,] pulledTexture, RGB?[,] unpulledTexture, Clock clock) : base(x, y, unpulledTexture, true)
    {
        _connectedObject = connectedObject;
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
            throw new ArgumentException("Cinematic camera not supported");
        await Task.Delay(1);
        bool inView = _camera.InView(_connectedObject.X + _connectedObject.Width / 2, (_connectedObject.Y + _connectedObject.Height / 2)/ 2);
        _camera.GoToCoordinates(_connectedObject.X + _connectedObject.Width / 2, _connectedObject.Y + _connectedObject.Height / 2);
        _camera.LockPosition = true;
        if (!inView)
            await Task.Delay(700);
        _connectedObject.ChangeVisibility();
        if (!inView)
            await Task.Delay(300);
        _camera.LockPosition = false;
    }
}