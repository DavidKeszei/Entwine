using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YAMLyzer;

public interface IWriteableYAMLEntity {

    /// <summary>
    /// Add primitive value to the YAML object.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="key">Key of the YAML value.</param>
    /// <param name="value">Value of the property.</param>
    /// <param name="format">Format of the property.</param>
    /// <param name="provider">Culture info the environment.</param>
    /// <returns>Return <see langword="false"/>, if the value is can be converted to YAML value. Otherwise return <see langword="false"/>.</returns>
    public bool Add<T>(string key, T value, string format = null!, IFormatProvider provider = null!) where T : IFormattable;

    /// <summary>
    /// Add string to the YAML source.
    /// </summary>
    /// <param name="key">Key of the string.</param>
    /// <param name="value">The string value.</param>
    /// <returns>Return <see langword="true"/>, if the value is can be converted to YAML object. Otherwise return <see langword="false"/>.</returns>
    public bool Add(string key, string value);

    /// <summary>
    /// Add <typeparamref name="T"/> to the YAML object.
    /// </summary>
    /// <typeparam name="T">Type of the object.</typeparam>
    /// <param name="key">Key of the object inside the YAML object.</param>
    /// <param name="value">The object value.</param>
    /// <returns>Return <see langword="true"/>, if the value is can be converted to YAML object. Otherwise return <see langword="false"/>.</returns>
    public bool Add<T>(string key, T value) where T: IYAMLSerializable;
}
