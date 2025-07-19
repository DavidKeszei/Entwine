using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YAMLyzer.Interfaces;

namespace YAMLyzer;

/// <summary>
/// Represent an abstract representation of a YAML object/value.
/// </summary>
public interface IEntity {

    /// <summary>
    /// The keyless constant for <see cref="YAMLObject"/>.
    /// </summary>
    public const string KEYLESS = "<no key>";

    /// <summary>
    /// Key of the object in the YAML document.
    /// </summary>
    public string Key { get; }

    /// <summary>
    /// Type of the YAML object.
    /// </summary>
    public YAMLType Type { get; }
}
