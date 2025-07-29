namespace YAMLyzer;

/// <summary>
/// Smallest unit of the YAML string by the parser.
/// </summary>
internal readonly struct YamlToken {
    private readonly string _token = string.Empty;
    private readonly YamlTokenType _type = YamlTokenType.None;

    private readonly int _indentation = 0;

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
    public int Indentation { get => _indentation; }

    public YamlToken(string token, YamlTokenType type, int order) {
        this._token = token;
        this._type = type;

        this._indentation = order;
    }
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
