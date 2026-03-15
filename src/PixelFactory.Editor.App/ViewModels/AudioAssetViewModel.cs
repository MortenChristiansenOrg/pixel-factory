using CommunityToolkit.Mvvm.ComponentModel;

namespace PixelFactory.Editor.App.ViewModels;

public sealed partial class AudioAssetViewModel : AssetViewModelBase
{
    [ObservableProperty]
    private double _duration;

    [ObservableProperty]
    private string _format = "";
}
