using CommunityToolkit.Mvvm.ComponentModel;

namespace PixelFactory.Editor.App.ViewModels;

public sealed partial class MaterialAssetViewModel : AssetViewModelBase
{
    [ObservableProperty]
    private string? _shaderName;
}
