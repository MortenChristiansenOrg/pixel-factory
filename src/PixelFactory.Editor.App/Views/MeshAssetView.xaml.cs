using System.ComponentModel;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using PixelFactory.Editor.App.ViewModels;
using PixelFactory.Editor.Core;
using PixelFactory.Engine.Core.Rendering;
using PixelFactory.Engine.Graphics.D3D11;

namespace PixelFactory.Editor.App.Views;

public partial class MeshAssetView : UserControl
{
    private D3D11ViewportHost? _viewportHost;
    private IPipeline? _pipeline;
    private IBuffer? _vertexBuffer;
    private IBuffer? _indexBuffer;
    private IBuffer? _constantBuffer;
    private int _indexCount;
    private IsometricCamera? _camera;
    private Point _lastMousePos;
    private bool _isRotating;

    public MeshAssetView()
    {
        InitializeComponent();
        Loaded += OnLoaded;
        Unloaded += OnUnloaded;
        DataContextChanged += OnDataContextChanged;
    }

    // --- MVVM: watch ViewModel for MeshData changes ---

    private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        if (e.OldValue is MeshAssetViewModel oldVm)
            oldVm.PropertyChanged -= OnViewModelPropertyChanged;

        if (e.NewValue is MeshAssetViewModel newVm)
        {
            newVm.PropertyChanged += OnViewModelPropertyChanged;
            if (newVm.MeshData is not null)
                LoadMesh(newVm.MeshData);
        }
    }

    private void OnViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(MeshAssetViewModel.MeshData) && sender is MeshAssetViewModel vm)
        {
            if (vm.MeshData is not null)
                LoadMesh(vm.MeshData);
        }
    }

    // --- Viewport lifecycle ---

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        if (_viewportHost is not null) return;

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

        // If mesh data was set before the device was ready, load it now
        if (_pendingMesh is not null)
        {
            LoadMeshToViewport(_pendingMesh);
            _pendingMesh = null;
        }
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

    // --- Mesh loading ---

    private MeshData? _pendingMesh;

    private void LoadMesh(MeshData meshData)
    {
        if (_viewportHost?.Device is null)
        {
            _pendingMesh = meshData;
            return;
        }
        LoadMeshToViewport(meshData);
    }

    private void LoadMeshToViewport(MeshData meshData)
    {
        var device = _viewportHost?.Device;
        if (device is null) return;

        _vertexBuffer?.Dispose();
        _indexBuffer?.Dispose();

        if (meshData.Vertices.Length == 0 || meshData.Indices.Length == 0)
        {
            _vertexBuffer = null;
            _indexBuffer = null;
            _indexCount = 0;
            return;
        }

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

    // --- Cleanup ---

    private void OnUnloaded(object sender, RoutedEventArgs e)
    {
        if (DataContext is MeshAssetViewModel vm)
            vm.PropertyChanged -= OnViewModelPropertyChanged;

        CompositionTarget.Rendering -= OnCompositionRendering;
        _vertexBuffer?.Dispose();
        _indexBuffer?.Dispose();
        _constantBuffer?.Dispose();
        _pipeline?.Dispose();
        _vertexBuffer = null;
        _indexBuffer = null;
        _constantBuffer = null;
        _pipeline = null;
        _viewportHost?.Dispose();
        _viewportHost = null;
    }
}
