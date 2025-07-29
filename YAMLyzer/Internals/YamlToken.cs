namespace YAMLyzer;

/// <summary>
/// Smallest unit of the YAML string by the parser.
/// </summary>
internal readonly struct YamlToken {
    private readonly string m_token = string.Empty;
    private readonly YamlTokenType m_type = YamlTokenType.None;

    private readonly int m_indentation = 0;

    /// <summary>
    /// Raw value of the YAML token.
    /// </summary>
    public string Value { get => m_token; }

    /// <summary>
    /// Type of the YAML token.
    /// </summary>
    public YamlTokenType Type { get => m_type; }

    /// <summary>
    /// Indicates the order/indentation of the token.
    /// </summary>
    public int Indentation { get => m_indentation; }

    public YamlToken(string token, YamlTokenType type, int indentation) {
        this.m_token = token;
        this.m_type = type;

        this.m_indentation = indentation;
    }

    public YamlToken(char character, YamlTokenType type, int indentation) {
        this.m_token = string.Join<char>(separator: string.Empty, values: [character]);

        this.m_type = type;
        this.m_indentation = indentation;
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
