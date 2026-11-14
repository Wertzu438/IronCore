using JumpOn.Core;
using JumpOn.Interactables;
using MCR_Engine.Core;
using MCR_Engine.ObjectSystem;
using SolidObject = MCR_Engine.Core.BaseObjectTypes.SolidObject;

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
    public (Direction, PathType) Find(GameObject searcher, GameObject destination)
    {
        bool reached = false;
        return Step(searcher.X, searcher.Y + searcher.Height - 1, new HashSet<(int x, int y)>(), destination, 0, ref reached);
    }
    private (Direction, PathType) Step(int x , int y, HashSet<(int x, int y)> visited, GameObject destination, int depth, ref bool reached)
    {
        if (!visited.Add((x, y)))
            return (Direction.None, PathType.Walk);
        for (int i = 0; i < Directions.Length; i++)
        {
            if (depth > 150 || !_scene.ValidPixelCoordinates(x, y) || _map[x, y] == null)
            {
                return (Directions[i], PathType.Walk);
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
            if (destination.CoordinatesOnObject(newX, newY))
            {
                reached = true;
            }
            else Step(newX, newY, visited, destination, depth + 1, ref reached);
            if (reached)
            {
                return (Directions[i], (PathType)_map[x, y]);
            }
        }
        return (Direction.None, PathType.Walk);
    }
    private void FillMap()
    {
        int width = _map.GetLength(0);
        int height = _map.GetLength(1);
        for (int y = 0; y < height; y++)
            for (int x = 0; x < width; x++)
                _map[x, y] = null;
        foreach (GameObject sprite in _scene.Objects)
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
                if (ConnectToLadder(sprite.X + sprite.Width, sprite.Y, 10))
                {
                    for (int xAdd = 0; xAdd < 4; xAdd++)
                    {
                        _map[sprite.X + sprite.Width + xAdd - 1, sprite.Y - 2] = PathType.Ladder;
                    }
                }
                else
                {
                    for (int yAdd = -1; yAdd < 10; yAdd++)
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
            }
            else
            {
                for (int yAdd = 0; yAdd < sprite.Height; yAdd++)
                {
                    if (!ChangeMapPixel(sprite.X + 2, sprite.Y + yAdd))
                        continue;
                    _map[sprite.X + 1, sprite.Y + yAdd] = PathType.Ladder;
                }
                for (int xAdd = 0; xAdd < sprite.Width; xAdd++)
                {
                    _map[sprite.X + xAdd, sprite.Y] = PathType.Walk;
                }
            }
        }
    }
    private bool ChangeMapPixel(int x, int y)
    {
        if (!_scene.ValidPixelCoordinates(x, y))
            return false;
        GameObject? sprite = _scene.Objects.GetObjectMapPixel(x, y);
        if (sprite != null && sprite.Collision)
            return false;
        return true;
    }
    private bool ConnectToLadder(int x, int y, int height)
    {
        for (int yAdd = -1; yAdd < height; yAdd++)
        {
            if (!_scene.ValidPixelCoordinates(x, y))
                continue;
            GameObject? sprite = _scene.Objects.GetObjectMapPixel(x, y);
            if (sprite == null)
                continue;
            if (sprite.GetType() == typeof(Ladder))
                return true;
        }
        return false;
    }
    private bool SideBorder(GameObject gameObject) => gameObject.Height < 10;
}