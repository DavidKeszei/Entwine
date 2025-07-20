using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YAMLyzer;

/// <summary>
/// Provides reading mechanism for an <see cref="YAMLBase"/>.
/// </summary>
public interface IReadableEntity {

    /// <summary>
    /// Read a(n) <typeparamref name="T"/> entry from the current <see cref="IReadableEntity"/> instance.
    /// </summary>
    /// <typeparam name="T">Implementer class of the <see cref="YAMLBase"/> interface.</typeparam>
    /// <param name="route">Route to the target of the read process.</param>
    /// <returns>Return a(n) <typeparamref name="T"/> instance.</returns>
    /// <exception cref="ArgumentException"/>
    /// <exception cref="IndexOutOfRangeException"/>
    /// <exception cref="ArgumentOutOfRangeException"/>
    public T Read<T>(ReadOnlySpan<string> route) where T: IEntity;

    /// <summary>
    /// Read primitive value from the <see cref="IReadableEntity"/> based on the route.
    /// </summary>
    /// <typeparam name="T">Return type of the primitive value.</typeparam>
    /// <param name="route">Access route/Keys of the value inside the <see cref="YAMLBase"/>.</param>
    /// <param name="provider">Current culture of the environment.</param>
    /// <returns>Return a(n) <typeparamref name="T"/> instance. If type is incorrect, then return <see langword="null"/>.</returns>
    /// <exception cref="ArgumentException"/>
    /// <exception cref="IndexOutOfRangeException"/>
    /// <exception cref="ArgumentOutOfRangeException"/>
    public T? Read<T>(ReadOnlySpan<string> route, T valueOnError = default!, IFormatProvider provider = null!) where T: IParsable<T>;
}
