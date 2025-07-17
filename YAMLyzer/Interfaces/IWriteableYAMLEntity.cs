using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YAMLyzer;

/// <summary>
/// Provides modify mechanism for an <see cref="IYAMLEntity"/>.
/// </summary>
public interface IWriteableYAMLEntity: IYAMLEntity, IEnumerable<IYAMLEntity> {

    /// <summary>
    /// Write primitive value to the current <see cref="YAMLObject"/> instance.
    /// </summary>
    /// <typeparam name="T">Class instance type, which implements the <see cref="IFormattable"/> interface.</typeparam>
    /// <param name="key">Key of the YAML value.</param>
    /// <param name="value">Value of the property.</param>
    /// <param name="format">Format of the property.</param>
    /// <param name="provider">Culture info the environment.</param>
    /// <returns>Return <see langword="false"/>, if the value is can be converted to YAML value. Otherwise return <see langword="false"/>.</returns>
    public bool Write<T>(string key, T value, string format = null!, IFormatProvider provider = null!) where T: IFormattable;

    /// <summary>
    /// Write string to the current <see cref="IWriteableYAMLEntity"/> instance.
    /// </summary>
    /// <param name="key">Key of the value.</param>
    /// <param name="value">The string value.</param>
    /// <returns>Return <see langword="true"/>, if the value is can be converted to YAML object. Otherwise return <see langword="false"/>.</returns>
    public bool Write(string key, string value);

    /// <summary>
    /// Write <typeparamref name="T"/> to a <see cref="IWriteableYAMLEntity"/>.
    /// </summary>
    /// <typeparam name="T">Class instance type, which implements the <see cref="IYAMLSerializable"/> interface.</typeparam>
    /// <param name="key">Key of the object inside the <see cref="YAMLObject"/>.</param>
    /// <param name="value">The object value.</param>
    /// <returns>Return <see langword="true"/>, if the value is can be converted to YAML object. Otherwise return <see langword="false"/>.</returns>
    public bool Write<T>(string key, T value) where T: IYAMLSerializable;

    /// <summary>
    /// Add a collection of <typeparamref name="T"/> instances.
    /// </summary>
    /// <typeparam name="T">Type of the collection.</typeparam>
    /// <param name="key">Key of the collection.</param>
    /// <param name="list">The collection itself.</param>
    /// <returns>Return <see langword="true"/> if the collection is successfully added to the <see cref="IWriteableYAMLEntity"/> instance. Otherwise return <see langword="false"/>.</returns>
    public bool WriteRange<T>(string key, ReadOnlySpan<T> list, string format = null!, IFormatProvider provider = null!) where T: IFormattable;

    /// <summary>
    /// Add a collection of <typeparamref name="T"/> instances.
    /// </summary>
    /// <typeparam name="T">Implementer type of the <see cref="IYAMLSerializable"/> interface.</typeparam>
    /// <param name="key">Key of the collection.</param>
    /// <param name="list">Collection of the <typeparamref name="T"/> instances.</param>
    /// <returns>Return <see langword="true"/> if the collection is successfully added to the <see cref="IWriteableYAMLEntity"/> instance. Otherwise return <see langword="false"/>.</returns>
    public bool WriteRange<T>(string key, params ReadOnlySpan<T> list) where T: IYAMLEntity;
}
