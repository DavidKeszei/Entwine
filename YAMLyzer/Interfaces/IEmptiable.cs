using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YAMLyzer;

/// <summary>
/// Provides mechanims for check empty state for an object.
/// </summary>
public interface IEmptiable {

    /// <summary>
    /// Indicates the object is empty.
    /// </summary>
    public bool IsEmpty { get; }
}
