using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;
using PixelFactory.Editor.App.ViewModels;
using PixelFactory.Editor.App.Views;

namespace PixelFactory.Editor.App;

public partial class MainWindow : Window
{
    private readonly MainWindowViewModel _vm = new();

    // Cached views (avoid re-creating expensive controls)
    private MeshAssetView? _meshView;
    private TextureAssetView? _textureView;
    private MaterialAssetView? _materialView;
    private ShaderAssetView? _shaderView;
    private SceneAssetView? _sceneView;
    private AudioAssetView? _audioView;
    private ScriptAssetView? _scriptView;

    public MainWindow()
    {
        InitializeComponent();
        DataContext = _vm;
        _vm.PropertyChanged += OnViewModelPropertyChanged;
    }

    // --- Asset view switching via ViewModel ---

    private void OnViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(MainWindowViewModel.CurrentAssetViewModel))
            ShowCurrentAssetView();
    }

    private void ShowCurrentAssetView()
    {
        var assetVm = _vm.CurrentAssetViewModel;

        if (assetVm is null)
        {
            AssetViewHost.Content = null;
            ViewportHeader.Text = "Viewport";
            return;
        }

        switch (assetVm)
        {
            case MeshAssetViewModel meshVm:
                _meshView ??= new MeshAssetView();
                _meshView.DataContext = meshVm;
                AssetViewHost.Content = _meshView;
                ViewportHeader.Text = "Mesh Viewer";
                break;
            case TextureAssetViewModel:
                _textureView ??= new TextureAssetView();
                _textureView.DataContext = assetVm;
                AssetViewHost.Content = _textureView;
                ViewportHeader.Text = "Texture Viewer";
                break;
            case MaterialAssetViewModel:
                _materialView ??= new MaterialAssetView();
                _materialView.DataContext = assetVm;
                AssetViewHost.Content = _materialView;
                ViewportHeader.Text = "Material Editor";
                break;
            case ShaderAssetViewModel:
                _shaderView ??= new ShaderAssetView();
                _shaderView.DataContext = assetVm;
                AssetViewHost.Content = _shaderView;
                ViewportHeader.Text = "Shader Editor";
                break;
            case SceneAssetViewModel:
                _sceneView ??= new SceneAssetView();
                _sceneView.DataContext = assetVm;
                AssetViewHost.Content = _sceneView;
                ViewportHeader.Text = "Scene Viewer";
                break;
            case AudioAssetViewModel:
                _audioView ??= new AudioAssetView();
                _audioView.DataContext = assetVm;
                AssetViewHost.Content = _audioView;
                ViewportHeader.Text = "Audio Player";
                break;
            case ScriptAssetViewModel:
                _scriptView ??= new ScriptAssetView();
                _scriptView.DataContext = assetVm;
                AssetViewHost.Content = _scriptView;
                ViewportHeader.Text = "Script Editor";
                break;
            default:
                AssetViewHost.Content = null;
                ViewportHeader.Text = "Viewport";
                break;
        }
    }

    // --- Menu commands ---

    private void NewProject_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new OpenFolderDialog { Title = "Select folder for new project" };
        if (dialog.ShowDialog() != true) return;

        var folder = dialog.FolderName;
        var name = System.IO.Path.GetFileName(folder) ?? "NewProject";
        _vm.CreateProject(name, folder);
    }

    private void OpenProject_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new OpenFileDialog
        {
            Filter = "Pixel Factory Project|*.pixproj",
            Title = "Open Project",
        };
        if (dialog.ShowDialog() != true) return;
        _vm.OpenProject(dialog.FileName);
    }

    private void Exit_Click(object sender, RoutedEventArgs e) => Close();

    private void AddAsset_Click(object sender, RoutedEventArgs e)
    {
        if (!_vm.HasProject)
        {
            MessageBox.Show("Open or create a project first.", "No Project", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        var dialog = new AddAssetDialog { Owner = this };
        if (dialog.ShowDialog() != true) return;
        _vm.AddAsset(dialog.AssetType, dialog.AssetName);
    }

    private void DeleteAsset_Click(object sender, RoutedEventArgs e)
    {
        if (_vm.SelectedAsset is not { } asset) return;
        var result = MessageBox.Show($"Delete asset '{asset.Name}'?", "Delete Asset",
            MessageBoxButton.YesNo, MessageBoxImage.Warning);
        if (result != MessageBoxResult.Yes) return;
        _vm.DeleteAsset(asset);
    }

    private void RenameAsset_Click(object sender, RoutedEventArgs e)
    {
        if (_vm.SelectedAsset is not { } asset) return;
        var dialog = new RenameDialog(asset.Name) { Owner = this };
        if (dialog.ShowDialog() != true || dialog.AssetName == asset.Name) return;
        _vm.RenameAsset(asset, dialog.AssetName);
    }

    // --- Per-node context menus ---

    private void TreeViewItem_Loaded(object sender, RoutedEventArgs e)
    {
        if (sender is not TreeViewItem item || item.DataContext is not AssetTreeNode node) return;

        var menu = new ContextMenu();
        menu.Opened += (_, _) => item.IsSelected = true;
        menu.Items.Add(new MenuItem { Header = "Add Asset..." });
        ((MenuItem)menu.Items[0]!).Click += AddAsset_Click;

        if (!node.IsProject)
        {
            menu.Items.Add(new Separator());
            var rename = new MenuItem { Header = "Rename..." };
            rename.Click += RenameAsset_Click;
            menu.Items.Add(rename);
            var delete = new MenuItem { Header = "Delete" };
            delete.Click += DeleteAsset_Click;
            menu.Items.Add(delete);
        }

        item.ContextMenu = menu;
    }

    // --- TreeView selection → ViewModel ---

    private void AssetTree_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
    {
        if (e.NewValue is AssetTreeNode { Asset: { } asset })
            _vm.SelectAsset(asset);
        else
            _vm.SelectAsset(null);
    }

    // --- Cleanup ---

    protected override void OnClosed(EventArgs e)
    {
        _vm.PropertyChanged -= OnViewModelPropertyChanged;
        base.OnClosed(e);
    }
}
