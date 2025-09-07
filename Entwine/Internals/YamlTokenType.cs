namespace Entwine;

/// <summary>
/// Collection of possible YAML lexical units.
/// </summary>
internal enum YamlTokenType: byte {
    /// <summary>
    /// Unknow token type. This automatically rises a error.
    /// </summary>
    None,
    /// <summary>
    /// Identifier type of a value.
    /// </summary>
    Identifier,
    /// <summary>
    /// A ':' character of the YAML token.
    /// </summary>
    Assign,
    /// <summary>
    /// A '\n' character of the YAML token.
    /// </summary>
    NewLine,
    /// <summary>
    /// A '\"' or a '\'' character.
    /// </summary>
    StringLiteralIndicator,
    /// <summary>
    /// Indicates the current string broken to multiple lines. (The '>' character)
    /// </summary>
    MultilineStringIndicator,
    /// <summary>
    /// Represent the start or the end of the inline array. (The '[' or the ']' character.)
    /// </summary>
    InlineArrayIndicator,
    /// <summary>
    /// Represent an array, which is positioned in the vertical axis. (The '-' character after the id+delimiter+newline character.)
    /// </summary>
    VerticalArrayIndicator,
    /// <summary>
    /// Represent any value after the ':' character. (Exclude: ["\n", "\'", "\""])
    /// </summary>
    Value
}
