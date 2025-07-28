using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YAMLyzer;

public class YamlLexerException: Exception {
    private string _msg = string.Empty;

    public YamlLexerException(string msg, int line, int character): 
        base(message: $"{msg} (Line: {line}, Character: {character})") { }
}
