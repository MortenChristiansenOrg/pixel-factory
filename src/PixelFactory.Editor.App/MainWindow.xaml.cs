using System.Numerics;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using Microsoft.Win32;
using PixelFactory.Editor.App.ViewModels;
using PixelFactory.Editor.Core;
using PixelFactory.Engine.Core.Rendering;
using PixelFactory.Engine.Graphics.D3D11;

namespace PixelFactory.Editor.App;

public partial class MainWindow : Window
{
    private readonly MainWindowViewModel _vm = new();

    // Viewport rendering state (inherently view-layer, stays in code-behind)
    private D3D11ViewportHost? _viewportHost;
    private IPipeline? _pipeline;
    private IBuffer? _vertexBuffer;
    private IBuffer? _indexBuffer;
    private IBuffer? _constantBuffer;
    private int _indexCount;
    private IsometricCamera? _camera;
    private Point _lastMousePos;
    private bool _isRotating;

    public MainWindow()
    {
        InitializeComponent();
        DataContext = _vm;
        _vm.MeshLoaded += LoadMeshToViewport;
        _vm.MeshCleared += ClearMeshFromViewport;
        Loaded += OnLoaded;
    }

    // --- Viewport lifecycle ---

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        _viewportHost = new D3D11ViewportHost();
        _viewportHost.OnDeviceReady = OnDeviceReady;
        _viewportHost.MouseWheelCallback = delta =>
        {
            if (_camera is null) return;
            _camera.Size = Math.Max(1f, _camera.Size * (delta > 0 ? 0.9f : 1.1f));
        };
        ViewportBorder.Child = _viewportHost;
    }

    private void OnDeviceReady()
    {
        var device = _viewportHost!.Device!;
        var (vs, ps) = D3D11RenderDevice.CompileEmbeddedShaders();
        _pipeline = device.CreatePipeline(vs, ps);
        _constantBuffer = device.CreateBuffer(
            new BufferDescription(64, BufferUsage.Constant), ReadOnlySpan<byte>.Empty);

        var w = (int)ViewportOverlay.ActualWidth;
        var h = (int)ViewportOverlay.ActualHeight;
        if (w > 0 && h > 0)
            _viewportHost.ResizeViewport(w, h);

        _camera = new IsometricCamera(8f, Math.Max(1f, (float)w / h));
        CompositionTarget.Rendering += OnCompositionRendering;
    }

    private void OnCompositionRendering(object? sender, EventArgs e)
    {
        var device = _viewportHost?.Device;
        if (device is null || _pipeline is null || _camera is null) return;

        device.BeginFrame();

        if (_vertexBuffer is not null && _indexBuffer is not null && _constantBuffer is not null && _indexCount > 0)
        {
            var view = _camera.ViewMatrix;
            var proj = _camera.ProjectionMatrix;
            var wvp = Matrix4x4.Transpose(view * proj);
            var wvpBytes = MemoryMarshal.AsBytes(MemoryMarshal.CreateReadOnlySpan(ref wvp, 1));
            device.UpdateBuffer(_constantBuffer, wvpBytes);

            _pipeline.Bind();
            device.SetConstantBuffer(0, _constantBuffer);
            device.SetVertexBuffer(_vertexBuffer, VertexPositionColor.SizeInBytes);
            device.SetIndexBuffer(_indexBuffer);
            device.DrawIndexed(_indexCount, 0, 0);
        }

        device.EndFrame();
        device.Present();
    }

    // --- Mesh loading (driven by ViewModel events) ---

    private void LoadMeshToViewport(MeshData meshData)
    {
        var device = _viewportHost?.Device;
        if (device is null) return;

        _vertexBuffer?.Dispose();
        _indexBuffer?.Dispose();

        var vertexData = MemoryMarshal.AsBytes(meshData.Vertices.AsSpan());
        _vertexBuffer = device.CreateBuffer(
            new BufferDescription(vertexData.Length, BufferUsage.Vertex), vertexData);

        var indexData = MemoryMarshal.AsBytes(meshData.Indices.AsSpan());
        _indexBuffer = device.CreateBuffer(
            new BufferDescription(indexData.Length, BufferUsage.Index), indexData);

        _indexCount = meshData.Indices.Length;

        if (_camera is not null && meshData.Vertices.Length > 0)
        {
            var min = meshData.Vertices[0].Position;
            var max = min;
            foreach (var v in meshData.Vertices)
            {
                min = Vector3.Min(min, v.Position);
                max = Vector3.Max(max, v.Position);
            }
            _camera.Target = (min + max) * 0.5f;
            var extent = max - min;
            _camera.Size = MathF.Max(extent.X, MathF.Max(extent.Y, extent.Z)) * 1.5f;
        }
    }

    private void ClearMeshFromViewport()
    {
        _vertexBuffer?.Dispose();
        _indexBuffer?.Dispose();
        _vertexBuffer = null;
        _indexBuffer = null;
        _indexCount = 0;
    }

    // --- Camera mouse interaction ---

    private void Viewport_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        _isRotating = true;
        _lastMousePos = e.GetPosition((IInputElement)sender);
        ((UIElement)sender).CaptureMouse();
    }

    private void Viewport_MouseMove(object sender, MouseEventArgs e)
    {
        if (!_isRotating || _camera is null) return;
        var pos = e.GetPosition((IInputElement)sender);
        var dx = (float)(pos.X - _lastMousePos.X);
        var dy = (float)(pos.Y - _lastMousePos.Y);
        _lastMousePos = pos;

        _camera.Yaw -= dx * 0.005f;
        _camera.Pitch = Math.Clamp(_camera.Pitch + dy * 0.005f, -MathF.PI / 2 + 0.01f, MathF.PI / 2 - 0.01f);
    }

    private void Viewport_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
        _isRotating = false;
        ((UIElement)sender).ReleaseMouseCapture();
    }

    private void Viewport_SizeChanged(object sender, SizeChangedEventArgs e)
    {
        var w = (int)e.NewSize.Width;
        var h = (int)e.NewSize.Height;
        if (w <= 0 || h <= 0) return;
        _viewportHost?.ResizeViewport(w, h);
        if (_camera is not null)
            _camera.AspectRatio = (float)w / h;
    }

    // --- Menu commands (dialogs are inherently view-layer) ---

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
            MessageBox.Show("Open or create a project first.", "No Project",
                MessageBoxButton.OK, MessageBoxImage.Warning);
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
        CompositionTarget.Rendering -= OnCompositionRendering;
        _vm.MeshLoaded -= LoadMeshToViewport;
        _vm.MeshCleared -= ClearMeshFromViewport;
        _vertexBuffer?.Dispose();
        _indexBuffer?.Dispose();
        _constantBuffer?.Dispose();
        _pipeline?.Dispose();
        base.OnClosed(e);
    }
}
