# YAMLyzer ![C#](https://img.shields.io/badge/c%23-%23239120.svg?style=for-the-badge&logo=csharp&logoColor=white) ![.Net](https://img.shields.io/badge/.NET-5C2D91?style=for-the-badge&logo=.net&logoColor=white) ![Visual Studio](https://img.shields.io/badge/Visual%20Studio-5C2D91.svg?style=for-the-badge&logo=visual-studio&logoColor=white) ![YAML](https://img.shields.io/badge/yaml-%23ffffff.svg?style=for-the-badge&logo=yaml&logoColor=151515)
Simple and intuitive YAML serialization for C#/.NET. Get up and running quickly with clean, readable code (with minimal reflection). 

## Example Code
```cs
        string yaml =
            """
            dependencies:

                # Implicit object declaration in a vertical collection for an item.
                - name: PowerToys
                  publisher: Microsoft
                  version: [0.92]
                  desc: >
                    Provides ultimate help in your .NET development pipeline!
                    See more: ...

                # Explicit object declaration in a vertical collection for an item.
                - package:
                    name: Visual Studio Community Edition
                    publisher: Microsoft
                    versions: [latest, 17.14.6]
                    desc: ~
            """;

        IReadableYAMLEntity @object = await YAMLSerializer.Deserialize(yaml);
        string packageName = @object.Read<string>(route: [ "dependencies", "0", "desc" ])!;
```
