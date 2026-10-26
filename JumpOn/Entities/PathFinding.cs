using JumpOn.Core;
using JumpOn.Interactables;
using MCR_Engine.Core;
using MCR_Engine.Objects;

namespace JumpOn.Entities;

public class PathFinding
{
    private static readonly Direction[] Directions = [
        Direction.Left, Direction.Right, Direction.Up, Direction.Down
    ];
    private readonly Scene _scene;
    private readonly PathType?[,] _map;
    public  PathFinding(Scene scene)
    {
        _scene = scene;
        _map = new PathType?[_scene.Width, _scene.Height];
        FillMap();
    }
    public PathType?[,] Map => _map;
    public Direction Find(Sprite searcher, Sprite destination)
    {
        bool reached = false;
        return Step(searcher.X, searcher.Y + searcher.Height - 1, new HashSet<(int x, int y)>(), destination, 0, ref reached);
    }
    private Direction Step(int x , int y, HashSet<(int x, int y)> visited, Sprite destination, int depth, ref bool reached)
    {
        if (!visited.Add((x, y)))
            return Direction.None;
        for (int i = 0; i < Directions.Length; i++)
        {
            
            if (depth > 150 || !_scene.ValidPixelCoordinates(x, y) || _map[x, y] == null)
            {
                return Directions[i];
            }
            int newX = x + (Directions[i]) switch
            {
                Direction.Left => -1,
                Direction.Right => 1,
                _ => 0,
            };
            int newY = y + (Directions[i]) switch
            {
                Direction.Up => -1,
                Direction.Down => 1,
                _ => 0,
            };
            if (destination.CoordinatesOnSprite(newX, newY))
            {
                reached = true;
            }
            else Step(newX, newY, visited, destination, depth + 1, ref reached);
            if (reached)
            {
                return Directions[i];
            }
        }
        return Direction.None;
    }
    private void FillMap()
    {
        int width = _map.GetLength(0);
        int height = _map.GetLength(1);
        for (int y = 0; y < height; y++)
            for (int x = 0; x < width; x++)
                _map[x, y] = null;
        foreach (Sprite sprite in _scene.Sprites)
        {
            Type spriteType = sprite.GetType();
            if (spriteType != typeof(SolidObject) && spriteType != typeof(Ladder))
                continue;
            if (spriteType == typeof(SolidObject))
            {
                for (int xAdd = 0; xAdd < sprite.Width; xAdd++)
                {
                    int x = sprite.X + xAdd, y = sprite.Y - 1;
                    if (!ChangeMapPixel(x,y))
                        continue;
                    _map[x, y] = PathType.Walk;
                }
                if (!SideBorder(sprite))
                    continue;
                for (int yAdd = -1; yAdd < sprite.Height; yAdd++)
                {
                    int x = sprite.X - 1, y = sprite.Y + yAdd;
                    if (ChangeMapPixel(x, y))
                    {
                        _map[x, y] = PathType.Drop;
                    }
                    x = sprite.X + sprite.Width;
                    if (ChangeMapPixel(x, y))
                        _map[x, y] = PathType.Drop;
                }
            }
            else
            {
                for (int yAdd = 0; yAdd < sprite.Height; yAdd++)
                {
                    if (!ChangeMapPixel(sprite.X + 2, sprite.Y + yAdd))
                        continue;
                    _map[sprite.X + 2, sprite.Y + yAdd] = PathType.Ladder;
                }
                for (int xAdd = 0; xAdd < sprite.Width; xAdd++)
                {
                    _map[sprite.X + xAdd, sprite.Y] = PathType.Ladder;
                }
            }
        }
    }
    private bool ChangeMapPixel(int x, int y)
    {
        if (!_scene.ValidPixelCoordinates(x, y))
            return false;
        Sprite? sprite = _scene.Sprites.GetSpriteMapPixel(x, y);
        if (sprite != null && sprite.Collision)
            return false;
        return true;
    }
    private bool ConnectToLadder(int x, int y)
    {
        Sprite? sprite = _scene.Sprites.GetSpriteMapPixel(x, y);
        if (sprite == null)
            return false;
        return sprite.GetType() == typeof(Ladder);
    }
    private bool SideBorder(Sprite sprite) => sprite.Height < 10;
}