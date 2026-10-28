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
    private readonly RGB?[,] _closedTexture;
    private readonly Key _interactingKey;
    private readonly MobileInfoBox _infoBox;
    private List<GameKey>? _keys;
    private bool _open;
    public Goal(int x, int y, Textures textures, Input input, MobileInfoBox infoBox) : base(x, y, textures.Get(CloseTextureName), true)
    {
        _openTexture = textures.Get(OpenTextureName);
        _closedTexture = textures.Get(CloseTextureName);
        _interactingKey = input.Get(KeyName);
        _infoBox = infoBox;
        _open = false;
    }
    private Goal(int x, int y, RGB?[,] openTexture, RGB?[,] closedTexture, Key interactingKey, MobileInfoBox infoBox) : base(x, y, closedTexture, true)
    {
        _openTexture = openTexture;
        _closedTexture = closedTexture;
        _interactingKey = interactingKey;
        _infoBox = infoBox;
        _open = false;
    }
    public int KeysAmount => Keys.Count;
    public bool EndGame => _open;
    public int CollectedKeysAmount => Collectable.CollectedAmount(Keys);
    private bool CollectedAllKeys => Collectable.AreAllCollected(Keys);
    private List<GameKey> Keys => _keys ?? throw new NullReferenceException("Keys cannot be null.");
    public override GameObject? Clone(ObjectManager objects) => new Goal(X, Y, _openTexture, _closedTexture, _interactingKey, _infoBox);
    protected override bool IsEntityInteracting(Entity entity)
    {
        bool playerInteracting = base.IsEntityInteracting(entity) && entity is Player;
        UpdateInfoBox(playerInteracting);
        if (!(playerInteracting && _interactingKey.Pressed && CollectedAllKeys))
            return false;
        _open = true;
        _infoBox.Visible = false;
        SetNewTexture(_openTexture);
        Thread.Sleep(350);
        return true;
    }
    private void UpdateInfoBox(bool playerInteracting)
    {
        if (playerInteracting)
        {
            if(CollectedAllKeys)
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
    public static Goal LoadGoal(ObjectManager objects)
    {
        List<Goal> goals = objects.GetAllObjectsOfType<Goal>();
        if (goals.Count != 1)
            throw new ArgumentException("Invalid amount of goals");
        goals[0]._keys = objects.GetAllObjectsOfType<GameKey>();
        return goals[0];
    }
}