using PixelFactory.Engine.Core.Rendering;

namespace PixelFactory.Editor.Core;

public sealed class MeshData
{
    public required VertexPositionColor[] Vertices { get; init; }
    public required ushort[] Indices { get; init; }
}
