using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YAMLyzer;

/// <summary>
/// Provides modify mechanism for an <see cref="YAMLBase"/>.
/// </summary>
public interface IWriteableEntity {

    /// <summary>
    /// Write a(n) <typeparamref name="T"/> instance to the current <see cref="YAMLObject"/> instance.
    /// </summary>
    /// <typeparam name="T">Type of the value. This argument must be one of these types: <see cref="IFormattable"/>, <see cref="YAMLBase"/>, <see cref="ISerializable"/>, <see cref="string"/> or any <b>primitive</b> type.</typeparam>
    /// <param name="route">Target of the write process in the object. If this is empty, then write process target is the current instance.</param>
    /// <param name="key">Key of the <paramref name="value"/>.</param>
    /// <param name="value">The value itself.</param>
    /// <param name="format">Format of the <paramref name="value"/>. This is ignored, when the <typeparamref name="T"/> not implement the <see cref="IFormattable"/> interface.</param>
    /// <param name="provider">Current environment of the runtime. This is ignored, when the <typeparamref name="T"/> not implement the <see cref="IFormattable"/> interface.</param>
    /// <exception cref="ArgumentException"/>
    /// <exception cref="KeyNotFoundException"/>
    /// <exception cref="IndexOutOfRangeException"/>
    public void Write<T>(ReadOnlySpan<string> route, string key, T value, string format = null!, IFormatProvider provider = null!);
}
