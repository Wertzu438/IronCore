using System.Diagnostics;
using MCR_Engine.Core;
using MCR_Engine.Objects;

namespace JumpOn.Entities.LivingEntities;

public class TestEnemy : LivingEntity
{
    private PathFinding _pathFinding;
    private Player _player;
    private Player.PlayerAction _action;
    public TestEnemy(int x, int y, Textures textures, Clock clock, PathFinding pathFinding, Player player) : base(x, y, 10, clock, textures)
    {
        _pathFinding = pathFinding;
        _player = player;
    }
    protected override double WalkingSpeed => 25;
    protected override double ClimbingSpeed => 15;
    protected override double HangingSpeed => 40;
    protected override bool IsInteractingWithInteractable => _action is Player.PlayerAction.Climb;
    public override GameObject? Clone(Objects objects) => null;
    public override void Update()
    {
        (Direction d, PathType p) = _pathFinding.Find(this, _player);
        if (p is PathType.Walk && Walk(d))
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