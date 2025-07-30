using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entwine;

/// <summary>
/// Provides identifier capabilities for a YAML entity.
/// </summary>
public interface IEntity {

    /// <summary>
    /// Key of the <see cref="IEntity"/>.
    /// </summary>
    public string Key { get; }

    /// <summary>
    /// Type of the <see cref="IEntity"/>.
    /// </summary>
    public YAMLType TypeOf { get; }
}
