using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YAMLyzer.Buffers;

/// <summary>
/// Smallest unit of the <see cref="ObjectPool{T}"/>.
/// </summary>
/// <typeparam name="T">Type of the <see cref="Data"/> instance. Must be implement the <see cref="IClearable"/> interface and must have a parameterless constructor.</typeparam>
internal record struct PoolItem<T> where T : class, IClearable, new() {
    private readonly T m_data = default!;
    private bool m_isUsed = false;

    /// <summary>
    /// The data itself.
    /// </summary>
    public readonly T Data { get => m_data; }

    /// <summary>
    /// Indicates the data instance is rented/used somewhere else in the code. 
    /// </summary>
    public readonly bool IsUsed { get => m_isUsed; internal init => m_isUsed = value; }

    public PoolItem(T data, bool isUsed) {
        this.m_data = data;
        this.m_isUsed = isUsed;
    }
}
