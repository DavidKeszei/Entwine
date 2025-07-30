using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Entwine;

/// <summary>
/// Helper class for create lexical tokens from a <see cref="YamlTokenSource"/>.
/// </summary>
internal class YamlLexer: IDisposable {

    #region CONSTANTS
    internal const int MAX_BUFFER_COUNT = 4096;

    private const char STRING_ENCLOSING_TAG_DOUBLE_QUOTE = '\"';
    private const char STRING_ENCLOSING_TAG_SINGLE_QUOTE = '\'';

    private const char ASSING_OPERATOR_TOKEN = ':';
    private const char FLOW_STR_TOKEN = '|';

    private const char MULTILANE_STR_TOKEN = '>';
    private const char VERTICAL_COLLECTION_TOKEN = '-';

    private const char COMMENT_LINE_TOKEN = '#';
    private const char WHITE_SPACE = ' ';

    private const char INLINE_COLLECTION_START_TOKEN = '[';
    private const char INLINE_COLLECTION_ENCOLSING_TOKEN = ']';

    private const string WIN_NEWLINE_ESCAPE_SEQ = "\r\n";
    #endregion

    private YamlTokenSource _file = default;
    private (int line, int character) m_position = (0, 0);

    public YamlLexer(StreamReader utf8)
        => this._file = new YamlTokenSource(reader: utf8);

    public YamlLexer(string content)
        => this._file = new YamlTokenSource(yaml: content);

    public void Dispose() 
        => this._file.Dispose();

    /// <summary>
    /// Create tokens from a YAML string or stream.
    /// </summary>
    /// <returns>Return a collection of <see cref="YamlToken"/>s.</returns>
    /// <exception cref="IndexOutOfRangeException"/>
    /// <exception cref="YamlLexerException"/>
    public Task<List<YamlToken>> CreateTokens() {
        List<YamlToken> tokens = new List<YamlToken>(capacity: _file.PossibleTokenCount);
        Span<char> buffer = stackalloc char[MAX_BUFFER_COUNT];

        int index = 0;
        int order = 0;

        int multiStringOrder = -1;
        YamlLexerFlag flags = 0;

        while (!this._file.EOS) {
            if (index >= MAX_BUFFER_COUNT)
                throw new IndexOutOfRangeException(message: $"The supported buffer length for one line is {MAX_BUFFER_COUNT} character.");

            buffer[index++] = (char)_file.Read();
            ++m_position.character;

            if ((flags & YamlLexerFlag.IS_START_OF_THE_LINE) == YamlLexerFlag.IS_START_OF_THE_LINE && buffer[index - 1] == WHITE_SPACE) {
                ++order;
                --index;

                continue;
            }
            else {
                flags &= ~YamlLexerFlag.IS_START_OF_THE_LINE;

                if(buffer[0] == COMMENT_LINE_TOKEN)
                    flags |= YamlLexerFlag.IS_COMMENT_LINE;
            }

            if ((flags & YamlLexerFlag.IS_MULTILINE_STR) == YamlLexerFlag.IS_MULTILINE_STR && multiStringOrder == -1 && tokens[^2].Type == YamlTokenType.MultilineStringIndicator) 
                multiStringOrder = order;

            switch (buffer[index - 1]) {
                case ':':
                    if (IsString(order, ref flags, ref multiStringOrder))
                        continue;

                    tokens.Add(new YamlToken(token: buffer[..(index - 1)].Trim().ToString(), YamlTokenType.Identifier, order));
                    tokens.Add(new YamlToken(character: ASSING_OPERATOR_TOKEN, YamlTokenType.Assign, order));

                    index = 0;
                    flags |= YamlLexerFlag.ASSIGN_CHARACTER_IS_REACHED;
                    break;
                case '\n':
                case '\r':
                    if((flags & YamlLexerFlag.IS_STR) == YamlLexerFlag.IS_STR && (flags & YamlLexerFlag.IS_MULTILINE_STR) != YamlLexerFlag.IS_MULTILINE_STR)
                        throw new YamlLexerException(msg: "The inline string value must have an enclosing tag.", m_position.line - 1, m_position.character);

                    if (buffer[index - 1] == WIN_NEWLINE_ESCAPE_SEQ[0]) 
                        _ = this._file.Read();

                    if (!buffer[..(index - 1)].SequenceEqual(other: [WHITE_SPACE]) && !buffer[..(index - 1)].SequenceEqual(other: string.Empty))
                        tokens.Add(new YamlToken(token: buffer[..(index - 1)].Trim().ToString(), type: YamlTokenType.Value, order));

                    if (tokens.Count == 0 || tokens[^1].Type != YamlTokenType.NewLine)
                        tokens.Add(new YamlToken(character: WIN_NEWLINE_ESCAPE_SEQ[1], type: YamlTokenType.NewLine, order));

                    index = 0;
                    order = 0;

                    flags |= YamlLexerFlag.IS_START_OF_THE_LINE;
                    flags &= ~YamlLexerFlag.ASSIGN_CHARACTER_IS_REACHED;

                    ++m_position.line;
                    m_position.character = 0;
                    break;
                case '-':
                    if (IsString(order, ref flags, ref multiStringOrder))
                        continue;

                    while (_file.Peek() == WHITE_SPACE) {
                        _ = _file.Read();
                        ++order;
                    }

                    tokens.Add(new YamlToken(character: VERTICAL_COLLECTION_TOKEN, type: YamlTokenType.VerticalArrayIndicator, ++order));
                    index = 0;
                    break;
                case '|':
                case '>':
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
                    
                    if (((flags & YamlLexerFlag.IS_STR) == YamlLexerFlag.IS_STR && (_file.Peek() == STRING_ENCLOSING_TAG_SINGLE_QUOTE || _file.Peek() == STRING_ENCLOSING_TAG_DOUBLE_QUOTE)) ||
                         (_file.Peek() == STRING_ENCLOSING_TAG_DOUBLE_QUOTE || _file.Peek() == STRING_ENCLOSING_TAG_SINGLE_QUOTE)) {

                        _ = _file.Read();
                        break;
                    }

                    if ((flags & YamlLexerFlag.IS_STR) == YamlLexerFlag.IS_STR) {
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
                    if(_file.Peek() == INLINE_COLLECTION_START_TOKEN || _file.Peek() == INLINE_COLLECTION_ENCOLSING_TOKEN) {
                        _ = _file.Read();
                        continue;
                    }

                    if ((flags & YamlLexerFlag.IS_INLINE_COLLECTION) != YamlLexerFlag.IS_INLINE_COLLECTION) {
                        tokens.Add(new YamlToken(character: INLINE_COLLECTION_START_TOKEN, type: YamlTokenType.InlineArrayIndicator, order));
                        flags |= YamlLexerFlag.IS_INLINE_COLLECTION;
                    }
                    else {
                        ParseCollection(tokens, order, buffer[..(index - 1)]);
                        flags &= ~YamlLexerFlag.IS_INLINE_COLLECTION;
                    }
                    
                    index = 0;
                    break;
                case '#':
                    if((flags & YamlLexerFlag.IS_COMMENT_LINE) == YamlLexerFlag.IS_COMMENT_LINE) {
                        _ = _file.ReadLine();
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

        if (index != 0)
            tokens.Add(new YamlToken(token: buffer[..index].ToString().Trim(), type: YamlTokenType.Value, order));

        return Task.FromResult<List<YamlToken>>(result: tokens);
    }

    private bool IsString(int order, ref YamlLexerFlag flags, ref int multiStringOrder) {
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
        ReadOnlySpan<char> trimChars = stackalloc char[3] { WHITE_SPACE, STRING_ENCLOSING_TAG_DOUBLE_QUOTE, STRING_ENCLOSING_TAG_SINGLE_QUOTE };
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

        tokens.Add(new YamlToken(character: INLINE_COLLECTION_ENCOLSING_TOKEN, type: YamlTokenType.InlineArrayIndicator, indentation: order));
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
