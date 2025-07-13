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
/// Represent a collection inside a YAML string.
/// </summary>
public class YAMLCollection: IClearable, IEnumerable<IYAMLEntity>, IReadableYAMLEntity {
    private readonly List<IYAMLEntity> _collection = null!;
    private string _key = string.Empty;

    private readonly YAMLType _type = YAMLType.Array;
    private bool _isCopied = false;

    /// <summary>
    /// Get an <see cref="IYAMLEntity"/> based on the <paramref name="index"/>.
    /// </summary>
    /// <param name="index">Position of the <see cref="IYAMLEntity"/>.</param>
    /// <returns>Return an <see cref="IYAMLEntity"/> instance.</returns>
    /// <exception cref="IndexOutOfRangeException"/>
    public IYAMLEntity this[int index] { get => this._collection[index]; }

    /// <summary>
    /// Length of the collection.
    /// </summary>
    public int Length { get => _collection.Count; }

    /// <summary>
    /// Key of the collection.
    /// </summary>
    public string Key { get => _key; internal set => _key = value; }

    public YAMLType Type { get => _type; }

    /// <summary>
    /// [Internal Use] The underlying collection of the current object instance.
    /// </summary>
    internal List<IYAMLEntity> InternalCollection { get => _collection; } 

    internal YAMLCollection(string key, params IEnumerable<IYAMLEntity> collection) {
        this._key = key;
        this._collection = new List<IYAMLEntity>(collection);
    }

    public YAMLCollection() {
        this._key = IYAMLEntity.KEYLESS;
        this._collection = new List<IYAMLEntity>();
    }

    public T Read<T>(ReadOnlySpan<string> route) where T: IYAMLEntity {
        if (!int.TryParse(route[0], out int index))
            throw new ArgumentException(message: $"The parameter {nameof(route)} is not a numeric string literal.");

        IYAMLEntity? entity = _collection[index];

        if (route.Length > 1 && entity is IReadableYAMLEntity @object)
            entity = @object.Read<T>(route[1..]);

        if (entity == null || !(entity is T)) return default!;
        return Unsafe.As<IYAMLEntity, T>(ref entity);
    }

    public T Read<T>(ReadOnlySpan<string> route, IFormatProvider provider = null!) where T: IParsable<T> {
        IYAMLEntity readable = null!;

        if(int.TryParse(s: route[0], out int index)) readable = _collection[index];
        else throw new ArgumentException(message: "The index can't converter to a number value.");

        if(route.Length > 1 && readable is IReadableYAMLEntity @object)
            readable = @object.Read<IYAMLEntity>(route[1..]);

        if(readable is YAMLValue value && value.Serialize<T>(out T? @result, provider))
            return @result!;

        return default!;
    }

    public void Clear() {
        if (!_isCopied) {
            foreach (IYAMLEntity entity in _collection) {
                if (entity is IClearable)
                    ((IClearable)entity).Clear();
            }
        }

        _collection.Clear();
        _key = IYAMLEntity.KEYLESS;
    }

    public IEnumerator<IYAMLEntity> GetEnumerator()
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
