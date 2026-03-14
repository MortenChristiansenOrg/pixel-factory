using PixelFactory.Common;

namespace PixelFactory.Editor.Core;

public interface IEditorProject
{
    string Name { get; }
    string RootPath { get; }
    IReadOnlyList<AssetMeta> Assets { get; }
}
