using PixelFactory.Common;

namespace PixelFactory.Editor.Core;

public sealed class AssetMetaFile
{
    public string Id { get; set; } = "";
    public string Type { get; set; } = "";
    public string Name { get; set; } = "";
    public string? SourcePath { get; set; }
    public string? DataFile { get; set; }
    public Dictionary<string, string> Properties { get; set; } = [];

    public AssetMeta ToAssetMeta() => new()
    {
        Id = new AssetId(Guid.ParseExact(Id, "N")),
        Type = Enum.Parse<AssetType>(Type),
        Name = Name,
        SourcePath = SourcePath,
    };

    public static AssetMetaFile FromAssetMeta(AssetMeta meta, string? dataFile = null) => new()
    {
        Id = meta.Id.ToString(),
        Type = meta.Type.ToString(),
        Name = meta.Name,
        SourcePath = meta.SourcePath,
        DataFile = dataFile,
    };
}
