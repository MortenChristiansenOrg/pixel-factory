using PixelFactory.Common;

namespace PixelFactory.Editor.Core;

public sealed class EditorProject
{
    private readonly List<AssetMeta> _assets = [];

    public required string Name { get; init; }
    public required string RootPath { get; init; }
    public string ProjectFilePath { get; init; } = "";
    public IReadOnlyList<AssetMeta> Assets => _assets;

    internal void AddAsset(AssetMeta asset) => _assets.Add(asset);
    internal void RemoveAsset(AssetId id) => _assets.RemoveAll(a => a.Id == id);
}
