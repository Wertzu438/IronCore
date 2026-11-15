using JumpOn.Entities;
using JumpOn.Entities.LivingEntities;
using JumpOn.Entities.OtherEntities;
using JumpOn.Interactables;
using MCR_Engine.Core;
using MCR_Engine.ObjectSystem;
using SolidObject = MCR_Engine.ObjectSystem.BaseObjectTypes.SolidObject;

namespace JumpOn.Traps;

public class LaserDevice : Trap
{
    private const double EntityDamage = 3;
    private const string LaserDeviceTextureName = "laserDevice", RayTextureName = "laserPart";
    private readonly Ray _ray;
    private readonly Clock _clock;
    private readonly RGB?[,] _rayPartTexture;
    private readonly RGB?[,] _deviceTexture;
    private readonly int _onMs, _offMs;
    private int _lastMsChanged;
    public LaserDevice(int x, int y, int onMs, int offMs, Textures textures, Clock clock) : base(x, y, textures.Get(LaserDeviceTextureName), false)
    {
        _ray = new Ray(RayX, RayY, textures.Get(RayTextureName));
        _ray.ChangeVisibility();
        _clock = clock;
        _rayPartTexture = textures.Get(RayTextureName);
        _deviceTexture = textures.Get(LaserDeviceTextureName);
        _onMs = onMs;
        _offMs = offMs;
        _lastMsChanged = 0;
    }
    private LaserDevice(int x, int y, int onMs, int offMs, RGB?[,] deviceTexture, RGB?[,] rayPartTexture, Clock clock) : base(x, y, deviceTexture, false)
    {
        _clock = clock;
        _onMs = onMs;
        _offMs = offMs;
        _deviceTexture = deviceTexture;
        _rayPartTexture = rayPartTexture;
        _ray = new Ray(RayX, RayY, _rayPartTexture);
        _ray.ChangeVisibility();
        _lastMsChanged = 0;
    }
    protected override double Damage => EntityDamage;
    private int RayX => X + (Width / 2);
    private int RayY => Y + Height;
    private int RayHeight => _ray.GetDistanceToNextObject(Direction.Down, false, [typeof(SolidObject), typeof(Ladder), typeof(Box), typeof(Scene)]) + 1;
    public override GameObject? Clone(Objects objects) => new LaserDevice(X, Y, _onMs, _offMs, _deviceTexture, _rayPartTexture, _clock);
    protected override void Update()
    {
        ArgumentNullException.ThrowIfNull(Objects);
        if (!Objects.Contains(_ray))
            Objects.Add(_ray);
        if (_lastMsChanged < _clock.PassedMilliSeconds - GetTime())
        {
            _ray.UpdateTexture(RayHeight);
            _lastMsChanged = _clock.PassedMilliSeconds;
            _ray.ChangeVisibility();
        }
        base.Update();
    }
    protected override bool IsSomeoneInTrap(out Entity? entity)
    {
        entity = null;
        if (_ray.Visible && _ray.IsExactCollision(out GameObject? sprite, [typeof(Player)]))
            entity = sprite as Entity;
        else return false;
        return true;
    }
    private int GetTime() => _ray.Visible ? _onMs : _offMs;
}