using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace YAMLyzer;

/// <summary>
/// Smallest unit inside a YAML file.
/// </summary>
public class YAMLValue: IEntity {
    private readonly string m_key = string.Empty;
    private string m_value = string.Empty;

    private readonly YAMLType m_type = YAMLType.Primitive;

    public string Key { get => m_key; }

    public YAMLType TypeOf { get => m_type; }

    internal YAMLValue(string key, string value) {
        this.m_key = key;
        this.m_value = value;
    }

    /// <summary>
    /// Serialize the YAML value to a primitive type.
    /// </summary>
    /// <typeparam name="T">Type of the primitive.</typeparam>
    /// <param name="result">Result of the serialization.</param>
    /// <returns>If the value is serializable, then return <see langword="true"/>. Otherwise return <see langword="false"/>.</returns>
    public bool Read<T>(out T? result, IFormatProvider provider = null!) where T: IParsable<T> {
        result = default!;

        if(m_value[0] == '~' || m_value == "null") return false;
        return T.TryParse(m_value, provider, out result);
    }

    /// <summary>
    /// Serialize the YAML value to a primitive type.
    /// </summary>
    /// <typeparam name="T">Type of the primitive.</typeparam>
    /// <exception cref="FormatException"/>
    public T Read<T>(IFormatProvider provider = null!) where T: IParsable<T> => T.Parse(m_value, provider);

    /// <summary>
    /// Write <typeparamref name="T"/> value to current <see cref="YAMLValue"/> instance.
    /// </summary>
    /// <typeparam name="T">Type of the primitive value.</typeparam>
    /// <param name="value">The value itself.</param>
    /// <param name="format">Format of the value.</param>
    /// <param name="provider">Current environment of the runtime.</param>
    /// <exception cref="ArgumentException"/>
    public void Write<T>(T value, string format = null!, IFormatProvider provider = null!) {
        _ = value switch {
            IFormattable => m_value = ((IFormattable)value).ToString(format, provider),
            string => m_value = Unsafe.As<T, string>(ref value),

            bool => m_value = $"{value}",
            _ => throw new ArgumentException(message: "The T type argument must be equal with some primitive type. (int, string, etc.)")
        };
    }

    public override string ToString() => m_key == YAMLBase.KEYLESS ? $"{IsNULL()}" : $"{m_key}: {IsNULL()}";

    private string IsNULL()
        => (m_value == string.Empty ? "null" : m_value);
}

/// <summary>
/// Provides identifier capabilities for a YAML entity.
/// </summary>
public interface IEntity {

    /// <summary>
    /// Key of the <see cref="IEntity"/>.
    /// </summary>
    public string Key { get; }

    /// <summary>
    /// Type of the <see cref="IEntity"/>.
    /// </summary>
    public YAMLType TypeOf { get; }
}
