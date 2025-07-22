using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using YAMLyzer.Interfaces;

namespace YAMLyzer;

/// <summary>
/// Represent a collection inside a YAML document.
/// </summary>
public class YAMLCollection: YAMLBase, IClearable, IEmptiable, IEnumerable<IEntity> {
    private readonly List<IEntity> m_collection = null!;
    private bool m_isCopied = false;

    /// <summary>
    /// Get an <see cref="YAMLBase"/> based on the <paramref name="index"/>.
    /// </summary>
    /// <param name="index">Position of the <see cref="YAMLBase"/>.</param>
    /// <returns>Return an <see cref="YAMLBase"/> instance.</returns>
    /// <exception cref="IndexOutOfRangeException"/>
    public IEntity this[int index] { get => this.m_collection[index]; }

    /// <summary>
    /// Length of the collection.
    /// </summary>
    public int Length { get => m_collection.Count; }

    /// <summary>
    /// Indicates the current collection is empty.
    /// </summary>
    public bool IsEmpty { get => m_collection == null || m_collection.Count == 0; }

    /// <summary>
    /// The underlying collection of the current object instance.
    /// </summary>
    internal List<IEntity> InternalCollection { get => m_collection; } 

    public YAMLCollection(string key, params IEnumerable<IEntity> collection): base(key, YAMLType.Collection) 
        => this.m_collection = new List<IEntity>(collection);

    public YAMLCollection(): base(key: YAMLBase.KEYLESS, type: YAMLType.Collection)
        => this.m_collection = new List<IEntity>();

    public void Clear() {
        if (!m_isCopied) {
            foreach (IEntity entity in m_collection) {
                if (entity is IClearable)
                    ((IClearable)entity).Clear();
            }
        }

        m_collection.Clear();
        Key = YAMLBase.KEYLESS;
    }

    public IEnumerator<IEntity> GetEnumerator() => this.m_collection.GetEnumerator();

    /// <summary>
    /// Create deep copy from the current instance.
    /// </summary>
    /// <returns>Return a <see cref="YAMLCollection"/> instance.</returns>
    public YAMLCollection AsCopy() {
        m_isCopied = true;
        return new YAMLCollection(key: this.Key, collection: m_collection);
    }

    protected override IEntity Resolve(string key) {
        if(!int.TryParse(s: key, out int index))
            throw new ArgumentException(message: $"The collection index is can't be parsed to number. (Value: {key})");

        return m_collection[index];
    }

    protected override void Create(IEntity entity) {
        int index = -1;
        if((index = m_collection.IndexOf(entity)) != -1) {
            m_collection[index] = entity;
            return;
        }

        m_collection.Add(item: entity);
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
