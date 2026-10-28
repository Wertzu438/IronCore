using System.Collections;
using MCR_Engine.Core;

namespace MCR_Engine.Objects;

public class ObjectManager : IEnumerable<GameObject>
    {
        private readonly List<GameObject> _objects;
        private readonly Scene _scene;
        private readonly GameObject?[,] _map;
        private float _deltaTime;
        internal ObjectManager(Scene scene)
        {
            _objects = [];
            _scene = scene;
            _map = new GameObject?[_scene.Width, _scene.Height];
        }
        private ObjectManager(Scene scene, bool clone)
        {
            _objects = [];
            _scene = scene;
            _map = new GameObject?[_scene.Width, _scene.Height];
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
        public bool ObjectOnScene(GameObject gameObject, Direction? searchingDirection = null)
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
        public GameObject? GetObjectMapPixel(int x, int y) => _scene.ValidPixelCoordinates(x, y) ? _map[x, y] : throw new ArgumentException("The specified coordinates are invalid.");
        public bool Collision(GameObject searchingGameObject, bool searchExact, out GameObject? collidedGameObject, Type[]? searchFor = null)
        {
            collidedGameObject = null;
            foreach (var currentGameObject in _objects)
            {
                if (searchFor is not null && !searchFor.Contains(currentGameObject.GetType()))
                    continue;
                if (currentGameObject == searchingGameObject)
                    continue;
                if (!currentGameObject.Collision && searchFor is null)
                    continue;
                if (!ObjectBordersTouching(currentGameObject, searchingGameObject))
                    continue;
                collidedGameObject = currentGameObject;
                if (!searchExact || ObjectsOverlap(currentGameObject, searchingGameObject))
                    return true;
            }
            return false;
        }
        public bool Collision(GameObject searchingGameObject, GameObject searchFor, bool searchExact)
        {
            if (!searchFor.Collision)
                return false;
            if (!ObjectBordersTouching(searchingGameObject, searchFor))
                return false;
            if (!searchExact || ObjectsOverlap(searchingGameObject, searchFor))
                return true;
            return false;
        }
        public int GetDistanceToNextObject(GameObject gameObject, Direction direction, bool onlyOnScene, bool onlyWithCollision, bool startOppositeSide, Type[]? searchFor = null)
        {
            int xStart = gameObject.X + (direction is Direction.Right && !startOppositeSide ? gameObject.Width - 1 : 0), 
                yStart = gameObject.Y + (direction is Direction.Down && !startOppositeSide ? gameObject.Height - 1 : 0);
            int xAdd = 0, yAdd = 0;
            bool quit = false, upDown = direction is Direction.Up or Direction.Down;
            int distance = upDown ? gameObject.Width : gameObject.Height;
            do
            {
                switch (direction)
                {
                    case Direction.Up: yAdd--; break;
                    case Direction.Down: yAdd++; break;
                    case Direction.Left: xAdd--; break;
                    case Direction.Right: xAdd++; break;
                }
                for (int i = 0; i < distance; i++)
                {
                    int x = xStart + xAdd + (upDown ? i : 0), y = yStart + yAdd+ (!upDown ? i : 0);
                    if (!_scene.ValidPixelCoordinates(x, y))
                    {
                        if (onlyOnScene)
                        {
                            quit = true;
                            break;
                        }
                        return -1;
                    }
                    if (_map[x, y] is not null && (searchFor is null || searchFor.Contains(_map[x, y]!.GetType())) &&
                    (!onlyWithCollision || _map[x, y]!.Collision))
                    {
                        quit = true;
                        break;
                    }
                }
            } while (!quit);
            return Math.Abs(xAdd) + Math.Abs(yAdd) - 1;
        }
        public bool ObjectBordersTouching(GameObject o1, GameObject o2) => o1.X + o1.Width > o2.X && o1.Y + o1.Height > o2.Y && o1.X < o2.X + o2.Width && o1.Y < o2.Y + o2.Height;
        public ObjectManager Clone() => new ObjectManager(_scene, true);
        public bool Contains(GameObject gameObject) => _objects.Contains(gameObject);
        public List<T> GetAllObjectsOfType<T>() => _objects.OfType<T>().ToList();
        private void MoveObjects()
        {
            foreach (GameObject gameObject in _objects)
                gameObject.UpdatePosition();
        }
        private void UpdateMap()
        {
            ClearMap();
            List<GameObject> order = new List<GameObject>(_objects);
            order.Sort();
            foreach (GameObject gameObject in order)
            {
                for (int y = 0; y < gameObject.Height; y++)
                {
                    for (int x = 0; x < gameObject.Width; x++)
                    {
                        int xObject = gameObject.X + x, yObject = gameObject.Y + y;
                        if (!_scene.ValidPixelCoordinates(xObject, yObject) || gameObject.GetColor(x, y) is null || !gameObject.Visible)
                            continue;
                        _map[xObject, yObject] = gameObject;
                    }
                }
            }
        }
        private void ClearMap()
        {
            for (int y = 0; y < _scene.Height; y++)
                for (int x = 0; x < _scene.Width; x++)
                    _map[x, y] = null;
        }
        public static bool ObjectsOverlap(GameObject s1, GameObject s2)
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
        public IEnumerator<GameObject> GetEnumerator() => new ObjectManagerIEnumerator(_objects);
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }