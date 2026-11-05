using System.Collections;

namespace MCR_Engine.Objects;

public class ObjectsEnumerator : IEnumerator<GameObject>
{
    private readonly List<GameObject> _objects;
    private int _index;
    public ObjectsEnumerator(List<GameObject> objects)
    {
        _objects = objects;
        Reset();
    }
    public GameObject Current => _objects[_index];
    object? IEnumerator.Current => Current;
    public bool MoveNext()
    {
        return ++_index < _objects.Count;
    }
    public void Dispose()
    {
        return;
    }
    public void Reset() => _index = -1;
}