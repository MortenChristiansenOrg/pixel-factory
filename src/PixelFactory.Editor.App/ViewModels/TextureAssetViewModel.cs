using CommunityToolkit.Mvvm.ComponentModel;

namespace PixelFactory.Editor.App.ViewModels;

public sealed partial class TextureAssetViewModel : AssetViewModelBase
{
    [ObservableProperty]
    private int _width;

    [ObservableProperty]
    private int _height;

    [ObservableProperty]
    private string _format = "";
}
