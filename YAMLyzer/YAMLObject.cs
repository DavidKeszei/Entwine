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
/// Represent a YAML object from any source.
/// </summary>
public class YAMLObject: IWriteableEntity, IReadableEntity, IClearable, IEmptiable {
    internal const string ROOT_OBJECT_INDENTIFIER = "<root>";

    private Dictionary<string, IEntity> _entities = null!;
    private string _key = "<root>";

    private bool _isCopied = false;
    private readonly YAMLType _type = YAMLType.Object;

    /// <summary>
    /// Key of the YAML object. If this name equal with <b>root</b>, then this is the root object.
    /// </summary>
    public string Key { get => _key; internal set => _key = value; }

    public YAMLType Type { get => _type; }

    /// <summary>
    /// Indicates the object has any properties inside herself.
    /// </summary>
    public bool IsEmpty { get => _entities.Count == 0; }

    /// <summary>
    /// Get a property based on the <paramref name="keys"/>.
    /// </summary>
    /// <param name="keys">Route/Keys of the property in the object.</param>
    /// <returns>Return a instance, which implement the <see cref="IEntity"/> interface.</returns>
    /// <exception cref="KeyNotFoundException"/>
    /// <exception cref="FieldAccessException"/>
    public IEntity this[string[] keys] { 
        get {
            IEntity? entity = _entities[keys[0]];
            YAMLObject obj = null!;

            for (int i = 1; i < keys.Length; ++i) {
                if ((obj = (entity as YAMLObject) ?? null!) != null && obj!._entities.TryGetValue(key: keys[i], out entity))
                    continue;

                if (obj != null) throw new KeyNotFoundException(message: $"The key (\'{keys[i]}\') isn't exists in the YAML object. Are you sure you not mistyped the key(s)?");
                else throw new FieldAccessException(message: $"To many keys declared for reach the property.");
            }

            return entity;
        }
    }

    public YAMLObject(string key) {
        this._key = key;
        this._entities = new Dictionary<string, IEntity>(capacity: 16);
    }

    public YAMLObject() {
        this._key = IEntity.KEYLESS;
        this._entities = new Dictionary<string, IEntity>();
    }

    /// <summary>
    /// Clear/Reset the current <see cref="YAMLObject"/> instance. 
    /// </summary>
    public void Clear() {
        if (!_isCopied) {
            foreach (IEntity entity in _entities.Values) {
                if (entity is IClearable)
                    ((IClearable)entity).Clear();
            }
        }

        _key = IEntity.KEYLESS;
        _entities.Clear();
    }

    public IEnumerator<IEntity> GetEnumerator() {
        foreach (string key in _entities.Keys)
            yield return _entities[key];
    }

    public T Read<T>(ReadOnlySpan<string> route) where T: IEntity {
        IEntity entity = _entities[route[0]];

        if(entity is IReadableEntity && route.Length > 1)
            entity = ((IReadableEntity)entity).Read<IEntity>(route: route[1..])!;

        if (entity == null || !(entity is T))
            return default!;

        return Unsafe.As<IEntity, T>(ref entity!);
    }

    public T? Read<T>(ReadOnlySpan<string> route, T valueOnError = default!, IFormatProvider provider = null!) where T: IParsable<T> {
        YAMLValue? entity = this.Read<YAMLValue>(route);

        if (entity == null! || !entity.Serialize<T>(out T? @result, provider)) 
            return valueOnError;

        return @result;
    }

    /// <summary>
    /// Write a(n) <typeparamref name="T"/> instance to the current <see cref="YAMLObject"/> instance.
    /// </summary>
    /// <typeparam name="T">Type of the value. This argument must be one of these types: <see cref="IFormattable"/>, <see cref="IEntity"/>, <see cref="ISerializable"/>, <see cref="string"/> or any <b>primitive</b> type.</typeparam>
    /// <param name="route">Target of the write process in the object. If this is empty, then the write process target is the current instance.</param>
    /// <param name="key">Key of the <paramref name="value"/>.</param>
    /// <param name="value">The value itself.</param>
    /// <param name="format">Format of the <paramref name="value"/>. This is ignored, when the <typeparamref name="T"/> not implement the <see cref="IFormattable"/> interface.</param>
    /// <param name="provider">Current environment of the runtime. This is ignored, when the <typeparamref name="T"/> not implement the <see cref="IFormattable"/> interface.</param>
    /// <exception cref="ArgumentException"/>
    /// <exception cref="KeyNotFoundException"/>
    /// <exception cref="IndexOutOfRangeException"/>
    public void Write<T>(ReadOnlySpan<string> route, string key, T value, string format = null!, IFormatProvider provider = null!) {
        IWriteableEntity writeable = route.Length == 0 ? this : this.Read<IWriteableEntity>(route);

        IEntity entity = value switch {
            IEntity => Unsafe.As<T, IEntity>(ref value),

            ISerializable => CreateEntity(key, serializable: Unsafe.As<T, ISerializable>(ref value)),
            IFormattable => new YAMLValue(key, ((IFormattable)value).ToString(format, provider)),

            string => new YAMLValue(key, value: Unsafe.As<T, string>(ref value)),
            bool => new YAMLValue(key, value: $"{Unsafe.As<T, bool>(ref value)}"),

            _ => throw new ArgumentException(message: "The T generic parameter is not valid type.")
        };

        ((YAMLObject)writeable)._entities.Add(key, entity);
    }

    /// <summary>
    /// Create deep copy from the current instance.
    /// </summary>
    /// <returns>Return a <see cref="YAMLObject"/> instance.</returns>
    internal YAMLObject AsCopy() {
        YAMLObject copy = new YAMLObject(key: _key[0..]);
        copy._entities = new Dictionary<string, IEntity>(collection: _entities);

        _isCopied = true;
        return copy;
    }

    IEnumerator IEnumerable.GetEnumerator()
        => GetEnumerator();

    private IEntity CreateEntity(string key, ISerializable serializable) {
        IWriteableEntity entity = new YAMLObject(key);
        serializable.ToYAML(in entity);

        return entity;
    }
}
