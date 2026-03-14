using PixelFactory.Engine.Core.Rendering;

namespace PixelFactory.Engine.Graphics.D3D11;

public sealed class D3D11RenderDevice : IRenderDevice
{
    public string BackendName => "Direct3D 11";

    public void Initialize(nint windowHandle, int width, int height)
    {
        // TODO: create device, swap chain, render target
    }

    public void Resize(int width, int height)
    {
        // TODO: resize swap chain buffers
    }

    public void BeginFrame()
    {
        // TODO: clear render target
    }

    public void EndFrame()
    {
        // TODO: finalize frame
    }

    public void Present()
    {
        // TODO: swap chain present
    }

    public void Dispose()
    {
        // TODO: release COM objects
    }
}
