using System.Numerics;
using System.Runtime.InteropServices;
using PixelFactory.Editor.Core;
using PixelFactory.Engine.Core;
using PixelFactory.Engine.Core.Rendering;
using PixelFactory.Engine.Graphics.D3D11;

namespace PixelFactory.Sandbox;

public static class Program
{
    public static void Main(string[] args)
    {
        var service = new ProjectService();
        EditorProject? project = null;
        MeshData? meshData = null;

        // Try to load project from args
        if (args.Length > 0 && File.Exists(args[0]))
        {
            project = service.LoadProject(args[0]);
            var meshAsset = project.Assets.FirstOrDefault(a => a.Type == Common.AssetType.Mesh);
            if (meshAsset is not null)
                meshData = service.LoadMeshData(project, meshAsset.Id);
        }

        // Fallback: generate mesh in memory
        meshData ??= DungeonRoomGenerator.Generate();

        var window = new Win32Window(1280, 720, "Pixel Factory Sandbox");
        var device = new D3D11RenderDevice();
        device.Initialize(window.Handle, window.Width, window.Height);

        window.OnResize = (w, h) => device.Resize(w, h);

        // Compile shaders and create pipeline
        var (vs, ps) = D3D11RenderDevice.CompileEmbeddedShaders();
        var pipeline = device.CreatePipeline(vs, ps);

        // Create GPU buffers from mesh data
        var vertexData = MemoryMarshal.AsBytes(meshData.Vertices.AsSpan());
        var vertexBuffer = device.CreateBuffer(
            new BufferDescription(vertexData.Length, BufferUsage.Vertex), vertexData);

        var indexData = MemoryMarshal.AsBytes(meshData.Indices.AsSpan());
        var indexBuffer = device.CreateBuffer(
            new BufferDescription(indexData.Length, BufferUsage.Index), indexData);

        var constantBuffer = device.CreateBuffer(
            new BufferDescription(64, BufferUsage.Constant), ReadOnlySpan<byte>.Empty);

        var camera = new IsometricCamera(8f, 1280f / 720f);

        var loop = new GameLoop(device);

        loop.ProcessMessages = () =>
        {
            window.ProcessMessages();
            if (!window.IsRunning) loop.Stop();
        };

        loop.OnRender = _ =>
        {
            var view = camera.ViewMatrix;
            var proj = camera.ProjectionMatrix;
            var wvp = Matrix4x4.Transpose(view * proj);
            var wvpBytes = MemoryMarshal.AsBytes(MemoryMarshal.CreateReadOnlySpan(ref wvp, 1));
            device.UpdateBuffer(constantBuffer, wvpBytes);

            pipeline.Bind();
            device.SetConstantBuffer(0, constantBuffer);
            device.SetVertexBuffer(vertexBuffer, VertexPositionColor.SizeInBytes);
            device.SetIndexBuffer(indexBuffer);
            device.DrawIndexed(meshData.Indices.Length, 0, 0);
        };

        loop.Run();

        constantBuffer.Dispose();
        indexBuffer.Dispose();
        vertexBuffer.Dispose();
        pipeline.Dispose();
        loop.Dispose();
    }
}
