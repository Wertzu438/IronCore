using System.Collections;

namespace MCR_Engine.Objects;

public class SpriteManagerIEnumerator : IEnumerator<Sprite>
{
    private readonly List<Sprite> _sprites;
    private int _index;
    public SpriteManagerIEnumerator(List<Sprite> sprites)
    {
        _sprites = sprites;
        Reset();
    }
    public Sprite Current => _sprites[_index];
    object? IEnumerator.Current => Current;
    public bool MoveNext()
    {
        return ++_index < _sprites.Count;
    }
    public void Dispose()
    {
        return;
    }
    public void Reset() => _index = -1;
}