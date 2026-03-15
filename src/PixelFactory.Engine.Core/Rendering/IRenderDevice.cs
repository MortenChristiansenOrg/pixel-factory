namespace PixelFactory.Engine.Core.Rendering;

public interface IRenderDevice : IDisposable
{
    string BackendName { get; }

    void Initialize(nint windowHandle, int width, int height);
    void Resize(int width, int height);
    void BeginFrame();
    void EndFrame();
    void Present();

    IBuffer CreateBuffer(BufferDescription desc, ReadOnlySpan<byte> initialData);
    IPipeline CreatePipeline(ShaderBytecode vertexShader, ShaderBytecode pixelShader);
    void SetVertexBuffer(IBuffer buffer, int stride);
    void SetIndexBuffer(IBuffer buffer);
    void SetConstantBuffer(int slot, IBuffer buffer);
    void UpdateBuffer(IBuffer buffer, ReadOnlySpan<byte> data);
    void DrawIndexed(int indexCount, int startIndex, int baseVertex);
    void SetViewport(int x, int y, int width, int height);
    byte[] CaptureBackBuffer();
}
