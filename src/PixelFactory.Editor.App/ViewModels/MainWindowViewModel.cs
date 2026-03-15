using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using PixelFactory.Common;
using PixelFactory.Editor.Core;

namespace PixelFactory.Editor.App.ViewModels;

public sealed partial class MainWindowViewModel : ObservableObject
{
    private readonly ProjectService _service = new();
    private readonly EditorSettings _settings = EditorSettings.Load();
    private EditorProject? _project;

    [ObservableProperty]
    private string _title = "Pixel Factory";

    [ObservableProperty]
    private ObservableCollection<AssetTreeNode> _treeRoots = [];

    [ObservableProperty]
    private string _selectedAssetName = "";

    [ObservableProperty]
    private string _selectedAssetType = "";

    [ObservableProperty]
    private string _selectedAssetVertices = "";

    [ObservableProperty]
    private string _selectedAssetIndices = "";

    [ObservableProperty]
    private string _statusText = "Ready";

    public event Action<MeshData>? MeshLoaded;
    public event Action? MeshCleared;

    public bool HasProject => _project is not null;

    public MainWindowViewModel()
    {
        TryReopenLastProject();
    }

    private void TryReopenLastProject()
    {
        var path = _settings.LastProjectPath;
        if (path is not null && File.Exists(path))
        {
            _project = _service.LoadProject(path);
            RefreshTree();
        }
    }

    public void CreateProject(string name, string folder)
    {
        _project = _service.CreateProject(name, folder);
        PersistProjectPath();
        RefreshTree();
    }

    public void OpenProject(string path)
    {
        _project = _service.LoadProject(path);
        PersistProjectPath();
        RefreshTree();
    }

    public void AddAsset(AssetType type, string name)
    {
        if (_project is null) return;

        var meta = _service.AddAsset(_project, type, name, type == AssetType.Mesh ? "mesh.bin" : null);

        if (type == AssetType.Mesh)
        {
            var mesh = DungeonRoomGenerator.Generate();
            var meshPath = Path.Combine(_service.GetAssetDirectory(_project, meta.Id), "mesh.bin");
            using var stream = File.Create(meshPath);
            MeshSerializer.Serialize(stream, mesh);
        }

        RefreshTree();
        StatusText = $"Added {type}: {name}";
    }

    public AssetMeta? SelectedAsset { get; private set; }

    public void DeleteAsset(AssetMeta asset)
    {
        if (_project is null) return;
        _service.DeleteAsset(_project, asset.Id);
        ClearSelection();
        MeshCleared?.Invoke();
        SelectedAsset = null;
        RefreshTree();
        StatusText = $"Deleted: {asset.Name}";
    }

    public void RenameAsset(AssetMeta asset, string newName)
    {
        if (_project is null) return;
        _service.RenameAsset(_project, asset.Id, newName);

        // Update tree node in-place to preserve selection
        if (TreeRoots.Count > 0)
        {
            var node = TreeRoots[0].Children.FirstOrDefault(n => n.Asset?.Id == asset.Id);
            if (node is not null)
                node.Name = newName;
        }

        SelectedAssetName = newName;
        StatusText = $"Renamed → {newName}";
    }

    public void SelectAsset(AssetMeta? meta)
    {
        SelectedAsset = meta;

        if (meta is null)
        {
            ClearSelection();
            MeshCleared?.Invoke();
            return;
        }

        SelectedAssetName = meta.Name;
        SelectedAssetType = meta.Type.ToString();

        if (meta.Type == AssetType.Mesh && _project is not null)
        {
            var meshData = _service.LoadMeshData(_project, meta.Id);
            if (meshData is not null)
            {
                SelectedAssetVertices = meshData.Vertices.Length.ToString();
                SelectedAssetIndices = meshData.Indices.Length.ToString();
                MeshLoaded?.Invoke(meshData);
                return;
            }
        }

        SelectedAssetVertices = "";
        SelectedAssetIndices = "";
        MeshCleared?.Invoke();
    }

    private void ClearSelection()
    {
        SelectedAssetName = "";
        SelectedAssetType = "";
        SelectedAssetVertices = "";
        SelectedAssetIndices = "";
    }

    private void RefreshTree()
    {
        TreeRoots.Clear();
        if (_project is null) return;

        var (rootIcon, rootBrush) = AssetTreeNode.GetIconForType(null);
        var root = new AssetTreeNode { Name = _project.Name, Icon = rootIcon, IconBrush = rootBrush };
        foreach (var asset in _project.Assets)
        {
            var (icon, brush) = AssetTreeNode.GetIconForType(asset.Type);
            root.Children.Add(new AssetTreeNode { Name = asset.Name, Asset = asset, Icon = icon, IconBrush = brush });
        }

        TreeRoots.Add(root);
        Title = $"Pixel Factory — {_project.Name}";
        StatusText = $"{_project.Assets.Count} asset(s)";
    }

    private void PersistProjectPath()
    {
        if (_project is null) return;
        _settings.LastProjectPath = _project.ProjectFilePath;
        _settings.Save();
    }
}
