using JumpOn.Entities;
using JumpOn.UI.InfoBoxes;
using MCR_Engine.Core;
using MCR_Engine.InputSystem;
using MCR_Engine.Objects;

namespace JumpOn.Collectables;

public class Medkit : Collectable
{
    private const string KeyName = "E", TextureName = "medkit";
    private const double HealAmount = 2;
    private readonly Key _interactingKey;
    private readonly MobileInfoBox _infoBox;
    public Medkit(int x, int y, Textures textures, Input input, MobileInfoBox infoBox) : base(x, y, textures.Get(TextureName))
    {
        _interactingKey = input.Get(KeyName);
        _infoBox = infoBox;
    }
    private Medkit(int x, int y, Textures textures, Input input, MobileInfoBox infoBox, bool collected) : base(x, y, textures.Get(TextureName))
    {
        Collected = collected;
        ChangeVisibility(!Collected);
        _interactingKey = input.Get(KeyName);
        _infoBox = infoBox;
    }
    public override void Update()
    {
        bool interacting = IsPlayerTouching(out Player? player) && !Collected;
        UpdateInfoBox(interacting);
        if (!(interacting && _interactingKey.Pressed))
            return;
        player!.Heal(HealAmount);
        IsCollected = true;
    }
    public override Sprite? Clone(Engine engine, SpriteManager sprites) => new Medkit(X, Y, engine.Textures, engine.Input, _infoBox, Collected);
    private void UpdateInfoBox(bool interacting)
    {
        if (interacting)
        {
            _infoBox.Show(this, X + 5, Y / 2 - 2, 12, 1, [[$"!{KeyName} - Nehmen"]]);
        }
        else if (_infoBox.UsedBy == this)
        {
            _infoBox.UsedBy = null;
        }
    }
}