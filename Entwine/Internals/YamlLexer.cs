using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Entwine;

/// <summary>
/// Helper class for create lexical tokens from a <see cref="YAMLSource"/>.
/// </summary>
internal class YamlLexer: IDisposable {

    #region CONSTANTS
    internal const int MAX_BUFFER_COUNT = (1 << 12);

    internal const char STR_DOUBLE_QUOTE = '\"';
    internal const char STR_SINGLE_QUOTE = '\'';

    internal const char ASSING = ':';
    internal const char STR_FLOW = '>';

    internal const char STR_MULTILINE = '|';
    internal const char VERTICAL_COLLECTION = '-';

    internal const char COMMENT = '#';
    internal const char WHITE_SPACE = ' ';

    internal const char INLINE_COLLECTION_START = '[';
    internal const char INLINE_COLLECTION_END = ']';

    internal const string NEW_LINE = "\r\n";
    #endregion

    private YAMLSource m_file = default;
    private (int line, int character) m_position = (0, 0);

    public YamlLexer(YAMLSource source) => m_file = source;

    public void Dispose() => this.m_file.Dispose();

    /// <summary>
    /// Create tokens from a YAML string or stream.
    /// </summary>
    /// <returns>Return a collection of <see cref="YamlToken"/>s.</returns>
    /// <exception cref="IndexOutOfRangeException"/>
    /// <exception cref="YamlLexerException"/>
    public Task<List<YamlToken>> CreateTokens() {
        List<YamlToken> tokens = new List<YamlToken>(capacity: m_file.PossibleTokenCount);
        Span<char> buffer = stackalloc char[MAX_BUFFER_COUNT];

        int index = 0;
        int order = 0;

        int multiStringOrder = -1;
        YamlLexerFlag flags = 0;

        while (!this.m_file.EOS) {
            if (index >= MAX_BUFFER_COUNT)
                throw new IndexOutOfRangeException(message: $"The supported buffer length for one line is {MAX_BUFFER_COUNT} character.");

            buffer[index++] = (char)m_file.Read();
            ++m_position.character;

            if ((flags & YamlLexerFlag.IS_START_OF_THE_LINE) == YamlLexerFlag.IS_START_OF_THE_LINE && buffer[index - 1] == WHITE_SPACE) {
                ++order;
                --index;

                continue;
            }
            else {
                flags &= ~YamlLexerFlag.IS_START_OF_THE_LINE;

                if(buffer[0] == COMMENT) 
                    flags |= YamlLexerFlag.IS_COMMENT_LINE;
            }

            if ((flags & YamlLexerFlag.IS_MULTILINE_STR) == YamlLexerFlag.IS_MULTILINE_STR && multiStringOrder == -1 && tokens[^2].Type == YamlTokenType.MultilineStringIndicator) 
                multiStringOrder = order;

            switch (buffer[index - 1]) {
                case ':':
                    if (IsString(order, ref flags, ref multiStringOrder)) break;

                    tokens.Add(new YamlToken(token: buffer[..(index - 1)].Trim().ToString(), YamlTokenType.Identifier, order));
                    tokens.Add(new YamlToken(character: ASSING, YamlTokenType.Assign, order));

                    index = 0;
                    flags |= YamlLexerFlag.ASSIGN_CHARACTER_IS_REACHED;
                    break;
                case '\n':
                case '\r':
                    if((flags & YamlLexerFlag.IS_STR) == YamlLexerFlag.IS_STR && (flags & YamlLexerFlag.IS_MULTILINE_STR) != YamlLexerFlag.IS_MULTILINE_STR)
                        throw new YamlLexerException(msg: "The inline string value must have an enclosing tag.", line: m_position.line, character: m_position.character);

                    if (buffer[index - 1] == NEW_LINE[0]) 
                        _ = this.m_file.Read();

                    if (!buffer[..(index - 1)].SequenceEqual(other: [WHITE_SPACE]) && !buffer[..(index - 1)].SequenceEqual(other: string.Empty))
                        tokens.Add(new YamlToken(token: buffer[..(index - 1)].Trim().ToString(), type: YamlTokenType.Value, order));

                    if (tokens.Count == 0 || tokens[^1].Type != YamlTokenType.NewLine)
                        tokens.Add(new YamlToken(character: NEW_LINE[1], type: YamlTokenType.NewLine, order));

                    index = 0;
                    order = 0;

                    flags |= YamlLexerFlag.IS_START_OF_THE_LINE;
                    flags &= ~YamlLexerFlag.ASSIGN_CHARACTER_IS_REACHED;

                    ++m_position.line;
                    m_position.character = 0;
                    break;
                case '-':
                    if (IsString(order, ref flags, ref multiStringOrder)) break;

                    while (m_file.Peek() == WHITE_SPACE) {
                        _ = m_file.Read();
                        ++order;
                    }

                    tokens.Add(new YamlToken(character: VERTICAL_COLLECTION, type: YamlTokenType.VerticalArrayIndicator, ++order));
                    index = 0;
                    break;
                case '|':
                case '>':
                    if (IsString(order, ref flags, ref multiStringOrder))
                        continue;

                    char peek = (char)m_file.Peek();
                    if(peek == '-' || peek == '+') _ = m_file.Read();

                    tokens.Add(new YamlToken(character: buffer[index - 1], YamlTokenType.MultilineStringIndicator, indentation: order));
                    flags |= YamlLexerFlag.IS_MULTILINE_STR;
                    index = 0;
                    break;
                case '\'':
                case '\"':
                    if ((flags & YamlLexerFlag.IS_MULTILINE_STR) == YamlLexerFlag.IS_MULTILINE_STR)
                        break;

                    if ((flags & YamlLexerFlag.IS_INLINE_COLLECTION) == YamlLexerFlag.IS_INLINE_COLLECTION) {
                        --index;
                        break;
                    }

                    if ((flags & YamlLexerFlag.IS_STR) == YamlLexerFlag.IS_STR) {
                        if (m_file.Peek() == STR_DOUBLE_QUOTE || m_file.Peek() == STR_SINGLE_QUOTE) {
                            _ = (char)m_file.Read();
                            break;
                        }

                        flags &= ~YamlLexerFlag.IS_STR;
                        tokens.Add(new YamlToken(token: buffer[..(index - 1)].Trim().ToString(), type: YamlTokenType.Value, order));
                    }
                    else {
                        flags |= YamlLexerFlag.IS_STR;
                    }

                    tokens.Add(new YamlToken(character: buffer[index - 1], YamlTokenType.StringLiteralIndicator, indentation: order));
                    index = 0;
                    break;
                case '[':
                case ']':
                    if((flags & YamlLexerFlag.IS_STR) == YamlLexerFlag.IS_STR || (flags & YamlLexerFlag.IS_MULTILINE_STR) == YamlLexerFlag.IS_MULTILINE_STR) break;

                    if ((flags & YamlLexerFlag.IS_INLINE_COLLECTION) != YamlLexerFlag.IS_INLINE_COLLECTION) {
                        tokens.Add(new YamlToken(character: INLINE_COLLECTION_START, type: YamlTokenType.InlineArrayIndicator, order));
                        flags |= YamlLexerFlag.IS_INLINE_COLLECTION;
                    }
                    else {
                        ParseCollection(tokens, order, buffer[..(index - 1)]);
                        flags &= ~YamlLexerFlag.IS_INLINE_COLLECTION;
                    }
                    
                    index = 0;
                    break;
                case '#':
                    if((flags & YamlLexerFlag.IS_COMMENT_LINE) == YamlLexerFlag.IS_COMMENT_LINE && (tokens.Count == 0 || tokens[^1].Type == YamlTokenType.NewLine)) {
                        _ = m_file.ReadLine();
                        index = 0;

                        if(tokens.Count > 0) {
                            for(int i = tokens.Count - 1; tokens[i].Type != YamlTokenType.NewLine; --i)
                                _ = tokens.Remove(tokens[i]);

                            flags = 0;
                        }

                        ++m_position.line;
                    }
                    break;
            }
        }

        if ((flags & YamlLexerFlag.IS_STR) == YamlLexerFlag.IS_STR && (flags & YamlLexerFlag.IS_MULTILINE_STR) != YamlLexerFlag.IS_MULTILINE_STR)
            throw new YamlLexerException(msg: "The inline string value must have an enclosing tag.", line: m_position.line, character: m_position.character);

        if (index != 0)
            tokens.Add(new YamlToken(token: buffer[..index].ToString().Trim(), type: YamlTokenType.Value, order));

        return Task.FromResult<List<YamlToken>>(result: tokens);
    }

    private bool IsString(int order, ref YamlLexerFlag flags, ref int multiStringOrder) {
        /* Unset multiline-string flags if indentation is changed. */
        if ((flags & YamlLexerFlag.IS_MULTILINE_STR) == YamlLexerFlag.IS_MULTILINE_STR && order != multiStringOrder && multiStringOrder != -1) {
            flags &= ~YamlLexerFlag.IS_MULTILINE_STR;

            if ((flags & YamlLexerFlag.IS_STR) == YamlLexerFlag.IS_STR)
                flags &= ~YamlLexerFlag.IS_STR;

            multiStringOrder = -1;
        }

        if ((flags & YamlLexerFlag.IS_STR) == YamlLexerFlag.IS_STR || (flags & YamlLexerFlag.IS_MULTILINE_STR) == YamlLexerFlag.IS_MULTILINE_STR ||
            (flags & YamlLexerFlag.ASSIGN_CHARACTER_IS_REACHED) == YamlLexerFlag.ASSIGN_CHARACTER_IS_REACHED)
            return true;

        return false;
    }

    private void ParseCollection(List<YamlToken> tokens, int order, ReadOnlySpan<char> buff) {
        ReadOnlySpan<char> trimChars = stackalloc char[3] { WHITE_SPACE, STR_DOUBLE_QUOTE, STR_SINGLE_QUOTE };
        int start = 0;

        for (int i = 0; i < buff.Length; ++i) {
            if (buff[i] == ',') {
                ReadOnlySpan<char> trim = buff[start..i].Trim(trimChars);

                if(trim.IsEmpty)
                    throw new YamlLexerException(msg: $"A collection entry must declared somehow in the collection with a value or NULL.", 
                                                  line: m_position.line - 1, 
                                                  character: m_position.character);

                tokens.Add(new YamlToken(token: trim.ToString(), type: YamlTokenType.Value, indentation: order));
                start = i + 1;
            }
        }

        if (buff[start..].Length != 0)
            tokens.Add(new YamlToken(buff[start..].Trim(trimChars).ToString(), YamlTokenType.Value, order));

        tokens.Add(new YamlToken(character: INLINE_COLLECTION_END, type: YamlTokenType.InlineArrayIndicator, indentation: order));
    }

    [Flags]
    private enum YamlLexerFlag: int {
        IS_STR = (1 << 0),
        IS_START_OF_THE_LINE = (1 << 1),
        IS_MULTILINE_STR = (1 << 2),
        IS_INLINE_COLLECTION = (1 << 3),
        ASSIGN_CHARACTER_IS_REACHED = (1 << 4),
        IS_COMMENT_LINE = (1 << 5)
    }
}
