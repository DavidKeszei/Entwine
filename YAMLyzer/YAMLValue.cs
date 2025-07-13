using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YAMLyzer;

/// <summary>
/// Represent a primitive value in the YAML value.
/// </summary>
public class YAMLValue: IYAMLEntity {
    private string _key = string.Empty;
    private string _value = string.Empty;

    private YAMLType _type = YAMLType.Primitive;

    public string Key { get => _key; }

    public YAMLType Type { get => _type; }

    internal YAMLValue(string key, string value) {
        this._key = key;
        this._value = value;
    }

    /// <summary>
    /// Serialize the YAML value to a primitive type.
    /// </summary>
    /// <typeparam name="T">Type of the primitive.</typeparam>
    /// <param name="result">Result of the serialization.</param>
    /// <returns>If the value is serializable, then return <see langword="true"/>. Otherwise return <see langword="false"/>.</returns>
    public bool Serialize<T>(out T? result, IFormatProvider provider = null!) where T: IParsable<T>
        => T.TryParse(_value, provider, out result);

    /// <summary>
    /// Serialize the YAML value to a primitive type.
    /// </summary>
    /// <typeparam name="T">Type of the primitive.</typeparam>
    /// <exception cref="FormatException"/>
    public T Serialize<T>(IFormatProvider provider = null!) where T: IParsable<T> => T.Parse(_value, provider);

    public override string ToString() => $"{_key}: {_value}";
}
