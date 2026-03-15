namespace PixelFactory.Common;

/// <summary>
/// JSON-serializable metadata for an asset.
/// </summary>
public sealed class AssetMeta
{
    public required AssetId Id { get; init; }
    public required AssetType Type { get; init; }
    public required string Name { get; set; }
    public string? SourcePath { get; init; }
}
