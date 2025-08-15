using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entwine;

/// <summary>
/// Provides de-/serialization mechanisms for object(s).
/// </summary>
public interface ISerializable {

    /// <summary>
    /// [Serialization] Setting up the <see cref="IWriteableEntity"/> <paramref name="obj"/> properties from the current object.
    /// </summary>
    /// <param name="obj">Result object reference of YAML representation.</param>
    public void ToYAML(in IWriteableEntity obj);

    /// <summary>
    /// [Deserialization] Setting up the current object properties from the <see cref="IReadableEntity"/> object.
    /// </summary>
    /// <param name="obj">YAML representation of the object.</param>
    public void FromYAML(in IReadableEntity obj);
}
