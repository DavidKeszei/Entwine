using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YAMLyzer;

/// <summary>
/// Collection of valid YAML types.
/// </summary>
public enum YAMLType: byte {
    /// <summary>
    /// Unknow type. This not presented the final YAML serialization.
    /// </summary>
    None,
    /// <summary>
    /// Represents an object inside the YAML source.
    /// </summary>
    Object,
    Array,
    /// <summary>
    /// Smallest unit in a YAML source.
    /// </summary>
    Primitive
}
