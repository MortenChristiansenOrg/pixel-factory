namespace PixelFactory.Engine.Core.Rendering;

public interface IBuffer : IDisposable
{
    int SizeInBytes { get; }
}
