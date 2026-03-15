using System.Text.Json.Serialization;

namespace PixelFactory.Editor.Core;

[JsonSerializable(typeof(ProjectFile))]
[JsonSerializable(typeof(AssetMetaFile))]
[JsonSourceGenerationOptions(WriteIndented = true, PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
public partial class JsonContext : JsonSerializerContext;
