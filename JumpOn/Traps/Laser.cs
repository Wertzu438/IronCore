using JumpOn.Entities;
using JumpOn.Interactables;
using MCR_Engine.Core;
using MCR_Engine.Objects;

namespace JumpOn.Traps;

public class Laser : Trap
{
    private const double EntityDamage = 2;
    private const string LaserDeviceTextureName = "laserDevice", RayTextureName = "laserPart";
    private readonly TrapChildObject _laser;
    private readonly Clock _clock;
    private readonly RGB?[,] _rayPartTexture;
    private readonly int _onMs, _offMs;
    private int _lastMsChanged;
    public Laser(int x, int y, int onMs, int offMs, Textures textures, Clock clock) : base(x, y, textures.Get(LaserDeviceTextureName))
    {
        _laser = new TrapChildObject(RayX, RayY, textures.Get(RayTextureName));
        _laser.ChangeVisibility();
        _clock = clock;
        _rayPartTexture = textures.Get(RayTextureName);
        _onMs = onMs;
        _offMs = offMs;
        _lastMsChanged = 0;
    }
    protected override double Damage => EntityDamage;
    private int RayX => X + (Width / 2);
    private int RayY => Y + Height;
    private RGB?[,] RayTexture => Textures.CreateExpansion(_rayPartTexture, Direction.Down, _laser.GetDistanceToNextSprite(Direction.Down, false, true, [typeof(SolidObject), typeof(Ladder)]) + 1);
    public override Sprite? Clone(Engine engine, SpriteManager sprites) => new Laser(X, Y, _onMs, _offMs, engine.Textures, _clock);
    protected override void Update()
    {
        if (!Sprites.Contains(_laser))
            Sprites.Add(_laser);
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
        if (_laser.Visible && _laser.IsExactCollision(out Sprite? sprite, [typeof(Player)]))
            entity = sprite as Entity;
        else return false;
        return true;
    }
    private int GetTime() => _laser.Visible ? _onMs : _offMs;
}