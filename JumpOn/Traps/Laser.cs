using JumpOn.Entities;
using JumpOn.Entities.LivingEntities;
using JumpOn.Entities.OtherEntities;
using JumpOn.Interactables;
using MCR_Engine.Core;
using MCR_Engine.ObjectSystem;
using SolidObject = MCR_Engine.Core.BaseObjectTypes.SolidObject;

namespace JumpOn.Traps;

public class Laser : Trap
{
    private const double EntityDamage = 3;
    private const string LaserDeviceTextureName = "laserDevice", RayTextureName = "laserPart";
    private readonly TrapChildObject _laser;
    private readonly Clock _clock;
    private readonly RGB?[,] _rayPartTexture;
    private readonly RGB?[,] _deviceTexture;
    private readonly int _onMs, _offMs;
    private int _lastMsChanged;
    public Laser(int x, int y, int onMs, int offMs, Textures textures, Clock clock) : base(x, y, textures.Get(LaserDeviceTextureName), false)
    {
        _laser = new TrapChildObject(RayX, RayY, textures.Get(RayTextureName));
        _laser.ChangeVisibility();
        _clock = clock;
        _rayPartTexture = textures.Get(RayTextureName);
        _deviceTexture = textures.Get(LaserDeviceTextureName);
        _onMs = onMs;
        _offMs = offMs;
        _lastMsChanged = 0;
    }
    private Laser(int x, int y, int onMs, int offMs, RGB?[,] deviceTexture, RGB?[,] rayPartTexture, Clock clock) : base(x, y, deviceTexture, false)
    {
        _clock = clock;
        _onMs = onMs;
        _offMs = offMs;
        _deviceTexture = deviceTexture;
        _rayPartTexture = rayPartTexture;
        _laser = new TrapChildObject(RayX, RayY, _rayPartTexture);
        _laser.ChangeVisibility();
        _lastMsChanged = 0;
    }
    protected override double Damage => EntityDamage;
    private int RayX => X + (Width / 2);
    private int RayY => Y + Height;
    private RGB?[,] RayTexture => 
        Textures.CreateExpansion(
            _rayPartTexture,
            Direction.Down, 
            _laser.GetDistanceToNextObject(Direction.Down, false, [typeof(SolidObject), typeof(Ladder), typeof(Box), typeof(Scene)]) + 1
        );
    public override GameObject? Clone(Objects objects) => new Laser(X, Y, _onMs, _offMs, _deviceTexture, _rayPartTexture, _clock);
    protected override void Update()
    {
        ArgumentNullException.ThrowIfNull(Objects);
        if (!Objects.Contains(_laser))
            Objects.Add(_laser);
        if (_lastMsChanged < _clock.PassedMilliSeconds - GetTime())
        {
            _laser.SetNewTexture(RayTexture);
            _lastMsChanged = _clock.PassedMilliSeconds;
            _laser.ChangeVisibility();
        }
        base.Update();
    }
    protected override bool IsSomeoneInTrap(out Entity? entity)
    {
        entity = null;
        if (_laser.Visible && _laser.IsExactCollision(out GameObject? sprite, [typeof(Player)]))
            entity = sprite as Entity;
        else return false;
        return true;
    }
    private int GetTime() => _laser.Visible ? _onMs : _offMs;
}