using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YAMLyzer;

/// <summary>
/// Provides de-/serialization mechanisms for object(s).
/// </summary>
public interface IYAMLSerializable {

    /// <summary>
    /// [Serialization] Setting up the <see cref="YAMLObject"/> <paramref name="obj"/> properties from the current object.
    /// </summary>
    /// <param name="obj">Result object reference of YAML representation.</param>
    public void ToYAML(in IWriteableYAMLEntity obj);

    /// <summary>
    /// [Deserialization] Setting up the current object properties from the <see cref="YAMLObject"/> object.
    /// </summary>
    /// <param name="obj">YAML representation of the object.</param>
    public void FromYAML(in IReadableYAMLEntity obj);
}
