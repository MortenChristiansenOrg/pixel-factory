using PixelFactory.Common;

namespace PixelFactory.Editor.Core;

public sealed class EditorProject : IEditorProject
{
    private readonly List<AssetMeta> _assets = [];

    public required string Name { get; init; }
    public required string RootPath { get; init; }
    public IReadOnlyList<AssetMeta> Assets => _assets;
}
