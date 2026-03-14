namespace PixelFactory.Engine.Core.Rendering;

public interface ISwapChain : IDisposable
{
    void Resize(int width, int height);
    void Present(int syncInterval = 1);
}
