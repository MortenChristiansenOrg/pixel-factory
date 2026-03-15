using PixelFactory.Engine.Core.Rendering;
using Vortice.Direct3D11;

namespace PixelFactory.Engine.Graphics.D3D11;

internal sealed class D3D11Buffer(ID3D11Buffer nativeBuffer, int sizeInBytes) : IBuffer
{
    public ID3D11Buffer NativeBuffer { get; } = nativeBuffer;
    public int SizeInBytes { get; } = sizeInBytes;

    public void Dispose() => NativeBuffer.Dispose();
}
