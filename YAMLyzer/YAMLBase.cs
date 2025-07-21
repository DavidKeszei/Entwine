using System;
using System.Collections;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
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

    public T? Read<T>(ReadOnlySpan<string> route) where T: IEntity {
        IEntity entity = Resolve(key: route[0]);

        if(route.Length > 1 && entity is IReadableEntity readable) entity = readable.Read<T>(route[1..])!;
        else if(route.Length > 1 && entity is not IReadableEntity) throw new AccessViolationException(message: "The route is too long. (Are you sure you don't go too far?)");

        if (entity is not T) entity = default!;
        return Unsafe.As<IEntity, T>(ref entity);
    }

    public T Read<T>(ReadOnlySpan<string> route, T onError = default!, IFormatProvider provider = null!) where T: IParsable<T> {
        YAMLValue entity = this.Read<YAMLValue>(route)!;

        if(entity != null && entity.Serialize<T>(out T? result, provider))
            return result!;

        return onError;
    }

    public List<T> ReadRange<T>(ReadOnlySpan<string> route) where T: IEntity {
        IEntity entity = route.Length == 0 ? this : Read<IEntity>(route);
        List<T> list = new List<T>();

        if (entity is not IEnumerable<IEntity>)
            throw new ArgumentException(message: $"The reached object is not implement IEnumerable<{nameof(IEntity)}>. Maybe try the Read<T> where {nameof(T)}: {nameof(IEntity)} function for it.");

        foreach(IEntity field in (IEnumerable<IEntity>)entity)
            if (field is T cast) list.Add(item: cast);

        return list;
    }

    public List<T> ReadRange<T>(ReadOnlySpan<string> route, IFormatProvider provider = null!) where T: IParsable<T> {
        List<T> list = new List<T>();

        foreach (YAMLValue field in this.ReadRange<YAMLValue>(route))
            if (field != null && field.Serialize<T>(out T? serialized, provider))
                list.Add(item: serialized!);

        return list;
    }

    public void Write<T>(ReadOnlySpan<string> route, string key, T value, string format = null!, IFormatProvider provider = null!) {
        IEntity target = route.IsEmpty ? this : this.Read<YAMLBase>(route);

        if (key == null || key == string.Empty)
            key = YAMLBase.KEYLESS;

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

    public void WriteRange<T>(ReadOnlySpan<string> route, List<(string Key, T Value)> list, string format, IFormatProvider provider) {
        YAMLBase target = route.IsEmpty ? this : Read<YAMLBase>(route);

        foreach((string key, T value) in CollectionsMarshal.AsSpan<(string, T)>(list))
            target.Write<T>(route: [], key, value, format, provider);
    }

    /// <summary>
    /// Resolve an <see cref="IEntity"/> instance independently from the collection type of the child class.
    /// </summary>
    /// <returns>Return an <see cref="IEntity"/> instance.</returns>
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
