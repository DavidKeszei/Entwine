# YAMLyzer ![C#](https://img.shields.io/badge/c%23-%23239120.svg?style=for-the-badge&logo=csharp&logoColor=white) ![.Net](https://img.shields.io/badge/.NET-5C2D91?style=for-the-badge&logo=.net&logoColor=white) ![Visual Studio](https://img.shields.io/badge/Visual%20Studio-5C2D91.svg?style=for-the-badge&logo=visual-studio&logoColor=white) ![YAML](https://img.shields.io/badge/yaml-%23ffffff.svg?style=for-the-badge&logo=yaml&logoColor=151515)
Simple serialization library for YAML files & texts.

## Example Code
```cs
string example = "dir: \"C:\\very_big_dir\\\"";
YAMLObject obj = await YAMLSerializer.Deserialize(yaml: example);

Console.Out.WriteLine(value: $"Save part: {((YAMLValue)obj[keys: ["dir"]]).Serialize<string>()}");
```
