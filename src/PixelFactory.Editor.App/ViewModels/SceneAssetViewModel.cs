using CommunityToolkit.Mvvm.ComponentModel;

namespace PixelFactory.Editor.App.ViewModels;

public sealed partial class SceneAssetViewModel : AssetViewModelBase
{
    [ObservableProperty]
    private int _entityCount;

    public string EntityCountLabel => EntityCount == 1 ? "1 entity" : $"{EntityCount} entities";

    partial void OnEntityCountChanged(int value) => OnPropertyChanged(nameof(EntityCountLabel));
}
