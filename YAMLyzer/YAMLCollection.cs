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
public class YAMLCollection: IReadableEntity, IClearable, IEmptiable {
    private readonly List<IEntity> _collection = null!;
    private string _key = string.Empty;

    private readonly YAMLType _type = YAMLType.Array;
    private bool _isCopied = false;

    /// <summary>
    /// Get an <see cref="IEntity"/> based on the <paramref name="index"/>.
    /// </summary>
    /// <param name="index">Position of the <see cref="IEntity"/>.</param>
    /// <returns>Return an <see cref="IEntity"/> instance.</returns>
    /// <exception cref="IndexOutOfRangeException"/>
    public IEntity this[int index] { get => this._collection[index]; }

    /// <summary>
    /// Length of the collection.
    /// </summary>
    public int Length { get => _collection.Count; }

    /// <summary>
    /// Key of the collection.
    /// </summary>
    public string Key { get => _key; internal set => _key = value; }

    public bool IsEmpty { get => Length == 0; }

    public YAMLType Type { get => _type; }

    /// <summary>
    /// The underlying collection of the current object instance.
    /// </summary>
    internal List<IEntity> InternalCollection { get => _collection; } 

    internal YAMLCollection(string key, params IEnumerable<IEntity> collection) {
        this._key = key;
        this._collection = new List<IEntity>(collection);
    }

    public YAMLCollection() {
        this._key = IEntity.KEYLESS;
        this._collection = new List<IEntity>();
    }

    public T Read<T>(ReadOnlySpan<string> route) where T: IEntity {
        if (!int.TryParse(route[0], out int index))
            throw new ArgumentException(message: $"The parameter is not a numeric string literal. (Route: {string.Join('-', route!)})");

        IEntity? entity = _collection[index];

        if (route.Length > 1 && entity is IReadableEntity)
            entity = ((IReadableEntity)entity).Read<T>(route[1..]);

        if (entity == null || !(entity is T)) return default!;
        return Unsafe.As<IEntity, T>(ref entity);
    }

    public T Read<T>(ReadOnlySpan<string> route, T valueOnError = default!, IFormatProvider provider = null!) where T: IParsable<T> {
        IEntity readable = null!;

        if(int.TryParse(s: route[0], out int index)) readable = _collection[index];
        else throw new ArgumentException(message: "The index can't converter to a number value.");

        if(route.Length > 1 && readable is IReadableEntity @object)
            readable = @object.Read<IEntity>(route[1..]);

        if(readable is YAMLValue value && value.Serialize<T>(out T? @result, provider))
            return @result!;

        return valueOnError;
    }

    public void Clear() {
        if (!_isCopied) {
            foreach (IEntity entity in _collection) {
                if (entity is IClearable)
                    ((IClearable)entity).Clear();
            }
        }

        _collection.Clear();
        _key = IEntity.KEYLESS;
    }

    public IEnumerator<IEntity> GetEnumerator()
        => this._collection.GetEnumerator();

    /// <summary>
    /// Create deep copy from the current instance.
    /// </summary>
    /// <returns>Return a <see cref="YAMLCollection"/> instance.</returns>
    internal YAMLCollection AsCopy() {
        _isCopied = true;
        return new YAMLCollection(key: this._key, collection: _collection);
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
