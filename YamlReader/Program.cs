using System.Collections.ObjectModel;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization.Metadata;
using YAMLyzer;

namespace YamlReader;

internal class Program
{
    static async Task Main(string[] args) {
        string example = """
         descriptions:
                - desc: |
                    This is: -not allowed by YAML lexer.
                    This is: --not allowed by YAML lexer. ###
                - desc: >
                    This is not allowed by YAML lexer.
                    This is not allowed by YAML lexer.
        badges:
            ok:
                - 200
                - The request is successful.
            not found:
                - 404
                - The resource is not found.
        """;

        YAMLObject obj = await YAMLSerializer.Deserialize(yaml: example);

        Console.Out.WriteLine(value: $"\n{((YAMLValue)((YAMLCollection)obj[keys: ["descriptions"]])[0]).Serialize<string>()}");
        Console.ReadKey();
    }
}
