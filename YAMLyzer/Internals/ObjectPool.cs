using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YAMLyzer.Interfaces;

namespace YAMLyzer.Internals;

internal class ObjectPool<T> where T: IClearable, new() {
    [ThreadStatic]
    private static ObjectPool<T> _shared = null!;

    private readonly List<T> _objects = null!;
    private int _currentRentedObjectCount = 0;

    public static ObjectPool<T> Shared { get => _shared ??= new ObjectPool<T>(); }

    public ObjectPool(int initialPoolSize = 4)
        => this._objects = new List<T>(capacity: 4);

    public T Rent() {
        if (_currentRentedObjectCount <= _objects.Count)
            _objects.Add(item: new T());
                
        return _objects[_currentRentedObjectCount++];
    }

    public void Return(T @object) {
        --_currentRentedObjectCount;
        @object.Clear();
    }
}
