namespace YAMLyzer;

/// <summary>
/// Smallest unit of the YAML string by the parser.
/// </summary>
public readonly struct YamlToken {
    private readonly string _token = string.Empty;
    private readonly YamlTokenType _type = YamlTokenType.None;

    private readonly int _order = 0;

    /// <summary>
    /// Raw value of the YAML token.
    /// </summary>
    public string Value { get => _token; }

    /// <summary>
    /// Type of the YAML token.
    /// </summary>
    public YamlTokenType Type { get => _type; }

    /// <summary>
    /// Indicates the order/indentation of the token.
    /// </summary>
    public int Order { get => _order; }

    public YamlToken(string token, YamlTokenType type, int order) {
        this._token = token;
        this._type = type;

        this._order = order;
    }
}

/// <summary>
/// Collection of possible YAML primitives.
/// </summary>
public enum YamlTokenType: byte {
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
    Delimiter,
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
    /// The current value is undefined. (The 'null' or '~' character.)
    /// </summary>
    Null,
    /// <summary>
    /// Represent the start or the end of the inline array. (The '[' or the ']' character.)
    /// </summary>
    InlineArrayIndicator,
    /// <summary>
    /// Represent an array, which is positioned in the vertical axis. (The '-' character after the id+delimiter+newline character.)
    /// </summary>
    VerticalArrayIndicator,
    /// <summary>
    /// Represent a comment in the YAML file. (The '#' character.)
    /// </summary>
    Comment,
    Value
}

/*
# This file is auto-generated. Don't modify anything in this file.
path: 'C:/david/source/repos/YAMLyzer'
name: 'YAMLyzer'
desc: >
    This project help de-/serialize objects to YAML string and vice-versa.
presets:
    - 'C:/david/source/repos/YAMLyzer/Debug_x86.yaml'
    - 'C:/david/source/repos/YAMLyzer/Release_x86'
supported_cli_versions: [ '.NET8', '.NET9' ]
*/
