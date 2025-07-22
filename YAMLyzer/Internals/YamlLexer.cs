using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YAMLyzer;

/// <summary>
/// Helper class for create lexical tokens from a <see cref="YamlTokenSource"/>.
/// </summary>
internal class YamlLexer: IDisposable {
    internal const int MAX_BUFFER_COUNT = 4096;

    private const char STRING_ENCLOSING_TAG_DOUBLE_QUOTE = '\"';
    private const char STRING_ENCLOSING_TAG_SINGLE_QUOTE = '\'';

    private YamlTokenSource _file = default;

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
    /// <exception cref="YamlParserException"/>
    public Task<List<YamlToken>> CreateTokens() {
        List<YamlToken> tokens = new List<YamlToken>(capacity: _file.PossibleTokenCount);
        Span<char> buffer = stackalloc char[MAX_BUFFER_COUNT];

        int index = 0;
        int order = 0;

        int multiStringOrder = -1;
        int line = 0;

        int lineCharacterPosition = 0;
        YamlLexerFlag flags = 0;

        while (!this._file.EOS) {
            if (index >= MAX_BUFFER_COUNT)
                throw new IndexOutOfRangeException(message: $"The supported buffer length for one line is {MAX_BUFFER_COUNT} character.");

            buffer[index++] = (char)_file.Read();
            ++lineCharacterPosition;

            if ((flags & YamlLexerFlag.IS_START_OF_THE_LINE) == YamlLexerFlag.IS_START_OF_THE_LINE && buffer[index - 1] == ' ') {
                ++order;
                --index;

                continue;
            }
            else {
                flags &= ~YamlLexerFlag.IS_START_OF_THE_LINE;
            }

            if ((flags & YamlLexerFlag.IS_MULTILINE_STR) == YamlLexerFlag.IS_MULTILINE_STR && multiStringOrder == -1) 
                multiStringOrder = order;

            switch (buffer[index - 1]) {
                case ':':
                    if (IsMString(order, ref flags, ref multiStringOrder))
                        continue;

                    tokens.Add(new YamlToken(token: buffer[..(index - 1)].Trim().ToString(), YamlTokenType.Identifier, order));
                    tokens.Add(new YamlToken(token: ":", YamlTokenType.Delimiter, order));

                    index = 0;
                    flags |= YamlLexerFlag.DELIMITER_IS_REACHED;
                    break;
                case '\n':
                case '\r':
                    if((flags & YamlLexerFlag.IS_STR) == YamlLexerFlag.IS_STR)
                        throw new YamlParserException(msg: "The string value not has enclosing character.", line, lineCharacterPosition);

                    if (buffer[index - 1] == '\r') _ = this._file.Read();

                    if (!buffer[..(index - 1)].SequenceEqual(other: [' ']) && !buffer[..(index - 1)].SequenceEqual(other: ""))
                        tokens.Add(new YamlToken(token: buffer[..(index - 1)].Trim().ToString(), type: YamlTokenType.Value, order));

                    if (tokens.Count == 0 || tokens[^1].Type != YamlTokenType.NewLine)
                        tokens.Add(new YamlToken(token: "\n", type: YamlTokenType.NewLine, order));

                    index = 0;
                    order = 0;

                    flags |= YamlLexerFlag.IS_START_OF_THE_LINE;
                    flags &= ~YamlLexerFlag.DELIMITER_IS_REACHED;

                    ++line;
                    lineCharacterPosition = 0;
                    break;
                case '-':
                    if (IsMString(order, ref flags, ref multiStringOrder))
                        continue;

                    while (_file.Peek() == ' ') {
                        _ = _file.Read();
                        ++order;
                    }

                    tokens.Add(new YamlToken(token: "-", type: YamlTokenType.VerticalArrayIndicator, ++order));
                    index = 0;
                    break;
                case '|':
                case '>':
                    tokens.Add(new YamlToken(token: $"{buffer[index - 1]}", YamlTokenType.MultilineStringIndicator, order: order));
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

                    if ((flags & YamlLexerFlag.IS_STR) == YamlLexerFlag.IS_STR && (_file.Peek() == STRING_ENCLOSING_TAG_SINGLE_QUOTE || _file.Peek() == STRING_ENCLOSING_TAG_DOUBLE_QUOTE)) {
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

                    tokens.Add(new YamlToken(token: $"{buffer[index - 1]}", YamlTokenType.StringLiteralIndicator, order: order));
                    index = 0;
                    break;
                case '[':
                case ']':
                    if ((flags & YamlLexerFlag.IS_INLINE_COLLECTION) != YamlLexerFlag.IS_INLINE_COLLECTION) {
                        tokens.Add(new YamlToken(token: "[", type: YamlTokenType.InlineArrayIndicator, order));
                        flags |= YamlLexerFlag.IS_INLINE_COLLECTION;
                    }
                    else {
                        CreateArray(tokens, order, buffer[..(index - 1)]);
                        flags &= ~YamlLexerFlag.IS_INLINE_COLLECTION;
                    }
                    
                    index = 0;
                    break;
                case '#':
                    //## indicates one # in the string, otherwise this a comment. 
                    if (_file.Peek() == '#') {

                        if ((flags & YamlLexerFlag.IS_MULTILINE_STR) == YamlLexerFlag.IS_MULTILINE_STR) {
                            buffer[index++] = '#';
                        }

                        _ = _file.Read();
                        break;
                    }

                    if (buffer[index - 1] == '#' && index - 1 != 0)
                        break;

                    _ = _file.ReadLine();

                    index = 0;

                    if (tokens.Count > 0) {
                        for (int i = tokens.Count - 1; tokens[i].Type != YamlTokenType.NewLine; --i)
                            _= tokens.Remove(tokens[i]);

                        flags = 0;
                    }

                    ++line;
                    break;
            }
        }

        if (index != 0)
            tokens.Add(new YamlToken(token: buffer[..index].ToString().Trim(), type: YamlTokenType.Value, order));

        return Task.FromResult<List<YamlToken>>(result: tokens);
    }

    private bool IsMString(int order, ref YamlLexerFlag flags, ref int multiStringOrder) {
        if ((flags & YamlLexerFlag.IS_MULTILINE_STR) == YamlLexerFlag.IS_MULTILINE_STR && order != multiStringOrder && multiStringOrder != -1) {
            flags &= ~YamlLexerFlag.IS_MULTILINE_STR;

            if ((flags & YamlLexerFlag.IS_STR) == YamlLexerFlag.IS_STR)
                flags &= ~YamlLexerFlag.IS_STR;

            multiStringOrder = -1;
        }

        if ((flags & YamlLexerFlag.IS_STR) == YamlLexerFlag.IS_STR || (flags & YamlLexerFlag.IS_MULTILINE_STR) == YamlLexerFlag.IS_MULTILINE_STR ||
            (flags & YamlLexerFlag.DELIMITER_IS_REACHED) == YamlLexerFlag.DELIMITER_IS_REACHED)
            return true;

        return false;
    }

    private void CreateArray(List<YamlToken> tokens, int order, ReadOnlySpan<char> buff) {
        int start = 0;

        for (int i = 0; i < buff.Length; ++i) {
            if (buff[i] == ',') {
                ReadOnlySpan<char> trim = buff[start..i].Trim(trimChars: [' ', '\'', '\"']);

                tokens.Add(new YamlToken(trim.ToString(), YamlTokenType.Value, order));
                start = i + 1;
            }
        }

        if (buff[start..].Length != 0)
            tokens.Add(new YamlToken(buff[start..].Trim([' ', '\'', '\"']).ToString(), YamlTokenType.Value, order));

        tokens.Add(new YamlToken(token: "]", type: YamlTokenType.InlineArrayIndicator, order));
    }

    [Flags]
    private enum YamlLexerFlag: int {
        IS_STR = (1 << 0),
        IS_START_OF_THE_LINE = (1 << 1),
        IS_MULTILINE_STR = (1 << 2),
        IS_INLINE_COLLECTION = (1 << 3),
        DELIMITER_IS_REACHED = (1 << 4)
    }
}
