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

    public void Dispose()
    {
        // TODO: release COM objects
    }
}
