using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Entwine;

/// <summary>
/// Represent a source for a YAML string.
/// </summary>
[StructLayout(layoutKind: LayoutKind.Explicit)]
public struct YAMLSource: IDisposable {
    [FieldOffset(offset: 0)] private StreamReader m_reader = null!;
    [FieldOffset(offset: 0)] private string m_str = null!;

    [FieldOffset(offset: 8)]  private int m_strReaderPosition = 0;
    [FieldOffset(offset: 12)] private bool m_isStream = false;

    /// <summary>
    /// Indicates the content is reached the end of the file.
    /// </summary>
    internal readonly bool EOS { get => m_isStream ? m_reader.EndOfStream : m_str.Length <= m_strReaderPosition; }

    /// <summary>
    /// Provides a token count tip from the source.
    /// </summary>
    internal readonly int PossibleTokenCount { get => TipPossibleTokenCount();  }

    /// <summary>
    /// Implicit conversion between an <see langword="string"/> and <see cref="YAMLSource"/> instance.
    /// </summary>
    /// <param name="source">Source data of the <see cref="YAMLSource"/>.</param>
    public static implicit operator YAMLSource(string source) => new YAMLSource(source);

    /// <summary>
    /// Create new <see cref="YAMLSource"/> instance from a <see cref="Stream"/> instance.
    /// </summary>
    /// <param name="source">YAML source. This can be a file path or a YAML string.</param>
    /// <exception cref="FileNotFoundException"/>
    /// <exception cref="AccessViolationException"/>
    /// <exception cref="IOException"/>
    public YAMLSource(Stream source) {
        this.m_reader = new StreamReader(stream: source);
        this.m_isStream = true;
    }

    /// <summary>
    /// Create new <see cref="YAMLSource"/> instance from a <see cref="string"/> instance.
    /// </summary>
    /// <param name="source">YAML source.</param>
    public YAMLSource(string source) {
        this.m_str = source;
        this.m_isStream = false;
    }

    public void Dispose() {
        if (m_isStream)
            m_reader.Dispose();
    }

    /// <summary>
    /// Read one character from the content and step forward the cursor by one.
    /// </summary>
    /// <returns>Return the character code. If reached the <see cref="EOS"/> the content, then return -1.</returns>
    internal int Read() {
        if (m_isStream)
            return m_reader.Read();
    
        return m_strReaderPosition >= m_str.Length ? -1 : m_str[m_strReaderPosition++];
    }

    /// <summary>
    /// Peek one character from the content.
    /// </summary>
    /// <returns>Return the character code. If reached the <see cref="EOS"/> the content, then return -1.</returns>
    internal readonly int Peek() {
        if (m_isStream)
            return m_reader.Peek();

        return m_strReaderPosition >= m_str.Length ? -1 : m_str[m_strReaderPosition];
    }

    /// <summary>
    /// Read one line from the content and step forward the cursor by length of the line.
    /// </summary>
    /// <returns>Return a line. If reached the EOF the content or the character is a newline, then return <see cref="string.Empty"/>.</returns>
    internal string ReadLine(bool beginInTheLine = true) {
        if(m_isStream)
            return m_reader.ReadLine() ?? string.Empty;

        int newLine = m_str.IndexOf(value: Environment.NewLine, startIndex: m_strReaderPosition, comparisonType: StringComparison.CurrentCulture);
        string line = null!;

        if(newLine == -1) {
            line = m_str[m_strReaderPosition..];

            m_strReaderPosition = m_str.Length;
            return line;
        }

        line = m_str[m_strReaderPosition..newLine];
        m_strReaderPosition = newLine + 1;

        return line;
    }

    private readonly int TipPossibleTokenCount() {
        if (!m_isStream) {
            int delimiterCount = 0;

            for (int i = 0; i < m_str.Length; ++i) {
                if (m_str[i] == YamlLexer.ASSING)
                    ++delimiterCount;
            }

            return delimiterCount == 0 ? 32 : delimiterCount * 4; 
        }

        int tokenCount = (int)(m_reader.BaseStream.Length / YamlLexer.MAX_BUFFER_COUNT / 4);
        return tokenCount < 128 ? 128 : tokenCount;
    }
}
