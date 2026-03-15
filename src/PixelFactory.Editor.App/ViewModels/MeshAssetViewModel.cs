using CommunityToolkit.Mvvm.ComponentModel;
using PixelFactory.Editor.Core;

namespace PixelFactory.Editor.App.ViewModels;

public sealed partial class MeshAssetViewModel : AssetViewModelBase
{
    [ObservableProperty]
    private MeshData? _meshData;

    [ObservableProperty]
    private int _vertexCount;

    [ObservableProperty]
    private int _indexCount;

    public void Load(MeshData data)
    {
        MeshData = data;
        VertexCount = data.Vertices.Length;
        IndexCount = data.Indices.Length;
    }
}
