using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using YAMLyzer.Internals;

namespace YAMLyzer;

/// <summary>
/// Helper class for serialize objects/values to YAML and vice-versa.
/// </summary>
public static class YAMLSerializer {

    /// <summary>
    /// Deserialize a YAML <see cref="string"/> to an object representation.
    /// </summary>
    /// <typeparam name="T">Implementer class/struct of the <see cref="ISerializable"/>.</typeparam>
    /// <param name="source">The YAML <see cref="string"/>.</param>
    /// <returns>Return a(n) <typeparamref name="T"/> instance, if this is successful. Otherwise return <see langword="null"/>.</returns>
    /// <exception cref="FormatException"/>
    public static async Task<T?> Deserialize<T>(string source) where T: ISerializable, new() {
        IReadableEntity obj = await Deserialize(source);
        T deserialized = new T();

        deserialized.FromYAML(in obj);
        return deserialized;
    }

    /// <summary>
    /// Deserialize an YAML <see cref="string"/> to an <see cref="IReadableEntity"/> representation.
    /// </summary>
    /// <param name="source">The YAML string.</param>
    /// <returns>Return an <see cref="IReadableEntity"/> instance.</returns>
    /// <exception cref="FormatException"/>
    public static async Task<IReadableEntity> Deserialize(string source) {
        using YamlLexer lexer = new YamlLexer(source);
        ReadOnlySpan<YamlToken> tokens = CollectionsMarshal.AsSpan(list: await lexer.CreateTokens());
        YAMLObject obj = new YAMLObject(key: YAMLObject.ROOT_OBJECT_INDENTIFIER);

        int count = 1;
        string id = null!;

        if (tokens[0].Type == YamlTokenType.NewLine)
            tokens = tokens[1..];

        while (tokens.Length > count) {
            switch (tokens[count].Type) {

                case YamlTokenType.Delimiter:
                    id = tokens[count - 1].Value;
                    ++count;
                    break;
                case YamlTokenType.NewLine:
                    if (tokens[count - 1].Type == YamlTokenType.InlineArrayIndicator) {
                        count += 2;
                        break;
                    }

                    if (tokens[count - 1].Type == YamlTokenType.Delimiter) {
                        int objEnd = FirstIndentationOf(tokens[count].Indentation, tokens[(count + 1)..]);

                        if (objEnd != -1) {
                            obj.Write<IEntity>(route: [], id, RecursivelyDeserialize(id, tokens[(count + 1)..(count + 1 + objEnd)]));
                            count += objEnd;
                        }
                        else {
                            obj.Write<IEntity>(route: [], id, RecursivelyDeserialize(id, tokens[(count + 1)..]));
                            count = tokens.Length;
                        }

                        id = null!;
                        break;
                    }

                    count += 2;
                    break;
                case YamlTokenType.InlineArrayIndicator:
                    int arrayEnd = IndexOf<YamlToken, char>(searchItem: ']', prop: static (x) => x.Value[0], tokens[count..]);
                    int newlinePos = IndexOf<YamlToken, char>(searchItem: '\n', prop: static (x) => x.Value[0], tokens[count..]);

                    if ((newlinePos < arrayEnd && newlinePos != -1) || arrayEnd == -1)
                        throw new FormatException(message: "The inline array must have a closer character in the YAML file. (\']\')");

                    arrayEnd += count;
                    IEntity[] collectionOf = new YAMLValue[arrayEnd - ++count];

                    for (int i = 0; i < collectionOf.Length; ++i)
                        collectionOf[i] = new YAMLValue(key: IEntity.KEYLESS, tokens[count + i].Value);

                    IEntity _collection = (IEntity)new YAMLCollection(id, collectionOf);
                    obj.Write<IEntity>(route: [], id, _collection);

                    count = arrayEnd + 1;
                    break;
                case YamlTokenType.StringLiteralIndicator:
                    ++count;
                    break;

                case YamlTokenType.MultilineStringIndicator:
                    int mStrEnd = FirstIndentationOf(tokens[count].Indentation, tokens[(count + 2)..]);

                    if (mStrEnd != -1) {
                        obj.Write<string>(route: [], id, CreateMultilineString(tokens[(count + 2)..(count + 2 + mStrEnd)], saveNewlines: tokens[count].Value[0] != '|'));
                        count += mStrEnd + 3;
                    }
                    else {
                        obj.Write<string>(route: [], id, CreateMultilineString(tokens[(count + 2)..], saveNewlines: tokens[count].Value[0] != '|'));
                        count = tokens.Length;
                    }

                    id = null!;
                    break;
                case YamlTokenType.Value:
                    obj.Write<string>(route: [], id, tokens[count].Value);

                    id = null!;

                    if (count < tokens.Length - 1) count += tokens[count + 1].Type == YamlTokenType.NewLine ? 3 : 4;
                    else ++count;
                    break;
            }
        }

        return obj;
    }

    /// <summary>
    /// Serialize a(n) <typeparamref name="T"/> instance to a YAML <see cref="string"/>. 
    /// </summary>
    /// <typeparam name="T">Implementer class of the <see cref="ISerializable"/> interface.</typeparam>
    /// <param name="source">Source instance of the <typeparamref name="T"/> class.</param>
    /// <param name="customKey">Key of the object inside the YAML string.</param>
    /// <returns>Return a new <see cref="string"/> instance.</returns>
    /// <exception cref="ArgumentException"/>
    /// <exception cref="KeyNotFoundException"/>
    /// <exception cref="IndexOutOfRangeException"/>
    public static async Task<string> Serialize<T>(T source, string customKey = null!) where T: ISerializable, new() {
        YAMLObject obj = new YAMLObject();
        obj.Write<IEntity>(route: [], key: customKey ?? nameof(T).ToLower(), value: obj);

        return await Serialize(obj);
    }

    /// <summary>
    /// Serialize an <see cref="IEntity"/> to YAML representation.
    /// </summary>
    /// <param name="entity">The entity, which holds the data.</param>
    public static Task<string> Serialize(IEntity entity) {
        StringBuilder builder = new StringBuilder();

        if (entity is not IReadableEntity)
            return Task.FromResult<string>(result: $"{entity}");

        foreach (IEntity prop in (IReadableEntity)entity) {
            if (prop is IReadableEntity) RecursivelySerialize((IReadableEntity)prop, builder, indentation: 0, isCollection: false);
            else builder.AppendLine($"{prop}");
        }

        return Task.FromResult<string>(result: builder.ToString());
    }

    #region PRIVATE_FUNCTIONS

    private static IEntity RecursivelyDeserialize(string key, ReadOnlySpan<YamlToken> tokens) {
        if(tokens.Length == 0)
            throw new FormatException(message: $"If you want to create a collection or an object, then must be specify 1 item, field or value. (Key: {key})");

        YAMLObject obj = ObjectPool<YAMLObject>.Shared.Rent();
        obj.Key = key;

        YAMLCollection collection = ObjectPool<YAMLCollection>.Shared.Rent();
        collection.Key = key;

        /* Current entity is a vertical collection or not? */
        bool isCollection = tokens[0].Type == YamlTokenType.VerticalArrayIndicator;
        string id = null!;

        /*
         Examples:
             ↱ This must be presented in tokens. (Vertical Collection Indicator)
            [-]["][Entry]["]  OR  [-][name][:]["][Entry]["] => In this scenario must be start with 2
             0  1    2    3        0    1   2  3    4    5

            [name][:]["][Entry]["]  => In this scenario must be start with 1 (When not token range is not a vertical collection)
               0   1   2   3    4 
        */
        int index = isCollection && tokens.Length > 2 && 
                    (tokens[1].Type == YamlTokenType.StringLiteralIndicator || tokens[2].Type == YamlTokenType.Delimiter) ? 2 : 1;

        /* Indicates the current entry is a object or just a primitive value inside in a collection */
        bool isCollectionObjectEntry = IsCollectionObjectEntry(tokens[index..]);

        while (tokens.Length > index) {
            switch (tokens[index].Type) {

                case YamlTokenType.Delimiter:
                    id = tokens[index - 1].Value;
                    ++index;

                    break;
                case YamlTokenType.NewLine:
                    if (tokens[index - 1].Type == YamlTokenType.InlineArrayIndicator) {
                        index += 2;
                        break;
                    }

                    if (tokens[index - 1].Type == YamlTokenType.Delimiter) {
                        int objEnd = FirstIndentationOf(tokens[index].Indentation, tokens[(index + 1)..]);
                        isCollectionObjectEntry = true;

                        if (objEnd != -1) {
                            obj.Write<IEntity>(route: [], id, RecursivelyDeserialize(id, tokens[(index + 1)..(index + 1 + objEnd)]));
                            index += objEnd;
                        }
                        else {
                            obj.Write<IEntity>(route: [], id, RecursivelyDeserialize(id, tokens[(index + 1)..]));
                            index = tokens.Length;
                        }

                        id = null!;
                        break;
                    }

                    if (index + 1 < tokens.Length) index += tokens[index + 1].Type == YamlTokenType.VerticalArrayIndicator ? 1 : 2;
                    else ++index;

                    break;
                case YamlTokenType.VerticalArrayIndicator:
                    if (isCollectionObjectEntry) {
                        obj.Key = IEntity.KEYLESS;

                        collection.InternalCollection.Add(item: obj.AsCopy());
                        obj.Clear();
                    }

                    index += tokens[index + 1].Type == YamlTokenType.Value ? 1 : 2;
                    isCollectionObjectEntry = IsCollectionObjectEntry(tokens[index..]);
                    break;
                case YamlTokenType.InlineArrayIndicator:
                    int arrayEnd = IndexOf<YamlToken, char>(searchItem: ']', prop: static (x) => x.Value[0], tokens[index..]);
                    int newlinePos = IndexOf<YamlToken, char>(searchItem: '\n', prop: static (x) => x.Value[0], tokens[index..]);

                    if ((newlinePos < arrayEnd && newlinePos != -1) || arrayEnd == -1)
                        throw new FormatException(message: "The inline array must have a closer character in the YAML file. (\']\')");

                    arrayEnd += index;
                    IEntity[] collectionOf = new YAMLValue[arrayEnd - ++index];

                    for (int i = 0; i < collectionOf.Length; ++i)
                        collectionOf[i] = new YAMLValue(key: IEntity.KEYLESS, tokens[index + i].Value);

                    IEntity _collection = (IEntity)new YAMLCollection(id, collectionOf);

                    if (isCollection && !isCollectionObjectEntry) collection.InternalCollection.Add(item: _collection);
                    else obj.Write<IEntity>(route: [], id, _collection);

                    index = arrayEnd + 1;
                    break;
                case YamlTokenType.StringLiteralIndicator:
                    ++index;
                    break;
                case YamlTokenType.MultilineStringIndicator:
                    int mStrEnd = FirstIndentationOf(tokens[index].Indentation, tokens[(index + 2)..]);
                    YAMLValue multilineString = null!;

                    if (mStrEnd != -1) {
                        multilineString = new YAMLValue(id, CreateMultilineString(tokens[(index + 2)..(index + 2 + mStrEnd)], saveNewlines: tokens[index].Value[0] != '|'));
                        index += mStrEnd;

                        if(tokens[index + 2].Type == YamlTokenType.VerticalArrayIndicator) index += 2;
                        else index += 3;
                    }
                    else {
                        multilineString = new YAMLValue(id, CreateMultilineString(tokens[(index + 2)..], saveNewlines: tokens[index].Value[0] != '|'));
                        index = tokens.Length;
                    }

                    if (isCollection && !isCollectionObjectEntry) collection.InternalCollection.Add(item: multilineString);
                    else obj.Write<IEntity>(route: [], key: id, value: multilineString);

                    id = null!;
                    break;
                case YamlTokenType.Value:
                    if (isCollection && !isCollectionObjectEntry) collection.InternalCollection.Add(item: new YAMLValue(key: id ?? IEntity.KEYLESS, value: tokens[index].Value));
                    else obj.Write<string>(route: [], id ?? IEntity.KEYLESS, tokens[index].Value);

                    id = null!;
                    ++index;
                    break;
            }
        }

        if (isCollection) {
            if (!obj.IsEmpty) {
                if(obj.Key == collection.Key) obj.Key = IEntity.KEYLESS;
                collection.InternalCollection.Add(item: obj.AsCopy());
            }

            ObjectPool<YAMLObject>.Shared.Return(@object: obj);
            YAMLCollection collectionResult = collection.AsCopy();

            ObjectPool<YAMLCollection>.Shared.Return(@object: collection);
            return collectionResult;
        }

        ObjectPool<YAMLCollection>.Shared.Return(@object: collection);
        YAMLObject result = obj.AsCopy();

        ObjectPool<YAMLObject>.Shared.Return(@object: obj);
        return result;
    }

    private static string CreateMultilineString(ReadOnlySpan<YamlToken> tokens, bool saveNewlines = false) {
        StringBuilder builder = new StringBuilder(capacity: 32);

        foreach (YamlToken token in tokens) {
            if (!saveNewlines && token.Type == YamlTokenType.NewLine) {
                builder.Append(value: ' ');
                continue;
            }

            builder.Append(value: token.Value);
        }

        return builder.ToString();
    }

    private static int FirstIndentationOf(int order, ReadOnlySpan<YamlToken> tokens) {
        for (int i = 0; i < tokens.Length; ++i) {
            if (tokens[i].Indentation == order)
                return i;
        }

        return -1;
    }

    private static int IndexOf<T, U>(U searchItem, Func<T, U> prop, ReadOnlySpan<T> collection) where U: IEquatable<U> {
        for (int i = 0; i < collection.Length; ++i) {
            if (prop(collection[i]).Equals(other: searchItem))
                return i;
        }

        return -1;
    }

    private static bool IsCollectionObjectEntry(ReadOnlySpan<YamlToken> tokens) {
        int vIndicator = IndexOf<YamlToken, char>(searchItem: '-', prop: static(x) => x.Value[0], tokens);
        if (vIndicator != -1) return vIndicator > 3;
        return tokens.Length > 5;
    }

    private static void RecursivelySerialize(IReadableEntity parent, StringBuilder builder, int indentation, bool isCollection) {
        if(parent is IEmptiable empty && empty.IsEmpty) {
            if(builder.Length > 2 && builder[^2] != '-')
                AppendIndentation(builder, indentation + 1 + (isCollection ? 1 : 0));

            builder.Append($"{parent.Key}: ~");
            return;
        }

        if (isCollection) builder.Append("- ");
        if (parent.Key != IEntity.KEYLESS) builder.Append($"{parent.Key}:\n");

        foreach (IEntity entity in parent) {
            if (entity is IReadableEntity obj) {
                if (builder.Length > 2 && builder[^2] != '-') AppendIndentation(builder, indentation + 1 + (isCollection ? 1 : 0));

                RecursivelySerialize(obj, builder, indentation + 1 + (isCollection ? 1 : 0), parent.Type == YAMLType.Array);
                continue;
            }

            if(builder.Length > 2 && builder[^2] != '-') AppendIndentation(builder, indentation + 1 + (isCollection ? 1 : 0));
            builder.Append($"{(parent.Type == YAMLType.Array ? "- " : "")}{entity}\n");
        }
    }

    private static void AppendIndentation(StringBuilder builder, int count) {
        if (count == 0) return;

        for (int i = 0; i < count; ++i)
            builder.Append(' ');
    }

    #endregion
}
