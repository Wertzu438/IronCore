using JumpOn.Entities;
using MCR_Engine.Core;
using MCR_Engine.Objects;

namespace JumpOn.Traps;

public class ArrowMachine : Trap
{
    private const double EntityDamage = 1.0, ArrowSpeed = 30;
    private const string ArrowDeviceTextureName = "arrowDevice", ArrowTextureName = "arrow";
    private readonly RGB?[,] _arrowTexture; 
    private readonly Clock _clock;
    private readonly List<TrapChildObject> _arrows;
    private readonly Direction _direction;
    private readonly double _arrowOffset;
    private int _lastMsShot;
    public ArrowMachine(int x, int y, Direction direction, double arrowOffset, Textures textures, Clock clock) : base(x, y, GetDeviceTexture(textures, direction))
    {
        _arrows = [];
        _direction = direction;
        _arrowOffset = arrowOffset;
        _clock = clock;
        _lastMsShot = 0;
        _arrowTexture = ShootRight ? textures.Get(ArrowTextureName) : Textures.MirrorLeftRight(textures.Get(ArrowTextureName));
    }
    protected override double Damage => EntityDamage;
    private int ArrowStartX => (ShootRight ? X + Width : X - Width);
    private int ArrowStartY => Y;
    private bool ShootRight => _direction is Direction.Right;
    protected override void Update()
    {
        foreach (TrapChildObject arrow in _arrows)
        {
            if (arrow.Visible && arrow.OnScene() && arrow.GetDistanceToNextSprite(_direction, false, false, [typeof(SolidObject), typeof(DecorationObject)]) != 0)
            {
                arrow.SetNewVelocity(_direction, ArrowSpeed);
                continue;
            }
            arrow.StopMovingX(); arrow.StopMovingY();
            arrow.ChangeVisibility(false);
        }
        if (_lastMsShot < _clock.PassedMilliSeconds - _arrowOffset)
        {
            _lastMsShot = _clock.PassedMilliSeconds;
            ShootArrow();
        }
        base.Update();
    }
    protected override bool IsSomeoneInTrap(out Entity? entity)
    {
        foreach (TrapChildObject arrow in _arrows)
        {
            if (!arrow.Visible || !arrow.IsExactCollision(out Sprite? sprite, [typeof(Player)]))
                continue;
            arrow.ChangeVisibility(false);
            entity = sprite as Entity;
            return true;
        }
        entity = null;
        return false;
    }
    private void ShootArrow()
    {
        for (int i = 0; i < _arrows.Count; i++)
        {
            if (_arrows[i].Move)
                continue;
            _arrows[i].X = ArrowStartX; _arrows[i].Y = ArrowStartY;
            _arrows[i].ChangeVisibility(true);
            _arrows[i].SetNewVelocity(_direction, ArrowSpeed);
            return;
        }
        TrapChildObject arrow = new TrapChildObject(ArrowStartX, ArrowStartY, _arrowTexture);
        arrow.ChangeVisibility(true);
        arrow.SetNewVelocity(_direction, ArrowSpeed);
        Sprites.Add(arrow);
        _arrows.Add(arrow);
    }
    private static RGB?[,] GetDeviceTexture(Textures textures, Direction direction) => direction is Direction.Right ? textures.Get(ArrowDeviceTextureName) : Textures.MirrorLeftRight(textures.Get(ArrowDeviceTextureName));
}