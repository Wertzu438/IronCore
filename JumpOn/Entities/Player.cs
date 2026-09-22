using JumpOn.Interactables;
using MCR_Engine.Core;
using MCR_Engine.InputSystem;

namespace JumpOn.Entities;

public class Player : Entity
{
    private enum PlayerAction
    {
        StandRight, StandLeft, Hang, Climb, WalkLeft, WalkRight
    }
    private const double MaxHealth = 5;
    private readonly Key[] _movementKeys;
    private readonly RGB?[][,] _standingTextures;
    private readonly RGB?[][,] _walkingTextures;
    private readonly RGB?[][,] _climbingTextures;
    private readonly RGB?[][,] _hangingTextures;
    private PlayerAction _action;
    public Player(int x, int y, Textures textures, Input input, Clock clock) : base(x, y, Textures.DebuggingTexture(1, 1), 5, clock)
    {
        _movementKeys = [input.GetKey("A"), input.GetKey("D"), input.GetKey("Space"), input.GetKey("W"), input.GetKey("S")];
        _standingTextures = [Textures.MirrorLeftRight(textures.Get("playerStand")), textures.Get("playerStand")];
        _walkingTextures =
        [
            textures.Get("playerWalk_1"), textures.Get("playerWalk_2"),
            Textures.MirrorLeftRight(textures.Get("playerWalk_1")),
            Textures.MirrorLeftRight(textures.Get("playerWalk_2"))
        ];
        _climbingTextures = [textures.Get("playerLadder_1"), textures.Get("playerLadder_2")];
        _hangingTextures = [textures.Get("playerBar_1"), textures.Get("playerBar_2")];
        _action = PlayerAction.StandRight;
    }
    public override bool Gravity => !(OnLadder || OnBar);
    protected override double WalkingSpeed => 30;
    protected override double ClimbingSpeed => 0.4;
    protected override double HangingSpeed => 0.4;
    private bool OnTopOfLadder => OnLadder && ((Ladder)CurrentInteractable!).OnTopOfIt(this);
    public override void Update()
    {
        if (!Alive)
            return;
        _action =  GetAction(GetDirection());
        if (Jump())
            SetNewVelocity(Direction.Up, 75);
        SetNewTexture(GetNewTexture());
    }
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
        else action = (direction is Direction.Left ? PlayerAction.StandLeft : PlayerAction.StandRight);
        return action;
    }
    private RGB?[,] GetNewTexture()
    {
        if (_action is PlayerAction.StandLeft or PlayerAction.StandRight) 
            return _standingTextures[_action is PlayerAction.StandRight ? 0 : 1];
        if (_action is PlayerAction.WalkRight)
            return _walkingTextures[2];
        if (_action is PlayerAction.WalkLeft)
            return _walkingTextures[0];
        if (_action is PlayerAction.Climb)
            return _climbingTextures[0];
        if (_action is PlayerAction.Hang)
            return _hangingTextures[0];
        throw new NotImplementedException();
    }
    private bool Jump() => (Input.IsOnePressed([_movementKeys[2]]) && (OnTopOfLadder || (IsFloorBeneath() && Gravity)));
}