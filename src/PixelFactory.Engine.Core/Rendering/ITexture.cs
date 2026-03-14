namespace PixelFactory.Engine.Core.Rendering;

public interface ITexture : IDisposable
{
    int Width { get; }
    int Height { get; }
}
