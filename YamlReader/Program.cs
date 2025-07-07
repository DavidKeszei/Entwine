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
        string example =
        """
        texts: 
            - Something
            - 'In'
            - "My"
            - ass: Ass
        """;

        long memory = GC.GetTotalMemory(false);
        Console.Out.WriteLine(value: $"Currently used memory: {(memory / 1_000_000f):f2}MB");

        YAMLObject obj = await YAMLSerializer.Deserialize(yaml: example);

        Console.Out.WriteLine(value: $"Currently used memory: {((GC.GetTotalMemory(false) - memory) / 1_000_000f):f2}MB");
        Console.ReadKey();
    }
}
