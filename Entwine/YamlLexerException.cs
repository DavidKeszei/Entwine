using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entwine;

/// <summary>
/// Represents an error from the <see cref="YamlLexer"/>.
/// </summary>
public class YamlLexerException: Exception {
    private string m_msg = string.Empty;

    public YamlLexerException(string msg, int line, int character): 
        base(message: $"{msg} (Line: {line}, Character: {character})") { }
}
