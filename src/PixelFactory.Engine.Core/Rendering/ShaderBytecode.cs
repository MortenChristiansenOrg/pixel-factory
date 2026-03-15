namespace PixelFactory.Engine.Core.Rendering;

public sealed class ShaderBytecode
{
    private readonly byte[] _data;

    public ShaderBytecode(byte[] data)
    {
        _data = data;
    }

    public ReadOnlySpan<byte> Data => _data;
}
