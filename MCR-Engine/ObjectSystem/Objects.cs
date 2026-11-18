using System.Collections;
using MCR_Engine.Core;

namespace MCR_Engine.ObjectSystem;

public class Objects : IEnumerable<GameObject>
{
    private readonly List<GameObject> _objects;
    private readonly Scene _scene;
    private readonly GameObject?[][,] _map;
    private float _deltaTime;
    internal Objects(Scene scene)
    {
        _objects = [];
        _scene = scene;
        _map = new GameObject[GameObject.MaxLayer][,];
        for (int i = GameObject.MinLayer; i < GameObject.MaxLayer; i++)
            _map[i] = new GameObject?[_scene.Width, _scene.Height];
        if (scene.Objects is null)
            return;
        foreach (var gameObject in scene.Objects)
        {
            GameObject? s = gameObject.Clone(this);
            if (s is null)
                continue;
            Add(s);
        }
    }
    public float DeltaTime => _deltaTime >= 0 ? _deltaTime : 0;
    public int Count => _objects.Count;
    public void Update(float deltaTime)
    {
        _deltaTime = deltaTime;
        MoveObjects();
        UpdateMap();
    }
    public GameObject Add(GameObject gameObject)
    {
        ArgumentNullException.ThrowIfNull(gameObject);
        gameObject.SetObjectManager(this);
        _objects.Add(gameObject);
        return  gameObject;
    }
    public GameObject Get(int index)
    {
        if (index < 0 || index >= _objects.Count)
            throw new ArgumentOutOfRangeException(nameof(index));
        return _objects[index];
    }
    public int GetIndex(GameObject gameObject)
    {
        for (int i = 0; i < Count; i++)
            if (gameObject == _objects[i])
                return i;
        return -1;
    }
    public bool Remove(GameObject gameObject) => _objects.Remove(gameObject);
    public Objects Clone() => new Objects(_scene);
    public bool Contains(GameObject gameObject) => _objects.Contains(gameObject);
    public T[] GetAllObjectsOfType<T>() => _objects.OfType<T>().ToArray();
    public GameObject? GetObjectMapPixel(int x, int y, Type[]? searchFor = null, bool onlyWithCollision = false)
    {
        if (!_scene.ValidPixelCoordinates(x, y))
            throw new ArgumentException($"Invalid pixel coordinates {x},{y}");
        for (int currentLayer = GameObject.MaxLayer - 1; currentLayer >= GameObject.MinLayer; currentLayer--)
        {
            GameObject? obj = _map[currentLayer][x, y];
            if (obj is null || (searchFor != null && !searchFor.Any(t => t.IsAssignableFrom(obj.GetType()))) || (onlyWithCollision && !obj.IsCollidable))
                continue;
            return _map[currentLayer][x, y];
        }
        return null;
    }
    internal bool ObjectOnScene(GameObject gameObject, Direction? searchingDirection = null)
    {
        if (searchingDirection is null)
            return _scene.ValidPixelCoordinates(gameObject.X, gameObject.Y) && 
                   _scene.ValidPixelCoordinates(gameObject.X + gameObject.Width, gameObject.Y + gameObject.Height);
        if (searchingDirection is Direction.Up or Direction.Down)
            return _scene.ValidPixelCoordinates(0, gameObject.Y) && 
                   _scene.ValidPixelCoordinates(0, gameObject.Y + gameObject.Height);
        if (searchingDirection is Direction.Left or Direction.Right)
            return _scene.ValidPixelCoordinates(gameObject.X, 0) &&
                   _scene.ValidPixelCoordinates(gameObject.X + gameObject.Width - 1, 0);
        throw new ArgumentException($"Invalid direction {searchingDirection}");
    }
    internal bool Collision(GameObject searchingGameObject, bool searchExact, out GameObject? collidedGameObject, Type[]? searchFor = null)
    {
        collidedGameObject = null;
        foreach (var currentGameObject in _objects)
        {
            if (searchFor is not null && !searchFor.Contains(currentGameObject.GetType()))
                continue;
            if (currentGameObject == searchingGameObject)
                continue;
            if (!currentGameObject.IsCollidable && searchFor is null)
                continue;
            if (!ObjectBordersTouching(currentGameObject, searchingGameObject))
                continue;
            collidedGameObject = currentGameObject;
            if (!searchExact || ObjectsOverlap(currentGameObject, searchingGameObject))
                return true;
        }
        return false;
    }
    internal bool Collision(GameObject searchingGameObject, GameObject searchFor, bool searchExact)
    {
        if (!searchFor.IsCollidable)
            return false;
        if (!ObjectBordersTouching(searchingGameObject, searchFor))
            return false;
        return !searchExact || ObjectsOverlap(searchingGameObject, searchFor);
    }
    internal int GetDistanceToNextObject(GameObject gameObject, Direction direction, bool onlyWithCollision, Type[]? searchFor = null)
    {
        if (direction is Direction.None)
            throw new ArgumentException("Invalid direction.");
        int x = gameObject.X + (direction is Direction.Right ? gameObject.Width - 1 : 0);
        int y = gameObject.Y + (direction is Direction.Down ? gameObject.Height - 1 : 0);
        bool upDown = direction is Direction.Up or Direction.Down;
        int distance = upDown ? gameObject.Width : gameObject.Height;
        int xAdd = 0, yAdd = 0;
        do
        {
            switch (direction)
            {
                case Direction.Up: yAdd--; break;
                case Direction.Down: yAdd++; break;
                case Direction.Left: xAdd--; break;
                case Direction.Right: xAdd++; break;
            }
            for (int add = 0; add < distance; add++)
            {
                int cX = x + xAdd + (upDown ? add : 0);
                int cY = y + yAdd + (upDown ? 0 : add);
                if (!_scene.ValidPixelCoordinates(cX, cY))
                {
                    if (searchFor.Contains(typeof(Scene)))
                        return Math.Abs(xAdd) + Math.Abs(yAdd) - 1;
                    return -1;
                }
                GameObject? obj = GetObjectMapPixel(cX, cY, searchFor, onlyWithCollision);
                if (obj is not null && (searchFor is null || searchFor.Any(t => t.IsInstanceOfType(obj!))) &&
                (!onlyWithCollision || obj.IsCollidable) && !gameObject.Equals(obj))
                    return Math.Abs(xAdd) + Math.Abs(yAdd) - 1;
            }
        } while (true);
    }
    internal GameObject? GetNearestObject(GameObject gameObject, Direction direction, Type[] searchFor)
    {
        if (searchFor.Length == 0 || direction is Direction.None)
            throw new ArgumentException("The values are invalid.");
        int x = gameObject.X + (direction is Direction.Right ? gameObject.Width - 1 : 0);
        int y = gameObject.Y + (direction is Direction.Down ? gameObject.Height - 1 : 0);
        bool upDown = direction is Direction.Up or Direction.Down;
        int distance = upDown ? gameObject.Width : gameObject.Height;
        do
        {
            switch (direction)
            {
                case Direction.Up: y--; break;
                case Direction.Down: y++; break;
                case Direction.Left: x--; break;
                case Direction.Right: x++; break;
            }
            for (int add = 0; add < distance; add++)
            {
                int cX = x + (upDown ? add : 0);
                int cY = y + (upDown ? 0 : add);
                if (!_scene.ValidPixelCoordinates(cX, cY))
                    return null;
                GameObject? obj = GetObjectMapPixel(cX, cY, searchFor);
                if (obj is not null && !gameObject.Equals(obj) && searchFor.Any(type => type.IsInstanceOfType(obj!)))
                    return obj;
            }
        } while (true);
    }
    private void MoveObjects()
    {
        foreach (GameObject gameObject in _objects)
            gameObject.TickUpdate();
    }
    private void UpdateMap()
    {
        ClearMap();
        foreach (GameObject gameObject in _objects)
        {
            for (int y = 0; y < gameObject.Height; y++)
            {
                for (int x = 0; x < gameObject.Width; x++)
                {
                    int xObject = gameObject.X + x, yObject = gameObject.Y + y;
                    if (!_scene.ValidPixelCoordinates(xObject, yObject) || gameObject.CompareTo(_map[gameObject.Layer][xObject, yObject]) != 1)
                        continue;
                    if (gameObject.GetColor(x, y) is null || !gameObject.Visible)
                        continue;
                    _map[gameObject.Layer][xObject, yObject] = gameObject;
                }
            }
        }
    }
    private void ClearMap()
    {
        int sW = _scene.Width, sH = _scene.Height;
        for (int layer = GameObject.MinLayer; layer < GameObject.MaxLayer; layer++)
            for (int y = 0; y < sH; y++)
                for (int x = 0; x < sW; x++)
                    _map[layer][x, y] = null;
    }
    private static bool ObjectsOverlap(GameObject s1, GameObject s2)
    {
        for (int y = 0; y < s1.Height; y++)
        {
            for (int x = 0; x < s1.Width; x++)
            {
                if (s1.GetColor(x, y) != null && s2.CoordinatesOnObject(s1.X + x, s1.Y + y) && s2.GetColor(s1.X - s2.X + x, s1.Y - s2.Y + y) != null)
                    return true;
            }
        }
        return false;
    }
    private static bool ObjectBordersTouching(GameObject o1, GameObject o2)
    {
        return o1.X + o1.Width > o2.X && o1.Y + o1.Height > o2.Y && o1.X < o2.X + o2.Width && o1.Y < o2.Y + o2.Height;
    }
    public IEnumerator<GameObject> GetEnumerator() => new ObjectsEnumerator(_objects);
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}