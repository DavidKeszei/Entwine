# YAMLyzer ![C#](https://img.shields.io/badge/c%23-%23239120.svg?style=for-the-badge&logo=csharp&logoColor=white) ![.Net](https://img.shields.io/badge/.NET-5C2D91?style=for-the-badge&logo=.net&logoColor=white) ![Visual Studio](https://img.shields.io/badge/Visual%20Studio-5C2D91.svg?style=for-the-badge&logo=visual-studio&logoColor=white) ![YAML](https://img.shields.io/badge/yaml-%23ffffff.svg?style=for-the-badge&logo=yaml&logoColor=151515)
Simple and intuitive YAML serialization for C#/.NET. Get up and running quickly with clean, readable code (with minimal reflection). 

## Why I make this?
__Firstly: just for fun.__ I love to build things from scratch to test my knowledge about create mostly usable libs/programs like this without any external dependencies (exception from this, the core lib of the language). Second reason is doing this: I want to create a YAML serialization library, which using next topics:

- Create lexical tokens from string/stream.
- Try to interpreter this to mostly usable primitives & other data structures.

Last reason is I am very curious doing this without using the Reflection API of the C#. (Mostly this is succeeded, the __IS__ keyword the only reflection based API what I am used)

## Used Technology
- .NET/C#

## Requirements
- .NET9 or above

## Example Code
```cs
/* Test YAML string of the deserialization. */
string yaml =
    """
    # This is incorrect for a number. If you specify a custom default value, then you get that in this case!
    version: f11
    lastUpdate: 2025-07-14
    dependencies:

        # Implicit object declaration in a vertical collection for an item.
        - name: PowerToys
          publisher: Microsoft
          version: [0.92]
          desc: |
            Provides ultimate help in your .NET development pipeline!

        # Explicit object declaration in a vertical collection for an item.
        - package:
            name: Visual Studio Community Edition
            publisher: Microsoft
            versions: [latest, 17.14.6]
            desc: ~
    """;

/* Deserialize the YAML string to IReadableEntity instance. */
IReadableEntity @object = await YAMLSerializer.Deserialize(yaml);

/* Read from the root object. (Version is fail to -1, but the date is parsed correctly) */
int version = @object.Read<int>(route: ["version"], valueOnError: -1)!;
DateOnly date = @object.Read<DateOnly>(route: ["lastUpdate"], provider: DateTimeFormatInfo.CurrentInfo);

/* Read the first dependency name from the object. (Route: dependencies/0/name) */
string dependencyName = @object.Read<string>(route: ["dependencies", "0", "name"])!;
```
