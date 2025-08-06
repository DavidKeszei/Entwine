using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entwine.Objects;

/// <summary>
/// Provides copy mechanism for an <typeparamref name="TSelf"/> class.
/// </summary>
/// <typeparam name="TSelf">The implementer class type.</typeparam>
public interface ICopyable<TSelf> {

    /// <summary>
    /// Create a new copy instance from the current <typeparamref name="TSelf"/> instance.
    /// </summary>
    /// <returns>Return a new <typeparamref name="TSelf"/> instance.</returns>
    public TSelf AsCopy();
}
