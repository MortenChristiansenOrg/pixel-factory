namespace PixelFactory.Engine.Core.Rendering;

public interface IRenderDevice : IDisposable
{
    string BackendName { get; }

    void Initialize(nint windowHandle, int width, int height);
    void Resize(int width, int height);
    void BeginFrame();
    void EndFrame();
    void Present();
}
