using CommunityToolkit.Mvvm.ComponentModel;

namespace PixelFactory.Editor.App.ViewModels;

public sealed partial class ScriptAssetViewModel : AssetViewModelBase
{
    [ObservableProperty]
    private string _sourceCode = "";
}
