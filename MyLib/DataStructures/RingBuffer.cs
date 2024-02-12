namespace MyLib.DataStructures;

public class RingBuffer<T> 
{
    private T[] _data;
    private int _startIndex;
    private int? _endIndex = null;
    
    public RingBuffer(int size)
    {
        _data = new T[size];
    }

    public T this[int index] => _data[(_startIndex + index) % _data.Length];

    public IEnumerable<T> GetRange(int index, int count)
    {
        var range = new List<T>();
        var startIndex = _startIndex + index;
        for (var i = index; i < index + count; i++)
        {
            range.Add(_data[(startIndex + i) % _data.Length]);
        }

        return range;
    }

    public void Push(T value)
    {
        if (_endIndex.HasValue)
        {
            _endIndex = _startIndex;
        }
        else
        {
            _startIndex = (_startIndex - 1 + _data.Length) % _data.Length;
            if (_startIndex == _endIndex)
            {
                _endIndex = (_endIndex - 1 + _data.Length) % _data.Length;
            }
        }

        _data[_startIndex] = value;
    }

    public void PushMany(IEnumerable<T> values)
    {
        foreach (var value in values)
        {
            Push(value);
        }
    }

    public T? Pop()
    {
        if (!_endIndex.HasValue)
            return default;

        var value = _data[_endIndex.Value];

        if (_endIndex == _startIndex)
        {
            _endIndex = null;
        }
        else
        {
            _endIndex = (_endIndex - 1 + Capacity) % Capacity;
        }

        return value;
    }
    
    public bool IsEmpty() => !_endIndex.HasValue;
    public bool IsFull() => Count == Capacity;

    public int Capacity => _data.Length;
    
    public int Count {
        get
        {
            if (!_endIndex.HasValue) return 0;
            
            if (_startIndex < _endIndex)
                return _endIndex.Value - _startIndex + 1;
            
            if (_startIndex == _endIndex)
                return 1;
            
            return Capacity - _startIndex + _endIndex.Value + 1;
        }
    }
}