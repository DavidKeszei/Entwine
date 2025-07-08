using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YAMLyzer.Interfaces;

/// <summary>
/// Provides clear/reset functionality for a class.
/// </summary>
public interface IClearable {

    /// <summary>
    /// Clear the current instance to a reset state.
    /// </summary>
    public void Clear();
}
