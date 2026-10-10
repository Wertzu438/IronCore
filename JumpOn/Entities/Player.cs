using JumpOn.Interactables;
using MCR_Engine.Core;
using MCR_Engine.InputSystem;
using MCR_Engine.Objects;

namespace JumpOn.Entities;

public class Player : Entity
{
    private enum PlayerAction
    {
        StandRight, StandLeft, Hang, Climb, WalkLeft, WalkRight
    }
    public const double MaxHealth = 5;
    private readonly Key[] _movementKeys;
    private readonly RGB?[][,] _standingTextures;
    private readonly RGB?[][,] _walkingTextures;
    private readonly RGB?[][,] _climbingTextures;
    private readonly RGB?[][,] _hangingTextures;
    private (int LastCoordinateChanged, int Index) _textureInfo;
    private int _respawns;
    private PlayerAction _action;
    public Player(int x, int y, Textures textures, Input input, Clock clock) : base(x, y, Textures.DebuggingTexture(1, 1), 5, clock)
    {
        _movementKeys = [input.Get("A"), input.Get("D"), input.Get("Space"), input.Get("W"), input.Get("S")];
        _standingTextures = [Textures.MirrorLeftRight(textures.Get("playerStand")), textures.Get("playerStand")];
        _walkingTextures =
        [
            textures.Get("playerWalk_1"), textures.Get("playerWalk_2"),
            Textures.MirrorLeftRight(textures.Get("playerWalk_1")),
            Textures.MirrorLeftRight(textures.Get("playerWalk_2")),
        ];
        _climbingTextures = [textures.Get("playerLadder_1"), textures.Get("playerLadder_2")];
        _hangingTextures = [textures.Get("playerBar_1"), textures.Get("playerBar_2")];
        _action = PlayerAction.StandRight;
        _textureInfo = (0, 0);
        _respawns = 0;
    }
    public int Respawns
    {
        get => _respawns;
        set => _respawns = (value > _respawns ? value : throw new ArgumentException("Value must be greater than respawns."));
    }
    protected override double WalkingSpeed => 40;
    protected override double ClimbingSpeed => 20;
    protected override double HangingSpeed => 20;
    public override void Update()
    {
        if (!Alive)
            return;
        if (GetFallDamage(out double fallDamage))
            Damage(fallDamage, true);
        _action = GetAction(GetDirection());
        if (Jump())
            SetNewVelocity(Direction.Up, 100);
        SetNewTexture(GetNewTexture());
    }
    public override Sprite? Clone(Engine engine, SpriteManager sprites) => null;
    private Direction GetDirection()
    {
        if (_movementKeys[0].Pressed)
            return Direction.Left;
        if (_movementKeys[1].Pressed)
            return Direction.Right;
        if (_movementKeys[3].Pressed)
            return Direction.Up;
        if (_movementKeys[4].Pressed)
            return Direction.Down;
        return Direction.None;
    }
    private PlayerAction GetAction(Direction direction)
    {
        PlayerAction action;
        if (Hang(direction))
        {
            action = PlayerAction.Hang;
        }
        else if (Climb(direction))
        {
            action = PlayerAction.Climb;
        }
        else if (Walk(direction))
        {
            action = direction is Direction.Left ? PlayerAction.WalkLeft : PlayerAction.WalkRight;
        }
        else if ((_action is PlayerAction.WalkLeft or PlayerAction.WalkRight && !Move) || (_action is PlayerAction.Hang))
        {
            action = (_action is PlayerAction.WalkLeft ? PlayerAction.StandLeft : PlayerAction.StandRight);
        }
        else return _action;
        return action;
    }
    private RGB?[,] GetNewTexture()
    {
        if (_action is PlayerAction.StandLeft)
            return _standingTextures[1];
        if (_action is PlayerAction.StandRight)
            return _standingTextures[0];
        if (_action is PlayerAction.WalkRight)
            return _walkingTextures[(_walkingTextures.Length / 2) + SetTextureIndex(_walkingTextures.Length / 2, X)];
        if (_action is PlayerAction.WalkLeft)
            return _walkingTextures[SetTextureIndex(_walkingTextures.Length / 2, X)];
        if (_action is PlayerAction.Climb)
            return _climbingTextures[SetTextureIndex(_climbingTextures.Length, Y)];
        if (_action is PlayerAction.Hang)
            return _hangingTextures[SetTextureIndex(_hangingTextures.Length, X)];
        throw new NotImplementedException();
    }
    private int SetTextureIndex(int textureCount, int coordinate)
    {
        if (!ChangeTexture(coordinate))
            return _textureInfo.Index;
        return _textureInfo.Index = (_textureInfo.Index + 1 >= textureCount ? 0 : _textureInfo.Index + 1);
    }
    private bool ChangeTexture(int coordinate)
    {
        if (coordinate % 5 != 0 || _textureInfo.LastCoordinateChanged == coordinate)
            return false;
        _textureInfo.LastCoordinateChanged = coordinate;
        return true;
    }
    private bool Jump() => (Input.IsOnePressed([_movementKeys[2]]) && IsFloorBeneath()) && _action is not (PlayerAction.Climb or PlayerAction.Hang);
}