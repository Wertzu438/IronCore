using MCR_Engine.Core;

namespace MCR_Engine.Objects;

public class SpriteManager
    {
        private readonly HashSet<Sprite> _sprites;
        private readonly Scene _scene;
        private readonly Sprite?[,] _map;
        private double _deltaTime;
        internal  SpriteManager(Scene scene)
        {
            _sprites = [];
            _scene = scene;
            _map = new Sprite?[_scene.Width, _scene.Height];
        }
        public double DeltaTime => _deltaTime is not 0 ? _deltaTime : throw new InvalidOperationException("The delta time shouldn't be zero.");
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
        public Sprite? GetSpriteMapPixel(int x, int y) => _scene.ValidPixelCoordinates(x, y) ? _map[x, y] : throw new ArgumentException("The specified coordinates are invalid.");
        public bool Collision(Sprite searchingSprite, out Sprite? collidedSprite, Type[]? searchFor = null)
        {
            foreach (var currentSprite in _sprites)
            {
                if (searchFor is not null && !searchFor.Contains(currentSprite.GetType()))
                    continue;
                if (currentSprite == searchingSprite)
                    continue;
                if (!currentSprite.Collision)
                    continue;
                if (/*!SpritesOverlap(currentSprite, searchingSprite)*/ !SpriteBordersTouching(currentSprite, searchingSprite))
                    continue;
                collidedSprite = currentSprite;
                return true;
            }
            collidedSprite = null;
            return false;
        }
        public int GetDistanceToNextSprite(Sprite sprite, Direction direction, bool onlyWithCollision, bool startAtTop,  Type[]? searchFor = null)
        {
            int xStart = sprite.X + (direction is Direction.Right && !startAtTop ? sprite.Width - 1 : 0), yStart = sprite.Y+ (direction is Direction.Down && !startAtTop ? sprite.Height - 1 : 0);
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
                int x = xStart + xAdd, y = yStart + yAdd;
                if (!_scene.ValidPixelCoordinates(x, y) || (_map[x, y] is not null && (searchFor is null || searchFor.Contains(_map[x, y].GetType())) && (!onlyWithCollision || _map[x, y].Collision)))
                    break;
            } while (true);
            return Math.Abs(xAdd) + Math.Abs(yAdd) - 1;
        }
        /*public int GetDistanceToNextSprite(Sprite sprite, Direction direction, bool onlyWithCollision, bool startAtTop, Type[]? searchFor = null)
        {
            int xStart = sprite.X;
            int yStart = sprite.Y;
            int start = xStart, distance = sprite.Width;
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
                for (int i = start; i < distance; i++)
                {
                    int x = xStart + xAdd + i, y = yStart + yAdd;
                    if (!_scene.ValidPixelCoordinates(x, y) || (_map[x, y] is not null && (searchFor is null || searchFor.Contains(_map[x, y].GetType())) && (!onlyWithCollision || _map[x, y].Collision)))
                        break;
                }
            } while (true);
        }*/
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
            List<Sprite> order = OrderSprites();
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
        private List<Sprite> OrderSprites()
        {
            List<Sprite> order = [];
            int currentLayer = 0;
            do
            {
                foreach (var sprite in _sprites)
                {
                    if (sprite.Layer !=  currentLayer)
                        continue;
                    order.Add(sprite);
                }
                currentLayer++;
            } while (order.Count < _sprites.Count);
            return order;
        }
        private void ClearMap()
        {
            for (int y = 0; y < _scene.Height; y++)
                for (int x = 0; x < _scene.Width; x++)
                    _map[x, y] = null;
        }
        public bool SpriteBordersTouching(Sprite s1, Sprite s2) => s1.X + s1.Width > s2.X && s1.Y + s1.Height > s2.Y && s1.X < s2.X + s2.Width && s1.Y < s2.Y + s2.Height;
        public static bool SpritesOverlap(Sprite s1, Sprite s2)
        {
            for (int y = 0; y < s1.Height; y++)
            {
                for (int x = 0; x < s1.Width; x++)
                {
                    if (s2.CoordinatesOnSprite(s1.X + x, s1.Y + y) && s2.GetColor(s1.X - s2.X + x, s1.Y - s2.Y + y) != null)
                        return true;
                }
            }
            return false;
        }
    }