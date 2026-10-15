using System.Collections;
using MCR_Engine.Core;

namespace MCR_Engine.Objects;

public class SpriteManager : IEnumerable<Sprite>
    {
        private readonly List<Sprite> _sprites;
        private readonly Scene _scene;
        private readonly Sprite?[,] _map;
        private double _deltaTime;
        internal SpriteManager(Scene scene)
        {
            _sprites = [];
            _scene = scene;
            _map = new Sprite?[_scene.Width, _scene.Height];
        }
        private SpriteManager(Engine engine, Scene scene)
        {
            _sprites = [];
            _scene = scene;
            _map = new Sprite?[_scene.Width, _scene.Height];
            foreach (var sprite in scene.Sprites)
            {
                Sprite? s = sprite.Clone(this);
                if (s is null)
                    continue;
                Add(s);
            }
        }
        public double DeltaTime => _deltaTime >= 0 ? _deltaTime : 0;
        public int Count => _sprites.Count;
        public void Update(double deltaTime)
        {
            _deltaTime = deltaTime;
            MoveSprites();
            UpdateMap();
        }
        public Sprite Add(Sprite sprite)
        {
            ArgumentNullException.ThrowIfNull(sprite);
            sprite.Sprites = this;
            _sprites.Add(sprite);
            return  sprite;
        }
        public Sprite Get(int index)
        {
            if (index < 0 || index >= _sprites.Count)
                throw new ArgumentOutOfRangeException(nameof(index));
            return _sprites[index];
        }
        public int GetIndex(Sprite sprite)
        {
            for (int i = 0; i < Count; i++)
                if (sprite == _sprites[i])
                    return i;
            return -1;
        }
        public bool SpriteOnScene(Sprite sprite) => _scene.ValidPixelCoordinates(sprite.X, sprite.Y);
        public Sprite? GetSpriteMapPixel(int x, int y) => _scene.ValidPixelCoordinates(x, y) ? _map[x, y] : throw new ArgumentException("The specified coordinates are invalid.");
        public bool Collision(Sprite searchingSprite, bool searchExact, out Sprite? collidedSprite, Type[]? searchFor = null)
        {
            collidedSprite = null;
            foreach (var currentSprite in _sprites)
            {
                if (searchFor is not null && !searchFor.Contains(currentSprite.GetType()))
                    continue;
                if (currentSprite == searchingSprite)
                    continue;
                if (!currentSprite.Collision)
                    continue;
                if (!SpriteBordersTouching(currentSprite, searchingSprite))
                    continue;
                collidedSprite = currentSprite;
                if (!searchExact || SpritesOverlap(currentSprite, searchingSprite))
                    return true;
            }
            return false;
        }
        public int GetDistanceToNextSprite(Sprite sprite, Direction direction, bool onlyOnScene, bool onlyWithCollision, bool startOppositeSide, Type[]? searchFor = null)
        {
            int xStart = sprite.X + (direction is Direction.Right && !startOppositeSide ? sprite.Width - 1 : 0), 
                yStart = sprite.Y + (direction is Direction.Down && !startOppositeSide ? sprite.Height - 1 : 0);
            int xAdd = 0, yAdd = 0;
            bool quit = false, upDown = direction is Direction.Up or Direction.Down;
            int distance = upDown ? sprite.Width : sprite.Height;
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
        public bool SpriteBordersTouching(Sprite s1, Sprite s2) => s1.X + s1.Width > s2.X && s1.Y + s1.Height > s2.Y && s1.X < s2.X + s2.Width && s1.Y < s2.Y + s2.Height;
        public SpriteManager Clone(Engine engine) => new SpriteManager(engine, _scene);
        public bool Contains(Sprite sprite) => _sprites.Contains(sprite);
        public List<T> GetAllSpritesOfType<T>() => _sprites.OfType<T>().ToList();
        private void MoveSprites()
        {
            foreach (var sprite in _sprites)
                sprite.UpdatePosition();
        }
        private void UpdateMap()
        {
            ClearMap();
            List<Sprite> order = new List<Sprite>(_sprites);
            order.Sort();
            foreach (var sprite in order)
            {
                for (int y = 0; y < sprite.Height; y++)
                {
                    for (int x = 0; x < sprite.Width; x++)
                    {
                        int xSprite = sprite.X + x, ySprite = sprite.Y + y;
                        if (!_scene.ValidPixelCoordinates(xSprite, ySprite) || sprite.GetColor(x, y) is null || !sprite.Visible)
                            continue;
                        _map[xSprite, ySprite] = sprite;
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
        public static bool SpritesOverlap(Sprite s1, Sprite s2)
        {
            for (int y = 0; y < s1.Height; y++)
            {
                for (int x = 0; x < s1.Width; x++)
                {
                    if (s1.GetColor(x, y) != null && s2.CoordinatesOnSprite(s1.X + x, s1.Y + y) && s2.GetColor(s1.X - s2.X + x, s1.Y - s2.Y + y) != null)
                        return true;
                }
            }
            return false;
        }
        public IEnumerator<Sprite> GetEnumerator() => new SpriteManagerIEnumerator(_sprites);
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }