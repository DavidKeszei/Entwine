using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entwine;

/// <summary>
/// Provides reading mechanism for an <see cref="YAMLBase"/> instance.
/// </summary>
public interface IReadableEntity {

    /// <summary>
    /// Read a(n) <typeparamref name="T"/> entry from the current <see cref="IReadableEntity"/> instance.
    /// </summary>
    /// <typeparam name="T">Implementer class of the <see cref="YAMLBase"/> interface.</typeparam>
    /// <param name="route">Route to the target of the read process.</param>
    /// <returns>Return a(n) <typeparamref name="T"/> instance.</returns>
    /// <exception cref="AccessViolationException"/>
    /// <exception cref="ArgumentException"/>
    public T? Read<T>(ReadOnlySpan<string> route) where T: IEntity;

    /// <summary>
    /// Read primitive value from the <see cref="IReadableEntity"/> based on the route.
    /// </summary>
    /// <typeparam name="T">Return type of the primitive value.</typeparam>
    /// <param name="route">Access route/Keys of the value inside the <see cref="YAMLBase"/>.</param>
    /// <param name="provider">Current culture of the environment.</param>
    /// <returns>Return a(n) <typeparamref name="T"/> instance. If type is incorrect, then return <see langword="null"/>.</returns>
    /// <exception cref="AccessViolationException"/>
    /// <exception cref="ArgumentException"/>
    public T Read<T>(ReadOnlySpan<string> route, T onError = default!, IFormatProvider provider = null!) where T: IParsable<T>;

    /// <summary>
    /// Read a list of <typeparamref name="T"/> instances from a collection inside the current object.
    /// </summary>
    /// <typeparam name="T">Type of one instance.</typeparam>
    /// <param name="route">Route of the collection inside the current instance. If this is empty, then collection is the object itself.</param>
    /// <returns>Return a list of <typeparamref name="T"/> instances.</returns>
    /// <exception cref="ArgumentException"/>
    /// <exception cref="IndexOutOfRangeException"/>
    /// <exception cref="ArgumentOutOfRangeException"/>
    public List<T> ReadRange<T>(ReadOnlySpan<string> route) where T: IEntity;

    /// <summary>
    /// Read a list of <typeparamref name="T"/> instances from a collection inside the current object.
    /// </summary>
    /// <typeparam name="T">Type of one instance.</typeparam>
    /// <param name="route">Route of the collection inside the current instance. If this is empty, then collection is the object itself.</param>
    /// <returns>Return a list of <typeparamref name="T"/> instances.</returns>
    /// <exception cref="ArgumentException"/>
    /// <exception cref="IndexOutOfRangeException"/>
    /// <exception cref="ArgumentOutOfRangeException"/>
    public List<T> ReadRange<T>(ReadOnlySpan<string> route, IFormatProvider provider = null!) where T: IParsable<T>;
}
