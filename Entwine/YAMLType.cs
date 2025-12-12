using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entwine;

/// <summary>
/// Collection of valid YAML types.
/// </summary>
public enum YAMLType: byte {
    /// <summary>
    /// Unknow type. This not presented the final YAML serialization.
    /// </summary>
    NONE,
    /// <summary>
    /// Represents an object inside the YAML source.
    /// </summary>
    OBJECT,
    /// <summary>
    /// Represent group of items in the YAML source.
    /// </summary>
    COLLECTION,
    /// <summary>
    /// Smallest unit in a YAML source.
    /// </summary>
    FIELD
}
