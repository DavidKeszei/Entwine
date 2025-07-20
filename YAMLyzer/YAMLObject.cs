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
public class YAMLObject: YAMLBase, IClearable, IEmptiable, IEnumerable<IEntity> {
    private Dictionary<string, IEntity> _entities = null!;
    private bool _isCopied = false;

    /// <summary>
    /// Indicates the object has any properties inside herself.
    /// </summary>
    public bool IsEmpty { get => _entities == null || _entities.Count == 0; }

    /// <summary>
    /// Get a property based on the <paramref name="keys"/>.
    /// </summary>
    /// <param name="keys">Route/Keys of the property in the object.</param>
    /// <returns>Return a instance, which implement the <see cref="YAMLBase"/> interface.</returns>
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

    public YAMLObject(string key): base(key, type: YAMLType.Object)
        => this._entities = new Dictionary<string, IEntity>(capacity: 16);

    public YAMLObject(): base(key: YAMLBase.KEYLESS, type: YAMLType.Object)
        => this._entities = new Dictionary<string, IEntity>();

    public IEnumerator<IEntity> GetEnumerator() {
        foreach (string key in _entities.Keys)
            yield return _entities[key];
    }

    /// <summary>
    /// Clear/Reset the current <see cref="YAMLObject"/> instance. 
    /// </summary>
    public void Clear() {
        if(!_isCopied) {
            foreach(YAMLBase entity in _entities.Values) {
                if(entity is IClearable)
                    ((IClearable)entity).Clear();
            }
        }

        Key = YAMLBase.KEYLESS;
        _entities.Clear();
    }

    /// <summary>
    /// Create deep copy from the current instance.
    /// </summary>
    /// <returns>Return a <see cref="YAMLObject"/> instance.</returns>
    public YAMLObject AsCopy() {
        YAMLObject copy = new YAMLObject(key: Key[0..]);
        copy._entities = new Dictionary<string, IEntity>(collection: _entities);

        _isCopied = true;
        return copy;
    }

    protected override IEntity Resolve(string key) => this._entities[key];

    protected override void Create(IEntity entity) {
        if(_entities.TryAdd(key: entity.Key, value: entity))
            return;

        _entities[entity.Key] = entity;
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
