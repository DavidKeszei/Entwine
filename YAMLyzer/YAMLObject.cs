using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YAMLyzer.Interfaces;

namespace YAMLyzer;

/// <summary>
/// Represent a YAML object from any source.
/// </summary>
public class YAMLObject: IYAMLEntity, IWriteableYAMLEntity, IClearable {
    private Dictionary<string, IYAMLEntity> _entities = null!;
    private string _key = "<root>";

    private YAMLType _type = YAMLType.Object;
    private bool _isCopied = false;

    /// <summary>
    /// Key of the YAML object. If this name equal with <b>root</b>, then this is the root object.
    /// </summary>
    public string Key { get => _key; internal set => _key = value; }

    public YAMLType Type { get => _type; }

    /// <summary>
    /// Indicates the object has any properties inside themself.
    /// </summary>
    public bool IsEmpty { get => _entities.Count == 0; }

    /// <summary>
    /// Get a property based on the <paramref name="keys"/>.
    /// </summary>
    /// <param name="keys">Route/Keys of the property in the object.</param>
    /// <returns>Return a instance, which implement the <see cref="IYAMLEntity"/> interface.</returns>
    /// <exception cref="KeyNotFoundException"/>
    /// <exception cref="FieldAccessException"/>
    public IYAMLEntity this[string[] keys] { 
        get {
            IYAMLEntity? entity = _entities[keys[0]];
            if(entity is not YAMLObject && keys.Length == 1) return entity;

            YAMLObject? obj = (YAMLObject)entity;

            for (int i = 1; i < keys.Length; ++i) {
                if ((obj = (entity as YAMLObject)) != null && obj!._entities.TryGetValue(key: keys[i], out entity))
                    continue;

                if (obj != null) throw new KeyNotFoundException(message: $"The key (\'{keys[i]}\') isn't exists in the YAML object. Are you sure you not mistyped the key(s)?");
                else throw new FieldAccessException(message: $"To many keys declared for to reach the property. (Current key index: {i})");
            }

            return entity;
        }
    }

    public YAMLObject(string key) {
        this._key = key;
        this._entities = new Dictionary<string, IYAMLEntity>(capacity: 128);
    }

    public YAMLObject() {
        this._key = "<object>";
        this._entities = new Dictionary<string, IYAMLEntity>();
    }

    public bool Add(string key, string value) => _entities.TryAdd(key, new YAMLValue(key, value));

    public bool Add<T>(string key, T value, string format = null!, IFormatProvider provider = null!) where T: IFormattable {
        provider ??= CultureInfo.CurrentCulture;

        IYAMLEntity entity = new YAMLValue(key, value.ToString(format, provider));
        return _entities.TryAdd(key, entity);
    }

    public bool Add<T>(string key, T value) where T: IYAMLSerializable {
        IWriteableYAMLEntity entity = (IWriteableYAMLEntity)new YAMLObject(key);
        value.ToYAML(in entity);

        return _entities.TryAdd(key, (IYAMLEntity)entity);
    }

    internal bool Add(string key, IYAMLEntity entity) => _entities.TryAdd(key, entity);

    /// <summary>
    /// Reset the internal dictionary of the <see cref="YAMLObject"/> instance. 
    /// </summary>
    public void Clear() {
        if (!_isCopied) {
            foreach (IYAMLEntity entity in _entities.Values) {
                if (entity is IClearable)
                    ((IClearable)entity).Clear();
            }
        }

        _key = "<object>";
        _entities.Clear();
    }

    /// <summary>
    /// Create a deep copy from the current instance.
    /// </summary>
    /// <returns>Return a <see cref="YAMLObject"/> instance.</returns>
    internal YAMLObject AsCopy() {
        YAMLObject copy = new YAMLObject(key: _key[0..]);
        copy._entities = new Dictionary<string, IYAMLEntity>(collection: _entities);

        _isCopied = true;
        return copy;
    }
}
