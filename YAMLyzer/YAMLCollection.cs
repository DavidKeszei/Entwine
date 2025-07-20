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
    private readonly List<IEntity> _collection = null!;
    private bool _isCopied = false;

    /// <summary>
    /// Get an <see cref="YAMLBase"/> based on the <paramref name="index"/>.
    /// </summary>
    /// <param name="index">Position of the <see cref="YAMLBase"/>.</param>
    /// <returns>Return an <see cref="YAMLBase"/> instance.</returns>
    /// <exception cref="IndexOutOfRangeException"/>
    public IEntity this[int index] { get => this._collection[index]; }

    /// <summary>
    /// Length of the collection.
    /// </summary>
    public int Length { get => _collection.Count; }

    /// <summary>
    /// Indicates the current collection is empty.
    /// </summary>
    public bool IsEmpty { get => _collection == null || _collection.Count == 0; }

    /// <summary>
    /// The underlying collection of the current object instance.
    /// </summary>
    internal List<IEntity> InternalCollection { get => _collection; } 

    public YAMLCollection(string key, params IEnumerable<IEntity> collection): base(key, YAMLType.Collection) 
        => this._collection = new List<IEntity>(collection);

    public YAMLCollection(): base(key: YAMLBase.KEYLESS, type: YAMLType.Collection)
        => this._collection = new List<IEntity>();

    public void Clear() {
        if (!_isCopied) {
            foreach (IEntity entity in _collection) {
                if (entity is IClearable)
                    ((IClearable)entity).Clear();
            }
        }

        _collection.Clear();
        Key = YAMLBase.KEYLESS;
    }

    public IEnumerator<IEntity> GetEnumerator() => this._collection.GetEnumerator();

    /// <summary>
    /// Create deep copy from the current instance.
    /// </summary>
    /// <returns>Return a <see cref="YAMLCollection"/> instance.</returns>
    public YAMLCollection AsCopy() {
        _isCopied = true;
        return new YAMLCollection(key: this.Key, collection: _collection);
    }

    protected override IEntity Resolve(string key) {
        if(!int.TryParse(s: key, out int index))
            throw new ArgumentException(message: $"The collection index is can't be parsed to number. (Value: {key})");

        return _collection[index];
    }

    protected override void Create(IEntity entity) {
        int index = -1;
        if((index = _collection.IndexOf(entity)) != -1) {
            _collection[index] = entity;
            return;
        }

        _collection.Add(item: entity);
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
