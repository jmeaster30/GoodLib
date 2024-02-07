using System.Collections;

namespace MyLib.Enumerables;

public enum Ordering
{
    Row,
    Column,
}

public class Index2dEnumerable : IEnumerable<(uint, uint)>
{
    private readonly Index2dEnumerator _enumerator;
    
    public Index2dEnumerable(uint width, uint height, Ordering ordering = Ordering.Row)
    {
        _enumerator = new Index2dEnumerator(width, height, ordering);
    }
    
    public IEnumerator<(uint, uint)> GetEnumerator()
    {
        return _enumerator;
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}

public class Index2dEnumerator : IEnumerator<(uint, uint)>
{
    private readonly uint _width;
    private readonly uint _height;
    private readonly Ordering _ordering;

    private uint _x;
    private uint _y;

    public Index2dEnumerator(uint width, uint height, Ordering ordering)
    {
        _width = width;
        _height = height;
        _ordering = ordering;
        _x = 0;
        _y = 0;
    }
    
    public bool MoveNext()
    {
        bool result = false;
        switch (_ordering) 
        {
            case Ordering.Column:
            {
                _y += 1;
                if (_y >= _height)
                {
                    _y = 0;
                    _x += 1;
                }
                result = _x < _width;
                break;
            }
            case Ordering.Row:
            {
                _x += 1;
                if (_x >= _width)
                {
                    _x = 0;
                    _y += 1;
                }
                result = _y < _height;
                break;
            }
        }

        return result;
    }

    public void Reset()
    {
        _x = 0;
        _y = 0;
    }

    public (uint, uint) Current => (_x, _y);

    object IEnumerator.Current => Current;

    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }
}