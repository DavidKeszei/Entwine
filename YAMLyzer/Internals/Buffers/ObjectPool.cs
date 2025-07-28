using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YAMLyzer.Buffers;

/// <summary>
/// Represent a pool of reusable <typeparamref name="T"/> instances.
/// </summary>
/// <typeparam name="T">Type of the pool instances. Must be implement the <see cref="IClearable"/> interface and must have a parameterless constructor.</typeparam>
internal class ObjectPool<T> where T: class, IClearable, new() {
    [ThreadStatic]
    private static ObjectPool<T> m_shared = null!;
    private readonly List<PoolItem<T>> m_objects = null!;

    /// <summary>
    /// Thread-safe <see cref="ObjectPool{T}"/> instance.
    /// </summary>
    public static ObjectPool<T> Shared { get => m_shared ??= new ObjectPool<T>(); }

    public ObjectPool(int initialPoolSize = 4)
        => this.m_objects = new List<PoolItem<T>>(capacity: 4);

    public T Rent() {
        for(int i = 0; i < m_objects.Count; ++i) {
            if(!m_objects[i].IsUsed) {
                m_objects[i] = m_objects[i] with { IsUsed = true };
                return m_objects[i].Data;
            }
        }

        m_objects.Add(new PoolItem<T>(new T(), true));
        return m_objects[^1].Data;
    }

    public void Return(T @object) {        
        for(int i = 0; i < m_objects.Count; ++i) {
            if(m_objects[i].Data.Equals(@object)) {

                m_objects[i] = m_objects[i] with { IsUsed = false };
                @object.Clear();
                return;
            }
        }
    }
}