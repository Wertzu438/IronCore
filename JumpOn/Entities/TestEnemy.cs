using System.Diagnostics;
using MCR_Engine.Core;
using MCR_Engine.Objects;

namespace JumpOn.Entities;

public class TestEnemy : Entity
{
    private PathFinding _pathFinding;
    private Player _player;
    private Player.PlayerAction _action;
    public TestEnemy(int x, int y, Textures textures, Clock clock, PathFinding pathFinding, Player player) : base(x, y, textures, 10, clock)
    {
        _pathFinding = pathFinding;
        _player = player;
        //SetNewTexture(Textures.DebuggingTexture(4, 8));
    }
    protected override double WalkingSpeed => 25;
    protected override double ClimbingSpeed => 15;
    protected override double HangingSpeed => 40;
    protected override bool IsInteractingWithInteractable => _action is Player.PlayerAction.Climb;
    public override Sprite? Clone(SpriteManager sprites) => null;
    public override void Update()
    {
        Direction d = _pathFinding.Find(this, _player);
        if (Walk(d))
        {
            _action = Player.PlayerAction.WalkLeft;
        }
        else if (Climb(d))
        {
            _action = Player.PlayerAction.Climb;
        }
        Debug.WriteLine($"Direction: {d}; Action: {_action}");
    }
}