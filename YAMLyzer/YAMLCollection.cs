using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YAMLyzer.Interfaces;

namespace YAMLyzer;

/// <summary>
/// Represent a collection inside a YAML string.
/// </summary>
public class YAMLCollection: IYAMLEntity, IClearable {
    private readonly List<IYAMLEntity> _collection = null!;
    private string _key = string.Empty;

    private readonly YAMLType _type = YAMLType.Array;
    private bool _isCopied = false;

    /// <summary>
    /// Get 
    /// </summary>
    /// <param name="index"></param>
    /// <returns></returns>
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
        this._key = "<collection>";
        this._collection = new List<IYAMLEntity>();
    }

    public void Clear() {
        if (!_isCopied) {
            foreach (IYAMLEntity entity in _collection) {
                if (entity is IClearable)
                    ((IClearable)entity).Clear();
            }
        }

        _collection.Clear();
        _key = "<collection>";
    }

    public YAMLCollection AsCopy() {
        _isCopied = true;
        return new YAMLCollection(key: this._key, collection: _collection);
    }
}
