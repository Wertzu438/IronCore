using System.Diagnostics;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using JumpOn.Core;
using JumpOn.Interactables;
using MCR_Engine.Core;
using MCR_Engine.InputSystem;
using MCR_Engine.Objects;
using MCR_Engine.Rendering.Output;
using MCR_Engine.Rendering.UI;

namespace JumpOn.Core;

/// <summary>
/// Map-Editor: bearbeitet Scenes/levelN.txt im Format des LevelLoaders.
/// Aufruf unverändert über new MapEditor(engine).Run(); Run() kehrt zurück, wenn der Editor beendet wird.
/// Benötigt Camera.SetOffset(int x, int y) (siehe Hinweis in der Antwort).
/// </summary>
public class MapEditor
{
    private const string ScenesFolder = "Scenes", TexturesFolder = "Textures";
    private const int PanelWidth = 34, PanelMargin = 2;
    private const int MaxUndo = 60, MaxAmount = 400;
    private const int RepeatDelayMs = 350, RepeatIntervalMs = 45;

    private static readonly Regex LevelFileName = new(@"^level(\d+)\.txt$", RegexOptions.IgnoreCase);

    private static readonly HashSet<string> ReservedTextures = new()
    {
        "ladderPart", "barPart", "laserDevice", "laserPart", "arrowDevice", "arrow", "spike", "switch",
        "goalOpen", "goalClosed", "key", "medkit", "checkPoint_1", "checkPoint_2"
    };

    private static readonly ObjType[] TypeOrder =
    [
        ObjType.Solid, ObjType.Decoration, ObjType.Ladder, ObjType.Bar, ObjType.Spike, ObjType.Laser,
        ObjType.Arrow, ObjType.Switch, ObjType.Key, ObjType.Medkit, ObjType.CheckPoint, ObjType.Goal
    ];

    // Eigene Tasten (Windows Virtual-Key-Codes), damit der Editor nicht von deinen Input-Dateien abhängt.
    private static readonly (string Name, int Vk)[] KeyDefs =
    [
        ("EdLeft", 0x25), ("EdUp", 0x26), ("EdRight", 0x27), ("EdDown", 0x28),
        ("EdShift", 0x10), ("EdCtrl", 0x11),
        ("EdSpace", 0x20), ("EdEnter", 0x0D), ("EdEsc", 0x1B), ("EdTab", 0x09), ("EdDel", 0x2E),
        ("EdPgUp", 0x21), ("EdPgDn", 0x22), ("EdF5", 0x74),
        ("EdA", 0x41), ("EdD", 0x44), ("EdE", 0x45), ("EdG", 0x47), ("EdH", 0x48), ("EdI", 0x49),
        ("EdJ", 0x4A), ("EdK", 0x4B), ("EdN", 0x4E), ("EdP", 0x50), ("EdQ", 0x51), ("EdR", 0x52),
        ("EdS", 0x53), ("EdU", 0x55), ("EdW", 0x57), ("EdX", 0x58), ("EdY", 0x59), ("EdZ", 0x5A)
    ];

    // ------------------------------------------------------------------ Hilfsklassen

    /// <summary>Taste mit Flankenerkennung und Auto-Repeat (Input.Pressed ist nur pegelbasiert).</summary>
    private sealed class EdKey
    {
        private readonly Key _key;
        private bool _previous;
        private long _repeatAt;

        public EdKey(Key key) { _key = key; }

        public bool Down { get; private set; }
        public bool Pressed { get; private set; }
        public bool Repeat { get; private set; }

        /// <summary>Beim Start gehaltene Tasten lösen nichts aus, bis sie losgelassen wurden.</summary>
        public void Reset()
        {
            _previous = _key.Pressed;
            Down = _previous;
            _repeatAt = _previous ? long.MaxValue : 0;
        }

        public void Poll(long now)
        {
            Down = _key.Pressed;
            Pressed = Down && !_previous;
            Repeat = false;
            if (Pressed)
            {
                Repeat = true;
                _repeatAt = now + RepeatDelayMs;
            }
            else if (Down && now >= _repeatAt)
            {
                Repeat = true;
                _repeatAt = now + RepeatIntervalMs;
            }
            _previous = Down;
        }
    }

    private sealed class ModalItem
    {
        public ModalItem(Func<string> label) { Label = label; }
        public Func<string> Label { get; }
        public Action<int>? Change { get; init; }
        public Action? Activate { get; init; }
    }

    private sealed class Modal
    {
        public Modal(string title, List<ModalItem> items) { Title = title; Items = items; }
        public string Title { get; }
        public List<ModalItem> Items { get; }
        public List<string> Info { get; init; } = [];
        public Action? OnEscape { get; init; }
        public int Index { get; set; }
    }

    // ------------------------------------------------------------------ Zustand

    private readonly Engine _engine;
    private readonly Textures _textures;
    private readonly Camera _camera;
    private readonly UIManager _ui;
    private readonly Dictionary<string, EdKey> _keys = [];
    private readonly Stopwatch _time = Stopwatch.StartNew();
    private readonly List<string> _solidTextures = [], _backgrounds = [];
    private readonly HashSet<string> _missingTextures = [];
    private readonly List<EditorLevel> _undo = [], _redo = [];
    private readonly UIElement _statusBox, _brushBox, _messageBox, _helpBox, _modalBox;

    private EditorLevel _level = new();
    private EditorObject _brush = new();
    private EditorObject? _held, _linkSwitch;
    private Scene? _scene;
    private Sprite? _cursor, _ghost, _outline, _partner, _spawnPreview;
    private RGB?[,]? _playerTexture;
    private Modal? _modal;
    private string _message = "H = Hilfe, Esc = Menü";
    private int _cx, _cy, _grid = 1, _camX, _camY, _maxLayer = 7;
    private bool _panels = true, _help, _dirty, _dirtyBeforeGrab, _changed = true, _raysDirty, _ghostDirty = true, _exit;

    public MapEditor(Engine engine)
    {
        _engine = engine;
        _textures = engine.Textures;
        _camera = engine.Camera;
        _ui = engine.UI;
        _statusBox = new UIElement(PanelWidth, 7, PanelMargin, 1, true, false);
        _brushBox = new UIElement(PanelWidth, 10, PanelMargin, 9, true, false);
        _messageBox = new UIElement(PanelWidth, 7, PanelMargin, 20, true, false);
        _helpBox = new UIElement(66, 24, 0, 1, true, false);
        _modalBox = new UIElement(44, 8, 0, 1, true, false);
    }

    // ------------------------------------------------------------------ Start / Hauptschleife

    public void Run()
    {
        try
        {
            Initialize();
            MainLoop();
        }
        catch (Exception e)
        {
            ReportFatal(e);
        }
        finally
        {
            _ui.Clear();
            FlushConsoleInput();
        }
    }

    private void Initialize()
    {
        RegisterKeys(_engine.Input);
        LoadTextureLists();
        _brush = NewBrush(ObjType.Solid);
        _ui.Clear();
        _ui.Add(_statusBox);
        _ui.Add(_brushBox);
        _ui.Add(_messageBox);
        _ui.Add(_helpBox);
        _ui.Add(_modalBox);
        BuildHelp();
        ShowLevelPicker(false);
    }

    private void MainLoop()
    {
        int errors = 0;
        while (!_exit)
        {
            try
            {
                long now = _time.ElapsedMilliseconds;
                foreach (EdKey key in _keys.Values)
                    key.Poll(now);
                if (_modal is not null)
                    HandleModal();
                else if (_scene is not null)
                    HandleEdit();
                if (_changed && !_exit)
                    Redraw();
                errors = 0;
            }
            catch (Exception e)
            {
                Msg("Fehler: " + e.Message);
                if (++errors > 25)
                    break;
            }
            Thread.Sleep(10);
        }
    }

    private void Redraw()
    {
        _changed = false;
        if (_scene is not null)
        {
            ClampCursor();
            UpdateOverlays();
            ScrollToCursor();
            Refresh();
        }
        UpdatePanels();
        if (_modal is not null)
            RenderModal();
    }

    /// <summary>Zeichnet die Szene neu. Laser-Strahlen werden ohne Overlays gemessen, damit sie nicht verfälscht werden.</summary>
    private void Refresh(bool forceAll = false)
    {
        if (_scene is null)
            return;
        bool all = forceAll || (long)_level.Width * _level.Height <= 250_000;
        if (_raysDirty)
        {
            HideOverlays();
            _scene.Update(0.016f, all);
            RecomputeRays();
            _raysDirty = false;
            UpdateOverlays();
        }
        _scene.Update(0.016f, all);
    }

    private void RegisterKeys(Input input)
    {
        foreach ((string name, int vk) in KeyDefs)
        {
            Key key;
            try
            {
                key = input.Get(name);
            }
            catch (ArgumentException)
            {
                input.AddKey(name, vk);
                key = input.Get(name);
            }
            _keys[name] = new EdKey(key);
        }
        Thread.Sleep(80); // Input-Thread soll die neuen Tasten einmal abtasten
        foreach (EdKey key in _keys.Values)
            key.Reset();
    }

    private void LoadTextureLists()
    {
        string[] files = Directory.Exists(TexturesFolder) ? Directory.GetFiles(TexturesFolder) : Array.Empty<string>();
        foreach (string file in files.OrderBy(f => f, StringComparer.OrdinalIgnoreCase))
        {
            string name = Path.GetFileNameWithoutExtension(file);
            RGB?[,] texture;
            try
            {
                texture = _textures.Get(name);
            }
            catch (KeyNotFoundException)
            {
                continue;
            }
            if (!name.StartsWith("player", StringComparison.OrdinalIgnoreCase) && !ReservedTextures.Contains(name))
                _solidTextures.Add(name);
            try
            {
                Textures.ConvertNullableTexture(texture);
                _backgrounds.Add(name);
            }
            catch (ArgumentException)
            {
                // enthält transparente Pixel: als Hintergrund unbrauchbar
            }
        }
        if (_solidTextures.Count == 0)
            throw new InvalidOperationException($"Keine Texturen im Ordner '{TexturesFolder}' gefunden.");
        if (_backgrounds.Count == 0)
            throw new InvalidOperationException("Keine Textur ohne transparente Pixel für den Hintergrund gefunden.");
        try
        {
            _playerTexture = Tint(_textures.Get("playerStand"), 120, 255, 120);
        }
        catch (KeyNotFoundException)
        {
            _playerTexture = null;
        }
    }

    private void ReportFatal(Exception e)
    {
        try
        {
            _engine.UpdateRenderAction(ProcessAction.Pause);
            Thread.Sleep(500);
            Console.WriteLine(e);
            Console.WriteLine("Beliebige Taste drücken ...");
            Console.ReadKey(true);
            _engine.UpdateRenderAction(ProcessAction.Start);
        }
        catch (ArgumentException)
        {
            // Renderer war nicht im erwarteten Zustand
        }
    }

    private static void FlushConsoleInput()
    {
        try
        {
            while (Console.KeyAvailable)
                Console.ReadKey(true);
        }
        catch (InvalidOperationException)
        {
        }
    }

    // ------------------------------------------------------------------ Eingabe

    private bool Hit(string key) => _keys[key].Pressed;
    private bool Rep(string key) => _keys[key].Repeat;
    private bool Down(string key) => _keys[key].Down;

    private void HandleModal()
    {
        Modal m = _modal!;
        int count = m.Items.Count;
        if (Rep("EdUp")) { m.Index = (m.Index - 1 + count) % count; _changed = true; }
        if (Rep("EdDown")) { m.Index = (m.Index + 1) % count; _changed = true; }
        if (Rep("EdLeft")) { m.Items[m.Index].Change?.Invoke(-1); _changed = true; }
        if (Rep("EdRight")) { m.Items[m.Index].Change?.Invoke(1); _changed = true; }
        if (Hit("EdEnter") || Hit("EdSpace"))
        {
            m.Items[m.Index].Activate?.Invoke();
            _changed = true;
            return;
        }
        if (Hit("EdEsc"))
        {
            m.OnEscape?.Invoke();
            _changed = true;
        }
    }

    private void HandleEdit()
    {
        if (_help)
        {
            if (Hit("EdH") || Hit("EdEsc")) { _help = false; _changed = true; }
            return;
        }

        bool ctrl = Down("EdCtrl"), shift = Down("EdShift");

        if (Hit("EdEsc"))
        {
            if (_linkSwitch is not null) CancelLink();
            else if (_held is not null) CancelHold();
            else ShowMainMenu();
            return;
        }
        if (Hit("EdH")) { _help = true; _changed = true; return; }

        if (ctrl)
        {
            if (Hit("EdZ")) Undo();
            else if (Hit("EdY")) Redo();
            else if (Hit("EdS")) Save();
        }
        if (Hit("EdF5")) Save();

        // Cursor bewegen
        int step = _grid * (ctrl ? 32 : shift ? 8 : 1);
        int dx = 0, dy = 0;
        if (Rep("EdLeft")) dx -= step;
        if (Rep("EdRight")) dx += step;
        if (Rep("EdUp")) dy -= step;
        if (Rep("EdDown")) dy += step;
        if (dx != 0 || dy != 0)
        {
            _cx = Snap(_cx + dx);
            _cy = Snap(_cy + dy);
            ClampCursor();
            MoveHeld();
            _changed = true;
        }

        if (ctrl)
            return;

        if (Hit("EdG"))
        {
            _grid = _grid switch { 1 => 2, 2 => 4, 4 => 8, _ => 1 };
            _cx = Snap(_cx);
            _cy = Snap(_cy);
            MoveHeld();
            Msg($"Raster {_grid}");
        }
        if (Hit("EdP")) { _panels = !_panels; _changed = true; }

        if (_linkSwitch is not null)
        {
            if (Hit("EdEnter")) ConfirmLink();
            return;
        }

        if (Hit("EdSpace")) { if (_held is not null) Drop(); else Place(); }
        if (Hit("EdEnter")) { if (_held is not null) Drop(); else Grab(); }
        if (Hit("EdDel") || Hit("EdX")) DeleteUnderCursor();
        if (Hit("EdTab")) Pick();
        if (_held is null)
        {
            if (Hit("EdY")) RelinkUnderCursor();
            if (Hit("EdN")) SpawnUnderCursor();
            if (Hit("EdQ")) ChangeType(-1);
            if (Hit("EdE")) ChangeType(1);
        }

        // Eigenschaften des Pinsels bzw. des gegriffenen Objekts
        EditorObject cur = _held ?? _brush;
        bool edited = false;
        if (Rep("EdA")) edited |= ChangeTexture(cur, -1);
        if (Rep("EdD")) edited |= ChangeTexture(cur, 1);
        if (Rep("EdW")) edited |= ChangeAmount(cur, shift ? 8 : 1);
        if (Rep("EdS")) edited |= ChangeAmount(cur, shift ? -8 : -1);
        if (Hit("EdR")) edited |= ChangeDir(cur);
        if (Hit("EdPgUp")) edited |= ChangeLayer(cur, 1);
        if (Hit("EdPgDn")) edited |= ChangeLayer(cur, -1);
        if (Rep("EdU")) edited |= ChangeParam(cur, 0, 1, shift);
        if (Rep("EdJ")) edited |= ChangeParam(cur, 0, -1, shift);
        if (Rep("EdI")) edited |= ChangeParam(cur, 1, 1, shift);
        if (Rep("EdK")) edited |= ChangeParam(cur, 1, -1, shift);
        if (edited)
            AfterBrushChange(cur);
    }

    // ------------------------------------------------------------------ Aktionen

    private void Place()
    {
        if (_brush.Type == ObjType.Goal && _level.Alive.Any(x => x.Type == ObjType.Goal))
        {
            Msg("Es gibt schon ein Ziel (nur 1 erlaubt). Mit Enter greifen und verschieben.");
            return;
        }
        PushUndo();
        EditorObject o = _brush.Copy();
        o.X = _cx;
        o.Y = _cy;
        AddObject(o);
        DropToFloor(o);
        if (o.Type == ObjType.Switch)
        {
            _linkSwitch = o;
            Msg("Schalter platziert: Cursor auf das Ziel-Objekt, dann Enter. Esc bricht ab.");
        }
    }

    private void Grab()
    {
        EditorObject? o = ObjectAt(_cx, _cy, null);
        if (o is null)
        {
            Msg("Hier ist kein Objekt zum Greifen.");
            return;
        }
        _dirtyBeforeGrab = _dirty;
        PushUndo();
        _held = o;
        _cx = o.X;
        _cy = o.Y;
        Msg($"Gegriffen: {TypeName(o.Type)}. Pfeile bewegen, Enter legt ab, Esc bricht ab.");
    }

    private void Drop()
    {
        EditorObject o = _held!;
        DropToFloor(o);
        _held = null;
        _dirty = true;
        _raysDirty = true;
        RefreshCheckpoints();
        Msg("Abgelegt.");
    }

    private void CancelHold()
    {
        bool wasDirty = _dirtyBeforeGrab;
        Undo();
        _redo.Clear();
        _dirty = wasDirty;
        Msg("Abgebrochen.");
    }

    private void DeleteUnderCursor()
    {
        EditorObject? o = _held ?? ObjectAt(_cx, _cy, null);
        if (o is null)
        {
            Msg("Hier ist nichts zum Löschen.");
            return;
        }
        PushUndo();
        foreach (EditorObject s in _level.Alive.Where(x => x.Type == ObjType.Switch && ReferenceEquals(x.Target, o)).ToList())
            RemoveObject(s);
        RemoveObject(o);
        if (ReferenceEquals(o, _held))
            _held = null;
        Msg($"Gelöscht: {TypeName(o.Type)}.");
    }

    private void Pick()
    {
        EditorObject? o = ObjectAt(_cx, _cy, null);
        if (o is null)
        {
            Msg("Hier ist nichts zum Aufnehmen.");
            return;
        }
        _brush = o.Copy();
        _ghostDirty = true;
        Msg($"Aufgenommen: {TypeName(o.Type)}.");
    }

    private void RelinkUnderCursor()
    {
        EditorObject? o = ObjectAt(_cx, _cy, null);
        if (o is null || o.Type != ObjType.Switch)
        {
            Msg("Y: Cursor auf einen Schalter setzen.");
            return;
        }
        PushUndo();
        _linkSwitch = o;
        Msg("Neues Ziel wählen: Cursor auf das Objekt, dann Enter. Esc bricht ab.");
    }

    private void ConfirmLink()
    {
        EditorObject sw = _linkSwitch!;
        EditorObject? target = ObjectAt(_cx, _cy, sw);
        if (target is null)
        {
            Msg("Hier ist kein Ziel-Objekt.");
            return;
        }
        if (target.Type == ObjType.Switch)
        {
            Msg("Ein Schalter kann kein Schalter als Ziel haben.");
            return;
        }
        if (_level.Alive.Any(o => o.Type == ObjType.Switch && !ReferenceEquals(o, sw) && ReferenceEquals(o.Target, target)))
        {
            Msg("Dieses Objekt hat schon einen Schalter (der Loader erlaubt nur einen).");
            return;
        }
        sw.Target = target;
        _linkSwitch = null;
        _dirty = true;
        Msg($"Schalter verbunden mit: {TypeName(target.Type)}.");
    }

    private void CancelLink()
    {
        EditorObject sw = _linkSwitch!;
        _linkSwitch = null;
        if (sw.Target is null)
            RemoveObject(sw); // neu platzierter Schalter ohne Ziel
        if (_undo.Count > 0)
            _undo.RemoveAt(_undo.Count - 1);
        Msg("Verbinden abgebrochen.");
    }

    private void SpawnUnderCursor()
    {
        EditorObject? o = ObjectAt(_cx, _cy, null);
        if (o is null || o.Type != ObjType.CheckPoint)
        {
            Msg("N: Cursor auf einen Checkpoint setzen.");
            return;
        }
        int first = _level.Objects.FindIndex(x => !x.Deleted && x.Type == ObjType.CheckPoint);
        int current = _level.Objects.IndexOf(o);
        if (current == first)
        {
            Msg("Dieser Checkpoint ist schon der Start.");
            return;
        }
        PushUndo();
        _level.Objects.RemoveAt(current);
        _level.Objects.Insert(first, o);
        RefreshCheckpoints();
        _dirty = true;
        Msg("Checkpoint ist jetzt der Spieler-Start (der Loader nimmt den ersten Checkpoint).");
    }

    private void MoveHeld()
    {
        if (_held is null)
            return;
        _held.X = _cx;
        _held.Y = _cy;
        if (_held.Main is not null)
        {
            _held.Main.X = _cx;
            _held.Main.Y = _cy;
        }
        if (_held.Type == ObjType.Laser)
            _raysDirty = true;
        _dirty = true;
    }

    /// <summary>Sammelobjekte haben im Spiel Schwerkraft, deshalb werden sie auf den Boden gesetzt.</summary>
    private void DropToFloor(EditorObject o)
    {
        if (o.Main is null || o.Type is not (ObjType.Key or ObjType.Medkit or ObjType.CheckPoint))
            return;
        int distance = o.Main.GetDistanceToNextSprite(Direction.Down, true, true, false);
        if (distance <= 0)
            return;
        if (o.Y + o.Main.Height + distance >= _level.Height)
        {
            Msg("Kein Boden unter dem Objekt: es würde im Spiel fallen.");
            return;
        }
        o.Y += distance;
        o.Main.Y = o.Y;
        _changed = true;
    }

    // ------------------------------------------------------------------ Pinsel

    private EditorObject NewBrush(ObjType type)
    {
        var o = new EditorObject { Type = type };
        if (IsTiled(type) && _solidTextures.Count > 0)
            o.Texture = _solidTextures[0];
        if (type == ObjType.Arrow)
            o.Dir = Direction.Right;
        return o;
    }

    private void ChangeType(int d)
    {
        int index = Array.IndexOf(TypeOrder, _brush.Type);
        ObjType next = TypeOrder[(index + d + TypeOrder.Length) % TypeOrder.Length];
        EditorObject nb = NewBrush(next);
        if (IsTiled(_brush.Type) && IsTiled(next))
        {
            nb.Texture = _brush.Texture;
            nb.Layer = _brush.Layer;
            nb.Dir = _brush.Dir;
            nb.Amount = _brush.Amount;
        }
        _brush = nb;
        _ghostDirty = true;
        _changed = true;
    }

    private bool ChangeTexture(EditorObject o, int d)
    {
        if (!IsTiled(o.Type) || _solidTextures.Count == 0)
            return false;
        int index = _solidTextures.IndexOf(o.Texture);
        index = ((index < 0 ? 0 : index) + d + _solidTextures.Count) % _solidTextures.Count;
        o.Texture = _solidTextures[index];
        return true;
    }

    private static bool ChangeAmount(EditorObject o, int d)
    {
        if (!UsesAmount(o.Type))
            return false;
        int n = Math.Clamp(o.Amount + d, 1, MaxAmount);
        if (n == o.Amount)
            return false;
        o.Amount = n;
        Normalize(o);
        return true;
    }

    private static bool ChangeDir(EditorObject o)
    {
        if (IsTiled(o.Type))
        {
            o.Dir = o.Dir switch
            {
                Direction.None => Direction.Right,
                Direction.Right => Direction.Down,
                _ => o.Amount > 1 ? Direction.Right : Direction.None
            };
            return true;
        }
        if (o.Type == ObjType.Arrow)
        {
            o.Dir = o.Dir == Direction.Right ? Direction.Left : Direction.Right;
            return true;
        }
        return false;
    }

    private bool ChangeLayer(EditorObject o, int d)
    {
        if (!IsTiled(o.Type))
            return false;
        int n = Math.Clamp(o.Layer + d, 0, _maxLayer);
        if (n == o.Layer)
            return false;
        o.Layer = n;
        return true;
    }

    private static bool ChangeParam(EditorObject o, int which, int d, bool big)
    {
        int step = big ? 1000 : 100;
        if (o.Type == ObjType.Laser)
        {
            if (which == 0)
                o.OnMs = Math.Clamp(o.OnMs + d * step, 100, 30000);
            else
                o.OffMs = Math.Clamp(o.OffMs + d * step, 100, 30000);
            return true;
        }
        if (o.Type == ObjType.Arrow && which == 0)
        {
            o.IntervalMs = Math.Clamp(o.IntervalMs + d * step, 200, 30000);
            return true;
        }
        return false;
    }

    private void AfterBrushChange(EditorObject cur)
    {
        if (ReferenceEquals(cur, _held))
        {
            UpdateSprite(cur);
            _dirty = true;
        }
        else
        {
            _ghostDirty = true;
        }
        _changed = true;
    }

    /// <summary>Block/Deko mit Erweiterung braucht Richtung Right oder Down (der Loader wirft sonst).</summary>
    private static void Normalize(EditorObject o)
    {
        if (IsTiled(o.Type) && o.Amount > 1 && o.Dir is not (Direction.Right or Direction.Down))
            o.Dir = Direction.Right;
    }

    private static bool IsTiled(ObjType t) => t is ObjType.Solid or ObjType.Decoration;
    private static bool UsesAmount(ObjType t) => t is ObjType.Solid or ObjType.Decoration or ObjType.Ladder or ObjType.Bar or ObjType.Spike;

    // ------------------------------------------------------------------ Objekte und Sprites

    private void AddObject(EditorObject o)
    {
        _level.Objects.Add(o);
        BuildSprites(o);
        RefreshCheckpoints();
        _dirty = true;
        _changed = true;
    }

    private void RemoveObject(EditorObject o)
    {
        o.Deleted = true;
        o.Main?.ChangeVisibility(false);
        o.Ray?.ChangeVisibility(false);
        RefreshCheckpoints();
        _raysDirty = true;
        _dirty = true;
        _changed = true;
    }

    private void BuildSprites(EditorObject o)
    {
        o.Main = CreateSprite(o);
        if (o.Type == ObjType.Laser)
        {
            o.Ray = _scene!.Sprites.Add(new EditorSprite(o.X, o.Y, Tex("laserPart"), 0));
            _raysDirty = true;
        }
    }

    private Sprite CreateSprite(EditorObject o)
    {
        RGB?[,] texture = TextureFor(o);
        // SolidObject und Ladder sind die echten Spielklassen, weil der Laser-Strahl nach genau diesen Typen sucht.
        Sprite sprite = o.Type switch
        {
            ObjType.Solid => new SolidObject(o.X, o.Y, o.Layer, texture),
            ObjType.Decoration => new DecorationObject(o.X, o.Y, o.Layer, texture),
            ObjType.Ladder => new Ladder(o.X, o.Y, _textures, Math.Max(1, o.Amount)),
            _ => new EditorSprite(o.X, o.Y, texture, DrawLayer(o))
        };
        return _scene!.Sprites.Add(sprite);
    }

    /// <summary>Wendet geänderte Parameter auf das Sprite an. Die Layer ist unveränderlich, dann wird ein neues Sprite angelegt.</summary>
    private void UpdateSprite(EditorObject o)
    {
        if (o.Main is null)
            return;
        if (o.Main.Layer != DrawLayer(o))
        {
            o.Main.ChangeVisibility(false);
            o.Main = CreateSprite(o);
        }
        else
        {
            o.Main.SetNewTexture(TextureFor(o));
            o.Main.X = o.X;
            o.Main.Y = o.Y;
        }
        if (o.Type == ObjType.Laser)
            _raysDirty = true;
        _changed = true;
    }

    private RGB?[,] TextureFor(EditorObject o)
    {
        switch (o.Type)
        {
            case ObjType.Solid:
            case ObjType.Decoration:
                return Expand(Tex(o.Texture), o.Dir, o.Amount);
            case ObjType.Ladder:
                return Textures.CreateExpansion(Tex("ladderPart"), Direction.Down, Math.Max(1, o.Amount));
            case ObjType.Bar:
                return Textures.CreateExpansion(Tex("barPart"), Direction.Right, Math.Max(1, o.Amount));
            case ObjType.Spike:
                return Textures.CreateExpansion(Tex("spike"), Direction.Right, Math.Max(1, o.Amount));
            case ObjType.Laser:
                return Tex("laserDevice");
            case ObjType.Arrow:
                return o.Dir == Direction.Left ? Textures.MirrorLeftRight(Tex("arrowDevice")) : Tex("arrowDevice");
            case ObjType.Switch:
                return Tex("switch");
            case ObjType.Key:
                return Tex("key");
            case ObjType.Medkit:
                return Tex("medkit");
            case ObjType.Goal:
                return Tex("goalClosed");
            case ObjType.CheckPoint:
                return Tex(IsSpawn(o) ? "checkPoint_2" : "checkPoint_1");
            default:
                throw new InvalidOperationException($"Unbekannter Typ {o.Type}");
        }
    }

    private static RGB?[,] Expand(RGB?[,] texture, Direction dir, int amount)
        => amount > 1 && (dir is Direction.Right or Direction.Down) ? Textures.CreateExpansion(texture, dir, amount) : texture;

    private RGB?[,] Tex(string name)
    {
        try
        {
            return _textures.Get(name);
        }
        catch (KeyNotFoundException)
        {
            _missingTextures.Add(name);
            return Textures.DebuggingTexture(4, 4);
        }
    }

    private bool HasTexture(string name)
    {
        try
        {
            _textures.Get(name);
            return true;
        }
        catch (KeyNotFoundException)
        {
            return false;
        }
    }

    /// <summary>Gleiche Layer wie im Spiel: Interactables und Traps 0, Collectables 1, Block/Deko aus der Datei.</summary>
    private static int DrawLayer(EditorObject o) => o.Type switch
    {
        ObjType.Solid or ObjType.Decoration => o.Layer,
        ObjType.Key or ObjType.Medkit or ObjType.CheckPoint => 1,
        _ => 0
    };

    private EditorObject? Spawn() => _level.Alive.FirstOrDefault(o => o.Type == ObjType.CheckPoint);
    private bool IsSpawn(EditorObject o) => ReferenceEquals(o, Spawn());

    private void RefreshCheckpoints()
    {
        foreach (EditorObject o in _level.Alive)
        {
            if (o.Type == ObjType.CheckPoint && o.Main is not null)
                o.Main.SetNewTexture(TextureFor(o));
        }
    }

    private void RecomputeRays()
    {
        Type[] blockers = [typeof(SolidObject), typeof(Ladder)];
        RGB?[,] part = Tex("laserPart");
        foreach (EditorObject o in _level.Alive)
        {
            if (o.Type != ObjType.Laser || o.Main is null || o.Ray is null)
                continue;
            o.Ray.X = o.X + o.Main.Width / 2;
            o.Ray.Y = o.Y + o.Main.Height;
            int distance = o.Ray.GetDistanceToNextSprite(Direction.Down, true, false, true, blockers);
            o.Ray.SetNewTexture(Textures.CreateExpansion(part, Direction.Down, Math.Max(1, distance + 1)));
        }
    }

    private EditorObject? ObjectAt(int x, int y, EditorObject? exclude)
    {
        EditorObject? best = null;
        int bestKey = int.MinValue;
        for (int i = 0; i < _level.Objects.Count; i++)
        {
            EditorObject o = _level.Objects[i];
            if (o.Deleted || o.Main is null || ReferenceEquals(o, exclude))
                continue;
            if (x < o.X || y < o.Y || x >= o.X + o.Main.Width || y >= o.Y + o.Main.Height)
                continue;
            int key = DrawLayer(o) * 100000 + i;
            if (key > bestKey)
            {
                best = o;
                bestKey = key;
            }
        }
        return best;
    }

    // ------------------------------------------------------------------ Szene, Overlays, Kamera

    private void RebuildAll()
    {
        _level.Objects.RemoveAll(o => o.Deleted);
        _maxLayer = 7;
        foreach (EditorObject o in _level.Objects)
        {
            if (IsTiled(o.Type))
                _maxLayer = Math.Max(_maxLayer, o.Layer);
        }

        _missingTextures.Clear();
        var scene = new Scene(_camera, _level.Width, _level.Height, Textures.ConvertNullableTexture(_textures.Get(_level.Background)));
        _scene = scene;
        _held = null;
        _linkSwitch = null;
        foreach (EditorObject o in _level.Objects)
            BuildSprites(o);

        _spawnPreview = scene.Sprites.Add(new EditorSprite(0, 0, _playerTexture ?? new RGB?[1, 1], _maxLayer + 1));
        _ghost = scene.Sprites.Add(new EditorSprite(0, 0, new RGB?[1, 1], _maxLayer + 2));
        _outline = scene.Sprites.Add(new EditorSprite(0, 0, new RGB?[1, 1], _maxLayer + 3));
        _partner = scene.Sprites.Add(new EditorSprite(0, 0, new RGB?[1, 1], _maxLayer + 3));
        _cursor = scene.Sprites.Add(new EditorSprite(0, 0, CursorTexture(), _maxLayer + 4));
        _ghostDirty = true;
        _raysDirty = true;

        RefreshCheckpoints();
        ClampCursor();
        UpdateOverlays();
        Refresh(true);                 // erst komplett zeichnen, dann anzeigen (kein leerer Frame)
        _engine.SetRenderScene(scene);       // setzt die Kamera zurück, deshalb direkt danach wieder positionieren
        ScrollToCursor();
        Refresh();
        _changed = true;
        if (_missingTextures.Count > 0)
            Msg("Fehlende Texturen: " + string.Join(", ", _missingTextures));
    }

    private void UpdateOverlays()
    {
        if (_cursor is null || _ghost is null || _outline is null || _partner is null || _spawnPreview is null)
            return;

        _cursor.X = _cx - 1;
        _cursor.Y = _cy - 1;
        _cursor.ChangeVisibility(true);

        if (_ghostDirty)
        {
            _ghost.SetNewTexture(Tint(TextureFor(_brush), 0, 200, 255));
            _ghostDirty = false;
        }
        _ghost.X = _cx;
        _ghost.Y = _cy;
        _ghost.ChangeVisibility(_held is null && _linkSwitch is null && _modal is null);

        EditorObject? hover = _held ?? (_modal is null ? ObjectAt(_cx, _cy, _linkSwitch) : null);
        ShowOutline(_outline, hover, Rgb(255, 255, 255));

        EditorObject? partner = null;
        if (hover is not null)
        {
            partner = hover.Type == ObjType.Switch
                ? hover.Target
                : _level.Alive.FirstOrDefault(s => s.Type == ObjType.Switch && ReferenceEquals(s.Target, hover));
        }
        ShowOutline(_partner, partner, Rgb(255, 150, 0));

        EditorObject? spawn = Spawn();
        if (spawn is null || _playerTexture is null)
        {
            _spawnPreview.ChangeVisibility(false);
        }
        else
        {
            _spawnPreview.X = spawn.X;
            _spawnPreview.Y = spawn.Y;
            _spawnPreview.ChangeVisibility(true);
        }
    }

    private void HideOverlays()
    {
        _cursor?.ChangeVisibility(false);
        _ghost?.ChangeVisibility(false);
        _outline?.ChangeVisibility(false);
        _partner?.ChangeVisibility(false);
        _spawnPreview?.ChangeVisibility(false);
    }

    private static void ShowOutline(Sprite outline, EditorObject? o, RGB color)
    {
        if (o is null || o.Deleted || o.Main is null)
        {
            outline.ChangeVisibility(false);
            return;
        }
        int w = o.Main.Width + 2, h = o.Main.Height + 2;
        if (outline.Width != w || outline.Height != h)
            outline.SetNewTexture(MakeOutline(w, h, color));
        outline.X = o.X - 1;
        outline.Y = o.Y - 1;
        outline.ChangeVisibility(true);
    }

    private static RGB?[,] MakeOutline(int w, int h, RGB color)
    {
        var t = new RGB?[w, h];
        for (int x = 0; x < w; x++)
        {
            t[x, 0] = color;
            t[x, h - 1] = color;
        }
        for (int y = 0; y < h; y++)
        {
            t[0, y] = color;
            t[w - 1, y] = color;
        }
        return t;
    }

    private static RGB?[,] CursorTexture()
    {
        var t = new RGB?[3, 3];
        RGB yellow = Rgb(255, 255, 0);
        t[1, 0] = yellow;
        t[0, 1] = yellow;
        t[2, 1] = yellow;
        t[1, 2] = yellow;
        t[1, 1] = Rgb(255, 40, 40);
        return t;
    }

    private static RGB?[,] Tint(RGB?[,] source, int tr, int tg, int tb)
    {
        int w = source.GetLength(0), h = source.GetLength(1);
        var result = new RGB?[w, h];
        for (int y = 0; y < h; y++)
        {
            for (int x = 0; x < w; x++)
            {
                if (source[x, y] is RGB c)
                    result[x, y] = Rgb((byte)((c.Red * 55 + tr * 45) / 100), (byte)((c.Green * 55 + tg * 45) / 100), (byte)((c.Blue * 55 + tb * 45) / 100));
            }
        }
        return result;
    }

    // RGB hat einen internen Konstruktor, aber öffentliche Felder.
    private static RGB Rgb(byte r, byte g, byte b)
    {
        RGB c = default;
        c.Red = r;
        c.Green = g;
        c.Blue = b;
        return c;
    }

    private void ClampCursor()
    {
        _cx = Math.Clamp(_cx, 0, Math.Max(0, _level.Width - 1));
        _cy = Math.Clamp(_cy, 0, Math.Max(0, _level.Height - 1));
    }

    private int Snap(int v) => _grid <= 1 ? v : v / _grid * _grid;

    /// <summary>Die Kamera folgt nicht dem Engine-Follow, sondern wird direkt gesetzt (Camera.SetOffset).</summary>
    private void ScrollToCursor()
    {
        int usableWidth = Math.Max(20, Console.WindowWidth - (_panels ? PanelWidth + PanelMargin * 2 + 2 : 0));
        int viewHeight = Math.Max(4, (Console.WindowHeight - 1) * 2);   // Pixelzeilen, 2 pro Zellenzeile
        Sprite? shown = _held?.Main ?? _ghost;
        int extentX = Math.Clamp(shown?.Width ?? 0, 10, Math.Max(10, usableWidth / 2));
        int extentY = Math.Clamp(shown?.Height ?? 0, 10, Math.Max(10, viewHeight / 2));

        int offX = _camX, offY = _camY * 2;
        if (_cx - 10 < offX) offX = _cx - 10;
        else if (_cx + extentX >= offX + usableWidth) offX = _cx + extentX - usableWidth + 1;
        if (_cy - 10 < offY) offY = _cy - 10;
        else if (_cy + extentY >= offY + viewHeight) offY = _cy + extentY - viewHeight + 1;

        //_camera.RecalculateOffset(1);
        _camera.SetOffset(Math.Max(0, offX), Math.Max(0, offY) / 2);
        _camX = _camera.XOffset;
        _camY = _camera.YOffset;
    }

    // ------------------------------------------------------------------ Undo / Redo

    private void PushUndo()
    {
        _undo.Add(_level.Clone());
        if (_undo.Count > MaxUndo)
            _undo.RemoveAt(0);
        _redo.Clear();
    }

    private void Undo()
    {
        if (_undo.Count == 0)
        {
            Msg("Nichts zum Rückgängigmachen.");
            return;
        }
        _redo.Add(_level.Clone());
        _level = _undo[^1];
        _undo.RemoveAt(_undo.Count - 1);
        _dirty = true;
        RebuildAll();
        Msg("Rückgängig.");
    }

    private void Redo()
    {
        if (_redo.Count == 0)
        {
            Msg("Nichts zum Wiederholen.");
            return;
        }
        _undo.Add(_level.Clone());
        _level = _redo[^1];
        _redo.RemoveAt(_redo.Count - 1);
        _dirty = true;
        RebuildAll();
        Msg("Wiederholt.");
    }

    // ------------------------------------------------------------------ Dateien

    private static string LevelPath(int number) => Path.Combine(ScenesFolder, $"level{number}.txt");

    private static List<int> ExistingLevels()
    {
        var list = new List<int>();
        if (!Directory.Exists(ScenesFolder))
            return list;
        foreach (string file in Directory.GetFiles(ScenesFolder, "level*.txt"))
        {
            Match match = LevelFileName.Match(Path.GetFileName(file));
            if (match.Success && int.TryParse(match.Groups[1].Value, out int number))
                list.Add(number);
        }
        list.Sort();
        return list;
    }

    private void OpenLevel(int number)
    {
        EditorLevel? level = ParseLevel(number, out List<string> warnings);
        if (level is null)
        {
            ShowReport("LADEN FEHLGESCHLAGEN", warnings, () => ShowLevelPicker(_scene is not null));
            return;
        }

        // Kleiner als das Fenster würde die UI-Boxen außerhalb der Szene zeichnen (Exception im Renderer).
        int minWidth = Console.WindowWidth, minHeight = EvenUp(Console.WindowHeight * 2);
        bool adjusted = false;
        if (level.Width < minWidth)
        {
            level.Width = minWidth;
            adjusted = true;
        }
        if (level.Height < minHeight || level.Height % 2 != 0)
        {
            level.Height = EvenUp(Math.Max(level.Height, minHeight));
            adjusted = true;
        }
        if (!_backgrounds.Contains(level.Background))
        {
            warnings.Add($"Hintergrund '{level.Background}' unbrauchbar, ersetzt durch '{_backgrounds[0]}'.");
            level.Background = _backgrounds[0];
            adjusted = true;
        }
        if (adjusted)
            warnings.Add("Level wurde angepasst (Fenstergröße/Hintergrund), noch nicht gespeichert.");

        _level = level;
        _undo.Clear();
        _redo.Clear();
        _dirty = adjusted;
        EditorObject? spawn = Spawn();
        _cx = spawn?.X ?? 0;
        _cy = spawn?.Y ?? 0;
        _camX = 0;
        _camY = 0;
        RebuildAll();
        Msg($"Level {number + 1} geladen: {_level.Objects.Count} Objekte.");
        if (warnings.Count > 0)
            ShowReport("HINWEISE BEIM LADEN", warnings);
    }

    private void NewLevel(int number)
    {
        _level = new EditorLevel
        {
            Number = number,
            Width = Math.Max(Console.WindowWidth * 3, 240),
            Height = EvenUp(Math.Max(Console.WindowHeight * 4, 80)),
            Background = _backgrounds[0]
        };
        _undo.Clear();
        _redo.Clear();
        _dirty = true;
        _cx = 0;
        _cy = 0;
        _camX = 0;
        _camY = 0;
        RebuildAll();
        Msg($"Neues Level {number + 1}. Erst Größe und Hintergrund wählen.");
        ShowSettings();
    }

    private EditorLevel? ParseLevel(int number, out List<string> warnings)
    {
        warnings = [];
        string[] lines;
        try
        {
            lines = File.ReadAllLines(LevelPath(number));
        }
        catch (Exception e)
        {
            warnings.Add(e.Message);
            return null;
        }
        if (lines.Length == 0)
        {
            warnings.Add("Die Datei ist leer.");
            return null;
        }
        string[] head = lines[0].Split(',');
        if (head.Length != 3 || !TryInt(head, 0, out int width) || !TryInt(head, 1, out int height))
        {
            warnings.Add("Kopfzeile ungültig. Erwartet: Breite,Höhe,Hintergrund");
            return null;
        }
        var level = new EditorLevel { Number = number, Width = width, Height = height, Background = head[2].Trim() };
        for (int i = 1; i < lines.Length; i++)
        {
            if (string.IsNullOrWhiteSpace(lines[i]))
                continue;
            string[] parts = lines[i].Split(',').Select(s => s.Trim()).ToArray();
            EditorObject? o = ParseObject(parts, level, out string error);
            if (o is null)
                warnings.Add($"Zeile {i + 1}: {error}");
            else
                level.Objects.Add(o);
        }
        return level;
    }

    private EditorObject? ParseObject(string[] p, EditorLevel level, out string error)
    {
        error = "";
        ObjType type;
        switch (p[0])
        {
            case "SolidObj": type = ObjType.Solid; break;
            case "Decoration": type = ObjType.Decoration; break;
            case "Ladder": type = ObjType.Ladder; break;
            case "Bar": type = ObjType.Bar; break;
            case "Spike": type = ObjType.Spike; break;
            case "Laser": type = ObjType.Laser; break;
            case "ArrowMachine": type = ObjType.Arrow; break;
            case "Switch": type = ObjType.Switch; break;
            case "Key": type = ObjType.Key; break;
            case "Medkit": type = ObjType.Medkit; break;
            case "CheckPoint": type = ObjType.CheckPoint; break;
            case "Goal": type = ObjType.Goal; break;
            default:
                error = $"Unbekannter Typ '{p[0]}'.";
                return null;
        }
        if (!TryInt(p, 1, out int x) || !TryInt(p, 2, out int y))
        {
            error = "x/y fehlen oder sind keine Zahlen.";
            return null;
        }
        var o = new EditorObject { Type = type, X = x, Y = y };
        switch (type)
        {
            case ObjType.Solid:
            case ObjType.Decoration:
                if (p.Length < 7 || !TryInt(p, 3, out int layer) || !TryInt(p, 6, out int amount) || !Enum.TryParse(p[5], out Direction dir))
                {
                    error = "Erwartet: Typ,x,y,Layer,Textur,Richtung,Anzahl";
                    return null;
                }
                if (!HasTexture(p[4]))
                {
                    error = $"Textur '{p[4]}' nicht gefunden.";
                    return null;
                }
                if (dir is not (Direction.None or Direction.Right or Direction.Down))
                {
                    error = $"Richtung '{dir}' wird vom Loader nicht unterstützt (nur None, Right, Down).";
                    return null;
                }
                o.Layer = Math.Max(0, layer);
                o.Texture = p[4];
                o.Dir = dir;
                o.Amount = Math.Max(1, amount);
                Normalize(o);
                break;
            case ObjType.Ladder:
            case ObjType.Bar:
            case ObjType.Spike:
                if (!TryInt(p, 3, out int parts))
                {
                    error = "Anzahl fehlt.";
                    return null;
                }
                o.Amount = Math.Max(1, parts);
                break;
            case ObjType.Laser:
                if (!TryInt(p, 3, out int on) || !TryInt(p, 4, out int off))
                {
                    error = "Erwartet: Laser,x,y,EinMs,AusMs";
                    return null;
                }
                o.OnMs = Math.Max(1, on);
                o.OffMs = Math.Max(1, off);
                break;
            case ObjType.Arrow:
                if (p.Length < 5 || !Enum.TryParse(p[3], out Direction arrowDir) || !TryInt(p, 4, out int interval))
                {
                    error = "Erwartet: ArrowMachine,x,y,Richtung,TaktMs";
                    return null;
                }
                if (arrowDir is not (Direction.Left or Direction.Right))
                {
                    error = "Richtung muss Left oder Right sein.";
                    return null;
                }
                o.Dir = arrowDir;
                o.IntervalMs = Math.Max(1, interval);
                break;
            case ObjType.Switch:
                if (level.Objects.Count == 0)
                {
                    error = "Schalter ohne vorheriges Objekt.";
                    return null;
                }
                o.Target = level.Objects[^1];
                break;
        }
        return o;
    }

    private static bool TryInt(string[] p, int index, out int value)
    {
        value = 0;
        return index < p.Length && int.TryParse(p[index], NumberStyles.Integer, CultureInfo.InvariantCulture, out value);
    }

    private bool Save()
    {
        try
        {
            string path = LevelPath(_level.Number);
            Directory.CreateDirectory(ScenesFolder);
            if (File.Exists(path))
                File.Copy(path, path + ".bak", true);
            string text = _level.Serialize(out int skipped);
            File.WriteAllText(path, text, new UTF8Encoding(false));
            _dirty = false;
            int problems = Validate().Count;
            string note = problems > 0 ? $" {problems} Hinweis(e): Menü > Level prüfen." : "";
            if (skipped > 0)
                note += $" {skipped} Schalter ohne Ziel nicht gespeichert.";
            Msg($"Gespeichert: {Path.GetFullPath(path)}.{note}");
            return true;
        }
        catch (Exception e)
        {
            Msg("Speichern fehlgeschlagen: " + e.Message);
            return false;
        }
    }

    private List<string> Validate()
    {
        var result = new List<string>();
        int goals = _level.Alive.Count(o => o.Type == ObjType.Goal);
        if (goals != 1)
            result.Add($"Ziel: {goals} vorhanden, das Spiel verlangt genau 1.");
        if (!_level.Alive.Any(o => o.Type == ObjType.CheckPoint))
            result.Add("Kein Checkpoint: der Spieler-Start fehlt (erster Checkpoint = Start).");
        int loose = _level.Alive.Count(o => o.Type == ObjType.Switch && (o.Target is null || o.Target.Deleted));
        if (loose > 0)
            result.Add($"{loose} Schalter ohne Ziel (werden nicht gespeichert).");
        int outside = _level.Alive.Count(o => o.X < 0 || o.Y < 0 || o.X >= _level.Width || o.Y >= _level.Height);
        if (outside > 0)
            result.Add($"{outside} Objekt(e) außerhalb des Levels.");

        var floating = new List<EditorObject>();
        foreach (EditorObject o in _level.Alive)
        {
            if (o.Type is ObjType.Key or ObjType.Medkit or ObjType.CheckPoint && o.Main is not null)
            {
                if (o.Main!.GetDistanceToNextSprite(Direction.Down, true, true, false) > 0)
                    floating.Add(o);
            }
        }
        if (floating.Count > 0)
            result.Add($"{floating.Count} Sammelobjekt(e) schweben und fallen im Spiel, z. B. {TypeName(floating[0].Type)} bei {floating[0].X},{floating[0].Y}.");
        return result;
    }

    // ------------------------------------------------------------------ Menüs

    private void ShowModal(Modal modal)
    {
        modal.Index = 0;
        _modal = modal;
        _changed = true;
    }

    private void CloseModal()
    {
        _modal = null;
        _modalBox.Visible = false;
        _changed = true;
    }

    private void ShowLevelPicker(bool canCancel)
    {
        List<int> existing = ExistingLevels();
        var items = new List<ModalItem>();
        foreach (int n in existing)
        {
            int number = n;
            items.Add(new ModalItem(() => $"Level {number + 1}   (level{number}.txt)")
            {
                Activate = () => { CloseModal(); OpenLevel(number); }
            });
        }
        int next = existing.Count == 0 ? 0 : existing.Max() + 1;
        items.Add(new ModalItem(() => $"Neues Level   (level{next}.txt)")
        {
            Activate = () => { CloseModal(); NewLevel(next); }
        });
        ShowModal(new Modal("LEVEL WÄHLEN", items)
        {
            Info = ["Level 1 (level0.txt) ist auch der Menühintergrund."],
            OnEscape = () => { CloseModal(); if (!canCancel) _exit = true; }
        });
    }

    private void ShowMainMenu()
    {
        var items = new List<ModalItem>
        {
            new ModalItem(() => "Weiter") { Activate = CloseModal },
            new ModalItem(() => "Speichern") { Activate = () => { CloseModal(); Save(); } },
            new ModalItem(() => "Level-Einstellungen...") { Activate = () => { CloseModal(); ShowSettings(); } },
            new ModalItem(() => "Level prüfen")
            {
                Activate = () =>
                {
                    CloseModal();
                    List<string> problems = Validate();
                    ShowReport("PRÜFUNG", problems.Count == 0 ? ["Keine Probleme gefunden."] : problems);
                }
            },
            new ModalItem(() => "Anderes Level öffnen...") { Activate = () => { CloseModal(); ConfirmDiscard(() => ShowLevelPicker(true)); } },
            new ModalItem(() => "Editor beenden") { Activate = () => { CloseModal(); ConfirmDiscard(() => _exit = true); } }
        };
        ShowModal(new Modal("EDITOR-MENÜ", items) { OnEscape = CloseModal });
    }

    private void ShowSettings()
    {
        int w = _level.Width, h = _level.Height;
        string bg = _level.Background;
        int minWidth = Console.WindowWidth, minHeight = EvenUp(Console.WindowHeight * 2);
        var items = new List<ModalItem>
        {
            new ModalItem(() => $"Breite:       {w}")
            {
                Change = d => w = Math.Clamp(w + d * (Down("EdShift") ? 50 : 10), minWidth, 4000)
            },
            new ModalItem(() => $"Höhe:         {h}")
            {
                Change = d => h = EvenUp(Math.Clamp(h + d * (Down("EdShift") ? 20 : 2), minHeight, 2000))
            },
            new ModalItem(() => $"Hintergrund:  {bg}")
            {
                Change = d => bg = Cycle(_backgrounds, bg, d)
            },
            new ModalItem(() => "Übernehmen") { Activate = () => { CloseModal(); ApplySettings(w, h, bg); } },
            new ModalItem(() => "Abbrechen") { Activate = CloseModal }
        };
        ShowModal(new Modal("LEVEL-EINSTELLUNGEN", items)
        {
            Info = ["Links/Rechts ändert den Wert, Shift = große Schritte.", "Mindestgröße ist das Konsolenfenster."],
            OnEscape = CloseModal
        });
    }

    private void ApplySettings(int width, int height, string background)
    {
        if (width == _level.Width && height == _level.Height && background == _level.Background)
            return;
        PushUndo();
        _level.Width = width;
        _level.Height = height;
        _level.Background = background;
        _dirty = true;
        RebuildAll();
        Msg($"Level ist jetzt {width} x {height}.");
    }

    private void ConfirmDiscard(Action proceed)
    {
        if (!_dirty)
        {
            proceed();
            return;
        }
        var items = new List<ModalItem>
        {
            new ModalItem(() => "Speichern und fortfahren") { Activate = () => { CloseModal(); if (Save()) proceed(); } },
            new ModalItem(() => "Verwerfen und fortfahren") { Activate = () => { CloseModal(); proceed(); } },
            new ModalItem(() => "Abbrechen") { Activate = CloseModal }
        };
        ShowModal(new Modal("UNGESPEICHERTE ÄNDERUNGEN", items) { OnEscape = CloseModal });
    }

    private void ShowReport(string title, List<string> lines, Action? after = null)
    {
        var info = new List<string>();
        foreach (string line in lines)
        {
            foreach (string wrapped in Wrap(line, 60))
                info.Add(wrapped);
            if (info.Count >= 12)
                break;
        }
        if (info.Count > 12)
            info.RemoveRange(12, info.Count - 12);
        if (lines.Count > 1 && info.Count >= 12)
            info.Add("... (weitere Einträge ausgeblendet)");
        var items = new List<ModalItem>
        {
            new ModalItem(() => "OK") { Activate = () => { CloseModal(); after?.Invoke(); } }
        };
        ShowModal(new Modal(title, items)
        {
            Info = info,
            OnEscape = () => { CloseModal(); after?.Invoke(); }
        });
    }

    private void RenderModal()
    {
        Modal m = _modal!;
        var itemLines = new List<string>();
        for (int i = 0; i < m.Items.Count; i++)
            itemLines.Add((i == m.Index ? "> " : "  ") + m.Items[i].Label());

        const string hint = "Auf/Ab wählen   Links/Rechts ändern   Enter   Esc";
        int longest = Math.Max(hint.Length, m.Title.Length);
        longest = Math.Max(longest, itemLines.Max(l => l.Length));
        if (m.Info.Count > 0)
            longest = Math.Max(longest, m.Info.Max(l => l.Length));
        int width = Math.Clamp(longest + 4, 44, Math.Max(44, Console.WindowWidth - 4));

        var content = new List<string[]> { new[] { "!" + m.Title } };
        if (m.Info.Count > 0)
            content.Add(m.Info.Select(Safe).ToArray());
        content.Add(itemLines.ToArray());
        content.Add(new[] { "-" + hint });

        int rows = content.Sum(p => p.Length) + content.Count - 1;   // dazwischen je eine Trennzeile
        int height = rows + 2;
        _modalBox.UpdateSize(width, height);
        _modalBox.X = Math.Max(0, Console.WindowWidth / 2 - width / 2);
        _modalBox.Y = Math.Max(1, Console.WindowHeight / 2 - height / 2);
        _modalBox.UpdateContent(content.ToArray());
        _modalBox.Visible = true;
    }

    // ------------------------------------------------------------------ Panels

    private void BuildHelp()
    {
        string[] keys =
        [
            "Pfeile        Cursor bewegen (Shift 8x, Strg 32x)",
            "G             Raster 1 / 2 / 4 / 8",
            "Leertaste     Objekt platzieren",
            "Enter         Objekt greifen / ablegen",
            "Entf, X       Objekt unter dem Cursor löschen",
            "Tab           Objekt aufnehmen (Pipette)",
            "Q / E         Objekttyp wechseln",
            "A / D         Textur wechseln (Block, Deko)",
            "W / S         Größe +1 / -1 (Shift 8)",
            "R             Richtung drehen",
            "Bild auf/ab   Layer +1 / -1 (Block, Deko)",
            "U J           Laser Ein-Zeit / Pfeilwerfer Takt (+ / -)",
            "I K           Laser Aus-Zeit (+ / -)",
            "Y             Schalter neu mit Ziel verbinden",
            "N             Checkpoint als Spieler-Start setzen",
            "Strg+Z / +Y   Rückgängig / Wiederholen",
            "Strg+S, F5    Speichern",
            "P             Panels ein/aus",
            "H             Diese Hilfe",
            "Esc           Menü (Speichern, Einstellungen, Prüfen)"
        ];
        _helpBox.UpdateSize(66, keys.Length + 4);
        _helpBox.UpdateContent([["!TASTEN"], keys]);
    }

    private void UpdatePanels()
    {
        bool ready = _scene is not null;
        bool panels = ready && _panels && !_help;
        _statusBox.Visible = panels;
        _brushBox.Visible = panels;
        _messageBox.Visible = panels;
        _helpBox.Visible = ready && _help && _modal is null;
        if (!ready)
            return;

        _helpBox.X = Math.Max(0, Console.WindowWidth / 2 - _helpBox.Width / 2);

        _statusBox.UpdateContent(
        [
            [_dirty ? "!EDITOR *" : "!EDITOR"],
            [
                $"Level {_level.Number + 1}   {_level.Width} x {_level.Height}",
                $"Cursor {_cx},{_cy}   Raster {_grid}",
                $"Objekte {_level.Alive.Count()}   Undo {_undo.Count}"
            ]
        ]);

        EditorObject c = _held ?? _brush;
        string mode = _held is not null ? "Greifen (Enter legt ab)"
            : _linkSwitch is not null ? "Ziel wählen (Enter)"
            : "Platzieren (Leertaste)";
        var lines = new List<string>
        {
            "Modus: " + mode,
            $"Typ: {TypeName(c.Type)} ({Array.IndexOf(TypeOrder, c.Type) + 1}/{TypeOrder.Length})"
        };
        switch (c.Type)
        {
            case ObjType.Solid:
            case ObjType.Decoration:
                lines.Add($"Textur: {c.Texture}");
                lines.Add($"Layer {c.Layer}  Größe {c.Amount}  {(c.Amount > 1 ? c.Dir.ToString() : "-")}");
                break;
            case ObjType.Ladder:
            case ObjType.Bar:
            case ObjType.Spike:
                lines.Add($"Größe: {c.Amount}");
                break;
            case ObjType.Laser:
                lines.Add($"Ein {c.OnMs} ms   Aus {c.OffMs} ms");
                break;
            case ObjType.Arrow:
                lines.Add($"Richtung {c.Dir}   Takt {c.IntervalMs} ms");
                break;
            case ObjType.Switch:
                lines.Add("Danach Ziel-Objekt wählen");
                break;
            case ObjType.Key:
            case ObjType.Medkit:
            case ObjType.CheckPoint:
                lines.Add("Fällt im Spiel auf den Boden");
                break;
            case ObjType.Goal:
                lines.Add("Genau 1 pro Level");
                break;
        }
        EditorObject? hover = _modal is null ? ObjectAt(_cx, _cy, _linkSwitch) : null;
        lines.Add(hover is null ? "Unter Cursor: nichts" : $"Unter Cursor: {TypeName(hover.Type)}");
        _brushBox.UpdateContent([["!BRUSH"], lines.Select(Safe).ToArray()]);

        _messageBox.UpdateContent([["!INFO"], Wrap(_message, PanelWidth - 2).Take(3).Select(Safe).ToArray()]);
    }

    // ------------------------------------------------------------------ Helfer

    private void Msg(string text)
    {
        _message = text;
        _changed = true;
    }

    private static string TypeName(ObjType t) => t switch
    {
        ObjType.Solid => "Block (Solid)",
        ObjType.Decoration => "Deko",
        ObjType.Ladder => "Leiter",
        ObjType.Bar => "Stange (Bar)",
        ObjType.Spike => "Stacheln",
        ObjType.Laser => "Laser",
        ObjType.Arrow => "Pfeilwerfer",
        ObjType.Switch => "Schalter",
        ObjType.Key => "Schlüssel",
        ObjType.Medkit => "Medkit",
        ObjType.CheckPoint => "Checkpoint",
        ObjType.Goal => "Ziel",
        _ => t.ToString()
    };

    private static int EvenUp(int v) => v % 2 == 0 ? v : v + 1;

    private static string Cycle(List<string> list, string current, int d)
    {
        if (list.Count == 0)
            return current;
        int index = Math.Max(0, list.IndexOf(current));
        return list[(index + d + list.Count) % list.Count];
    }

    /// <summary>UIElement behandelt Zeilen mit '-', '!' oder dem Trennzeichen am Anfang speziell.</summary>
    private static string Safe(string s)
    {
        if (s.Length == 0)
            return " ";
        char c = s[0];
        return c is '-' or '!' or UIElement.Separator ? " " + s : s;
    }

    private static List<string> Wrap(string text, int width)
    {
        var lines = new List<string>();
        foreach (string paragraph in text.Split('\n'))
        {
            string line = "";
            foreach (string word in paragraph.Split(' ', StringSplitOptions.RemoveEmptyEntries))
            {
                if (line.Length > 0 && line.Length + 1 + word.Length <= width)
                {
                    line += " " + word;
                    continue;
                }
                if (line.Length > 0)
                    lines.Add(line);
                line = word;
                while (line.Length > width)
                {
                    lines.Add(line[..width]);
                    line = line[width..];
                }
            }
            if (line.Length > 0)
                lines.Add(line);
        }
        if (lines.Count == 0)
            lines.Add(" ");
        return lines;
    }
}

/// <summary>Alle Objekttypen, die der LevelLoader kennt.</summary>
internal enum ObjType
{
    Solid, Decoration, Ladder, Bar, Spike, Laser, Arrow, Switch, Key, Medkit, CheckPoint, Goal
}

/// <summary>Reines Vorschau-Sprite ohne Spiellogik (keine Kollision, keine Schwerkraft).</summary>
internal sealed class EditorSprite : Sprite
{
    public EditorSprite(int x, int y, RGB?[,] texture, int layer) : base(x, y, texture, layer, false, false)
    {
    }

    // Sprite hat jetzt eine abstrakte Clone-Methode; Editor-Sprites werden nie geklont.
    public override Sprite? Clone(SpriteManager sprites) => null;
}

/// <summary>Ein Objekt im Level (nur Daten + die zugehörigen Vorschau-Sprites).</summary>
internal sealed class EditorObject
{
    public ObjType Type;
    public int X, Y;
    public int Layer;                          // nur Block / Deko
    public string Texture = "";                // nur Block / Deko
    public Direction Dir = Direction.None;     // Block/Deko: None|Right|Down, Pfeilwerfer: Left|Right
    public int Amount = 1;                     // Erweiterung bei Block/Deko, Teile bei Leiter/Stange/Stacheln
    public int OnMs = 1000, OffMs = 1000;     // Laser
    public int IntervalMs = 1500;              // Pfeilwerfer
    public EditorObject? Target;               // nur Schalter: das verbundene Objekt
    public bool Deleted;
    public Sprite? Main, Ray;

    /// <summary>Kopiert nur die Daten (keine Sprites, kein Ziel, nicht gelöscht).</summary>
    public EditorObject Copy() => new()
    {
        Type = Type, X = X, Y = Y, Layer = Layer, Texture = Texture, Dir = Dir, Amount = Amount,
        OnMs = OnMs, OffMs = OffMs, IntervalMs = IntervalMs
    };
}

internal sealed class EditorLevel
{
    public int Number;
    public int Width = 300, Height = 100;
    public string Background = "";
    public readonly List<EditorObject> Objects = [];

    public IEnumerable<EditorObject> Alive => Objects.Where(o => !o.Deleted);

    /// <summary>Tiefe Kopie ohne gelöschte Objekte (für Undo/Redo).</summary>
    public EditorLevel Clone()
    {
        var copy = new EditorLevel { Number = Number, Width = Width, Height = Height, Background = Background };
        var map = new Dictionary<EditorObject, EditorObject>();
        foreach (EditorObject o in Objects)
        {
            if (o.Deleted)
                continue;
            EditorObject n = o.Copy();
            map[o] = n;
            copy.Objects.Add(n);
        }
        foreach (EditorObject o in Objects)
        {
            if (!o.Deleted && o.Target is not null && map.TryGetValue(o.Target, out EditorObject? target))
                map[o].Target = target;
        }
        return copy;
    }

    /// <summary>
    /// Erzeugt den Dateiinhalt im Format des LevelLoaders. Ein Schalter bindet im Loader an das
    /// zuletzt geladene Objekt, deshalb wird er direkt hinter sein Ziel geschrieben.
    /// </summary>
    public string Serialize(out int skippedSwitches)
    {
        var sb = new System.Text.StringBuilder();
        sb.Append(FormattableString.Invariant($"{Width},{Height},{Background}")).Append('\n');
        List<EditorObject> alive = Alive.ToList();
        var written = new HashSet<EditorObject>();
        skippedSwitches = 0;
        foreach (EditorObject o in alive)
        {
            if (o.Type == ObjType.Switch)
            {
                if (o.Target is null || o.Target.Deleted)
                    skippedSwitches++;
                continue;
            }
            Emit(o, alive, written, sb);
        }
        return sb.ToString();
    }

    private static void Emit(EditorObject o, List<EditorObject> alive, HashSet<EditorObject> written, System.Text.StringBuilder sb)
    {
        if (!written.Add(o))
            return;
        sb.Append(Line(o)).Append('\n');
        foreach (EditorObject s in alive)
        {
            if (s.Type == ObjType.Switch && ReferenceEquals(s.Target, o))
                Emit(s, alive, written, sb);
        }
    }

    private static string Line(EditorObject o) => o.Type switch
    {
        ObjType.Solid => FormattableString.Invariant($"SolidObj,{o.X},{o.Y},{o.Layer},{o.Texture},{o.Dir},{o.Amount}"),
        ObjType.Decoration => FormattableString.Invariant($"Decoration,{o.X},{o.Y},{o.Layer},{o.Texture},{o.Dir},{o.Amount}"),
        ObjType.Ladder => FormattableString.Invariant($"Ladder,{o.X},{o.Y},{o.Amount}"),
        ObjType.Bar => FormattableString.Invariant($"Bar,{o.X},{o.Y},{o.Amount}"),
        ObjType.Spike => FormattableString.Invariant($"Spike,{o.X},{o.Y},{o.Amount}"),
        ObjType.Laser => FormattableString.Invariant($"Laser,{o.X},{o.Y},{o.OnMs},{o.OffMs}"),
        ObjType.Arrow => FormattableString.Invariant($"ArrowMachine,{o.X},{o.Y},{o.Dir},{o.IntervalMs}"),
        ObjType.Switch => FormattableString.Invariant($"Switch,{o.X},{o.Y}"),
        ObjType.Key => FormattableString.Invariant($"Key,{o.X},{o.Y}"),
        ObjType.Medkit => FormattableString.Invariant($"Medkit,{o.X},{o.Y}"),
        ObjType.CheckPoint => FormattableString.Invariant($"CheckPoint,{o.X},{o.Y}"),
        ObjType.Goal => FormattableString.Invariant($"Goal,{o.X},{o.Y}"),
        _ => throw new InvalidOperationException($"Unbekannter Typ {o.Type}")
    };
}
