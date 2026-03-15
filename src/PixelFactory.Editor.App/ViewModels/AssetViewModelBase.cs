using CommunityToolkit.Mvvm.ComponentModel;

namespace PixelFactory.Editor.App.ViewModels;

public abstract partial class AssetViewModelBase : ObservableObject
{
    [ObservableProperty]
    private string _assetName = "";
}
