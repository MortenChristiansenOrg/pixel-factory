using CommunityToolkit.Mvvm.ComponentModel;

namespace PixelFactory.Editor.App.ViewModels;

public sealed partial class SceneAssetViewModel : AssetViewModelBase
{
    [ObservableProperty]
    private int _entityCount;
}
