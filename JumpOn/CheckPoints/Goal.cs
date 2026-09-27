using JumpOn.Collectables;
using JumpOn.Entities;
using JumpOn.Interactables;
using JumpOn.UI.InfoBoxes;
using MCR_Engine.Core;
using MCR_Engine.InputSystem;
using MCR_Engine.Objects;

namespace JumpOn.CheckPoints;

public class Goal : Interactable
{
    private const string KeyName = "E", OpenTextureName = "goalOpen", CloseTextureName = "goalClosed";
    private readonly List<GameKey> _keys;
    private readonly RGB?[,] _openTexture;
    private readonly Key _interactingKey;
    private readonly MobileInfoBox _infoBox;
    private bool _open, _endGame;
    public Goal(int x, int y, Textures textures, SpriteManager sprites, Input input, MobileInfoBox infoBox) : base(x, y, textures.Get(CloseTextureName))
    {
        _keys = sprites.GetAllSpritesOfType<GameKey>();
        _openTexture = textures.Get(OpenTextureName);
        _interactingKey = input.GetKey(KeyName);
        _infoBox = infoBox;
        _open = false;
    }
    public bool EndGame => _endGame;
    protected override bool IsEntityInteracting(out Entity? entity)
    {
        bool playerInteracting = base.IsEntityInteracting(out entity) && entity as Player is not null;
        UpdateInfoBox(playerInteracting);
        if (!(playerInteracting && _interactingKey.Pressed && Collectable.AreAllCollected(_keys)))
            return false;
        if (_open)
        {
            _endGame = true;
            return true;
        }
        _open = true;
        SetNewTexture(_openTexture);
        return true;
    }
    private void UpdateInfoBox(bool playerInteracting)
    {
        if (playerInteracting)
        {
            if (_open)
            {
                _infoBox.Show(this, X + 7, Y / 2, 16, 1, [[$"!{KeyName} - Durchgehen"]]);
            }
            else _infoBox.Show(this, X + 7, Y / 2, 14, 1, [[$"!{KeyName} - Öffnen"]]);
        }
        else if (_infoBox.UsedBy == this)
        {
            _infoBox.UsedBy = null;
        }
    }
}