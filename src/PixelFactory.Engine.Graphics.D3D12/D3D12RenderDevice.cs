using PixelFactory.Engine.Core.Rendering;

namespace PixelFactory.Engine.Graphics.D3D12;

public sealed class D3D12RenderDevice : IRenderDevice
{
    public string BackendName => "Direct3D 12";

    public void Initialize(nint windowHandle, int width, int height)
    {
        // TODO: create device, command queue, swap chain
    }

    public void Resize(int width, int height)
    {
        // TODO: resize swap chain buffers
    }

    public void BeginFrame()
    {
        // TODO: reset command allocator, begin command list
    }

    public void EndFrame()
    {
        // TODO: close and execute command list
    }

    public void Present()
    {
        // TODO: swap chain present, fence sync
    }

    public IBuffer CreateBuffer(BufferDescription desc, ReadOnlySpan<byte> initialData) =>
        throw new NotImplementedException();

    public IPipeline CreatePipeline(ShaderBytecode vertexShader, ShaderBytecode pixelShader) =>
        throw new NotImplementedException();

    public void SetVertexBuffer(IBuffer buffer, int stride) =>
        throw new NotImplementedException();

    public void SetIndexBuffer(IBuffer buffer) =>
        throw new NotImplementedException();

    public void SetConstantBuffer(int slot, IBuffer buffer) =>
        throw new NotImplementedException();

    public void UpdateBuffer(IBuffer buffer, ReadOnlySpan<byte> data) =>
        throw new NotImplementedException();

    public void DrawIndexed(int indexCount, int startIndex, int baseVertex) =>
        throw new NotImplementedException();

    public void SetViewport(int x, int y, int width, int height) =>
        throw new NotImplementedException();

    public byte[] CaptureBackBuffer() =>
        throw new NotImplementedException();

    public void Dispose()
    {
        // TODO: release COM objects
    }
}
