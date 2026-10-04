using JumpOn.Collectables;
using JumpOn.Entities;
using JumpOn.Interactables;
using JumpOn.UI.InfoBoxes;
using MCR_Engine.Core;
using MCR_Engine.InputSystem;
using MCR_Engine.Objects;

namespace JumpOn.Interactables;

public class Goal : Interactable
{
    private const string KeyName = "E", OpenTextureName = "goalOpen", CloseTextureName = "goalClosed";
    private readonly RGB?[,] _openTexture;
    private readonly Key _interactingKey;
    private readonly MobileInfoBox _infoBox;
    private List<GameKey>? _keys;
    private bool _open, _endGame;
    public Goal(int x, int y, Textures textures, Input input, MobileInfoBox infoBox) : base(x, y, textures.Get(CloseTextureName))
    {
        _openTexture = textures.Get(OpenTextureName);
        _interactingKey = input.Get(KeyName);
        _infoBox = infoBox;
        _open = false;
    }
    public int CollectedKeysAmount
    {
        get
        {
            int sum = 0;
            foreach (var key in _keys)
                if (key.IsCollected)
                    sum++;
            return sum;
        }
    }
    public int KeysAmount => _keys.Count;
    public List<GameKey> Keys
    {
        set
        {
            if (_keys is not null)
                throw new ArgumentException("Keys cannot be set twice.");
            _keys = value ?? throw new NullReferenceException("Value cannot be null.");
        }
    }
    public bool EndGame => _endGame;
    private bool CollectedAllKeys => Collectable.AreAllCollected(_keys) || true;
    protected override bool IsEntityInteracting(out Entity? entity)
    {
        if (_keys is null)
            throw new NullReferenceException("Keys cannot be null.");
        bool playerInteracting = base.IsEntityInteracting(out entity) && entity as Player is not null;
        UpdateInfoBox(playerInteracting);
        if (!(playerInteracting && _interactingKey.Pressed && CollectedAllKeys))
            return false;
        if (_open)
        {
            _endGame = true;
            return true;
        }
        _open = true;
        SetNewTexture(_openTexture);
        Thread.Sleep(200);
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
            else if(CollectedAllKeys)
            {
                _infoBox.Show(this, X + 7, Y / 2, 14, 1, [[$"!{KeyName} - Öffnen"]]);
            }
            else _infoBox.Show(this, X + 7, Y / 2, 13, 1, [[$"-Keys: {CollectedKeysAmount}/{KeysAmount}"]]);

        }
        else if (_infoBox.UsedBy == this)
        {
            _infoBox.UsedBy = null;
        }
    }
}