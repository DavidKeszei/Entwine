using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace YAMLyzer;

/// <summary>
/// Represent a source for a YAML string.
/// </summary>
[StructLayout(layoutKind: LayoutKind.Explicit)]
internal struct YamlTokenSource: IDisposable {
    [FieldOffset(offset: 0)] private StreamReader _reader = null!;
    [FieldOffset(offset: 0)] private string _str = null!;

    [FieldOffset(offset: 8)]  private int _strReaderPosition = 0;
    [FieldOffset(offset: 12)] private bool _isStream = false;

    /// <summary>
    /// Indicates the content is reached the end of the file.
    /// </summary>
    public readonly bool EOS { get => _isStream ? _reader.EndOfStream : _str.Length <= _strReaderPosition; }

    /// <summary>
    /// Provides a token count tip from the source.
    /// </summary>
    public readonly int PossibleTokenCount { get => TipPossibleTokenCount();  }

    public YamlTokenSource(StreamReader reader) {
        this._reader = reader;
        this._isStream = true;
    }

    public YamlTokenSource(string yaml)
        => this._str = yaml;

    public void Dispose() {
        if (_isStream)
            _reader.Dispose();
    }

    /// <summary>
    /// Read one character from the content and step forward the cursor by one.
    /// </summary>
    /// <returns>Return the character code. If reached the EOF the content, then return -1.</returns>
    public int Read() {
        if (_isStream)
            return _reader.Read();
    
        return _strReaderPosition >= _str.Length ? -1 : _str[_strReaderPosition++];
    }

    /// <summary>
    /// Peek one character from the content,
    /// </summary>
    /// <returns>Return the character code. If reached the EOF the content, then return -1.</returns>
    public readonly int Peek() {
        if (_isStream)
            return _reader.Peek();

        return _strReaderPosition >= _str.Length ? -1 : _str[_strReaderPosition];
    }

    /// <summary>
    /// Read one line from the content and step forward the cursor by length of the line.
    /// </summary>
    /// <returns>Return a line. If reached the EOF the content or the character is a newline, then return <see cref="string.Empty"/>.</returns>
    public string ReadLine() {
        if (_isStream)
            return _reader.ReadLine() ?? string.Empty;

        int newLine = _str.IndexOf(value: Environment.NewLine, startIndex: _strReaderPosition);
        string line = _str[_strReaderPosition..newLine];

        _strReaderPosition = newLine + 1;
        return line;
    }

    private readonly int TipPossibleTokenCount() {
        if (!_isStream) {
            int delimiterCount = 0;

            for (int i = 0; i < _str.Length; ++i) {
                if (_str[i] == ':')
                    ++delimiterCount;
            }

            return delimiterCount == 0 ? 32 : delimiterCount * 4; 
        }

        int tokenCount = (int)(_reader.BaseStream.Length / YamlLexer.MAX_BUFFER_COUNT / 4);
        return tokenCount < 128 ? 128 : tokenCount;
    }
}
