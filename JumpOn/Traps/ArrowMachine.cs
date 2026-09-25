using JumpOn.Entities;
using MCR_Engine.Core;
using MCR_Engine.Objects;

namespace JumpOn.Traps;

public class ArrowMachine : Trap
{
    private const double ArrowSpeed = 1.5;
    private const string ArrowDeviceTextureName = "arrowDevice", ArrowTextureName = "arrowTexture";
    private readonly TrapChildObject[] _arrows;
    private readonly Clock _clock;
    private readonly Direction _direction;
    private readonly int _msShootOffset;
    private int _lastMsShot;
    
    public ArrowMachine(int x, int y, int msShootOffset, Direction direction, Textures textures, Clock clock) : base(x, y, textures.Get(ArrowDeviceTextureName))
    {
        if (!(direction is Direction.Left or Direction.Right))
            throw new ArgumentException("Direction must be a left or right direction.", nameof(direction));
        _arrows = new TrapChildObject[4];
        _clock = clock;
        _direction = direction;
        _msShootOffset = msShootOffset;
        _lastMsShot = 0;
    }
    protected override double Damage => 0.8;
    private int ArrowStartX => (ShootRight ? X + Width : X - Width);
    private double MovingSpeed => (ShootRight ? ArrowSpeed : -ArrowSpeed);
    private bool ShootRight => _direction == Direction.Right;
    protected override void Update()
    {
        foreach (var arrow in _arrows)
        {
            if (arrow.Move)
                continue;
            arrow.ChangeVisibility();
            arrow.X = ArrowStartX;
        }
        if (_lastMsShot < _clock.PassedMilliSeconds - _msShootOffset)
        {
            _lastMsShot = _clock.PassedMilliSeconds;
            ShootArrow();
        }
        base.Update();
    }
    protected override bool IsSomeoneInTrap(out Entity? entity)
    {
        entity = null;
        foreach (TrapChildObject arrow in _arrows)
        {
            if (!arrow.Visible)
                continue;
            if (arrow.IsCollided(out Sprite? sprite, [typeof(Player)]))
            {
                entity = sprite as Entity;
                return true;
            }
        }
        return false;
    }
    private void ShootArrow()
    {
        foreach (var arrow in _arrows)
        {
            if (arrow.Move)
            {
                SetNewVelocity(_direction, MovingSpeed);
                if (!arrow.IsCollided(out _))
                    continue;
            }
            arrow.ChangeVisibility(true);
        }
    }
}