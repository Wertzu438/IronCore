using JumpOn.Collectables;
using JumpOn.Entities;
using JumpOn.Entities.LivingEntities;
using JumpOn.Interactables;
using JumpOn.UI.InfoBoxes;
using MCR_Engine.Core;
using MCR_Engine.InputSystem;
using MCR_Engine.ObjectSystem;
using MCR_Engine.Rendering.UI;

namespace JumpOn.Interactables;

public class Goal : Interactable
{
    private const string KeyName = "E", OpenTextureName = "goalOpen", CloseTextureName = "goalClosed";
    private readonly RGB?[,] _openTexture;
    private readonly RGB?[,] _closedTexture;
    private readonly Key _interactingKey;
    private readonly MobileInfoBox _infoBox;
    private GameKey[]? _keys;
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
    public int KeysAmount => Keys.Length;
    public bool EndGame => _open;
    public int CollectedKeysAmount => Collectable.CollectedAmount(Keys);
    private bool CollectedAllKeys => Collectable.AreAllCollected(Keys);
    private GameKey[] Keys => _keys ?? throw new NullReferenceException("Keys cannot be null.");
    public override GameObject? Clone(Objects objects) => new Goal(X, Y, _openTexture, _closedTexture, _interactingKey, _infoBox);
    protected override bool IsObjectInteracting(GameObject gameObject)
    {
        bool playerInteracting = base.IsObjectInteracting(gameObject) && gameObject is Player;
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
                _infoBox.Show(this, X + 7, Y, 16, 1, [[$"!{KeyName} - Öffnen"]]);
            }
            else _infoBox.Show(this, X + 7, Y, 15, 1, [[$"-Keys: {CollectedKeysAmount}/{KeysAmount}"]]);
        }
        else if (_infoBox.UsedBy == this)
        {
            _infoBox.UsedBy = null;
        }
    }
    public static Goal LoadGoal(Objects objects, UIManager ui)
    {
        Goal[] goals = objects.GetAllObjectsOfType<Goal>();
        if (goals.Length != 1)
            throw new ArgumentException("Invalid amount of goals");
        goals[0]._keys = objects.GetAllObjectsOfType<GameKey>();
        foreach (GameKey key in goals[0].Keys)
            ui.Add(new InfoIndicator(key, "🗝️", () => true));
        ui.Add(new InfoIndicator(goals[0], "🚩", () => goals[0].CollectedAllKeys));
        return goals[0];
    }
}