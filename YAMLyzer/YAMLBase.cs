using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace YAMLyzer;

/// <summary>
/// Superclass of all YAML specific classes.
/// </summary>
public abstract class YAMLBase: IEntity, IWriteableEntity, IReadableEntity {
    /// <summary>
    /// Root identifier for root object in the YAML graph.
    /// </summary>
    public const string ROOT = "<root>";

    /// <summary>
    /// Keyless identifier for keyless <see cref="YAMLBase"/> instances.
    /// </summary>
    public const string KEYLESS = "<no key>";

    private string _key = string.Empty;
    private YAMLType _type = YAMLType.None;

    /// <summary>
    /// Type of the current <see cref="YAMLBase"/> instance. This property must be overwritten by the child classes.
    /// </summary>
    public virtual YAMLType TypeOf { get => _type; }

    /// <summary>
    /// Key of the current <see cref="YAMLBase"/> instance.
    /// </summary>
    public string Key { get => _key; internal set => _key = value; }

    public YAMLBase(string key, YAMLType type) {
        this._key = key;
        this._type = type;
    }

    public T Read<T>(ReadOnlySpan<string> route) where T: IEntity {
        IEntity entity = Resolve(key: route[0]);

        if(route.Length > 1 && entity is IReadableEntity readable) entity = readable.Read<T>(route[1..]);
        else if(route.Length > 1 && entity is not IReadableEntity) throw new AccessViolationException(message: "The route is too long. (Are you sure you don't go too far?)");

        return Unsafe.As<IEntity, T>(ref entity);
    }

    public T? Read<T>(ReadOnlySpan<string> route, T onError, IFormatProvider provider = null!) where T: IParsable<T> {
        YAMLValue entity = this.Read<YAMLValue>(route);

        if(entity != null && entity.Serialize<T>(out T? result, provider))
            return result;

        return onError;
    }

    /// <summary>
    /// Write a(n) <typeparamref name="T"/> instance to the current instance.
    /// </summary>
    /// <typeparam name="T">Type of the value. This argument must be one of these types: <see cref="IFormattable"/>, <see cref="YAMLBase"/>, <see cref="ISerializable"/>, <see cref="string"/> or any <b>primitive</b> type.</typeparam>
    /// <param name="route">Target of the write process in the object. If this is empty, then the write process target is the current instance.</param>
    /// <param name="key">Key of the <paramref name="value"/>. This <paramref name="key"/> must be not equal with <see langword="null"/> or <see cref="YAMLBase.KEYLESS"/>.</param>
    /// <param name="value">The value itself.</param>
    /// <param name="format">Format of the <paramref name="value"/>. This is ignored, when the <typeparamref name="T"/> not implement the <see cref="IFormattable"/> interface.</param>
    /// <param name="provider">Current environment of the runtime. This is ignored, when the <typeparamref name="T"/> not implement the <see cref="IFormattable"/> interface.</param>
    /// <exception cref="ArgumentException"/>
    /// <exception cref="KeyNotFoundException"/>
    /// <exception cref="IndexOutOfRangeException"/>
    public void Write<T>(ReadOnlySpan<string> route, string key, T value, string format = null!, IFormatProvider provider = null!) {
        IEntity target = route.Length == 0 ? this : this.Read<YAMLBase>(route);

        IEntity? field = value switch {
            IEntity => Unsafe.As<T, IEntity>(ref value),

            ISerializable => CreateEntity(key, serializable: Unsafe.As<T, ISerializable>(ref value)),
            IFormattable => new YAMLValue(key, ((IFormattable)value).ToString(format, provider)),

            string => new YAMLValue(key, value: Unsafe.As<T, string>(ref value) == string.Empty ? "~" : Unsafe.As<T, string>(ref value)),
            bool => new YAMLValue(key, value: $"{Unsafe.As<T, bool>(ref value)}"),

            null => new YAMLObject(key),
            _ => null!
        };

        if(target is YAMLBase @base) @base.Create(entity: field);
    }

    /// <summary>
    /// Resolve a <see cref="YAMLBase"/> instance independently from the collection type.
    /// </summary>
    /// <returns>Return a <see cref="YAMLBase"/> instance.</returns>
    protected abstract IEntity Resolve(string key);

    /// <summary>
    /// Add <see cref="IEntity"/> instance to the collection.
    /// </summary>
    /// <param name="entity">The value itself.</param>
    protected abstract void Create(IEntity entity);

    /// <summary>
    /// Create new <see cref="IEntity"/> from a object.
    /// </summary>
    /// <param name="key">Key of the object.</param>
    /// <param name="serializable">The value itself.</param>
    /// <returns>Return a <see cref="IEntity"/> instance.</returns>
    private IEntity CreateEntity(string key, ISerializable serializable) {
        IWriteableEntity entity = new YAMLObject(key);
        serializable.ToYAML(in entity);

        return (IEntity)entity;
    }
}
