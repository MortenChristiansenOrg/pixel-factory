using System.Runtime.InteropServices;
using PixelFactory.Engine.Core.Rendering;

namespace PixelFactory.Editor.Core;

public static class MeshSerializer
{
    private const uint Magic = 0x4D455348; // "MESH"
    private const uint Version = 1;

    public static void Serialize(Stream stream, MeshData mesh)
    {
        using var writer = new BinaryWriter(stream, System.Text.Encoding.UTF8, leaveOpen: true);
        writer.Write(Magic);
        writer.Write(Version);
        writer.Write(mesh.Vertices.Length);
        writer.Write(mesh.Indices.Length);

        var vertexBytes = MemoryMarshal.AsBytes(mesh.Vertices.AsSpan());
        writer.Write(vertexBytes);

        var indexBytes = MemoryMarshal.AsBytes(mesh.Indices.AsSpan());
        writer.Write(indexBytes);
    }

    public static MeshData Deserialize(Stream stream)
    {
        using var reader = new BinaryReader(stream, System.Text.Encoding.UTF8, leaveOpen: true);
        var magic = reader.ReadUInt32();
        if (magic != Magic) throw new InvalidDataException("Not a valid mesh file");

        var version = reader.ReadUInt32();
        if (version != Version) throw new InvalidDataException($"Unsupported mesh version: {version}");

        var vertexCount = reader.ReadInt32();
        var indexCount = reader.ReadInt32();

        var vertices = new VertexPositionColor[vertexCount];
        var vertexByteCount = vertexCount * Marshal.SizeOf<VertexPositionColor>();
        var vertexBytes = reader.ReadBytes(vertexByteCount);
        MemoryMarshal.Cast<byte, VertexPositionColor>(vertexBytes).CopyTo(vertices);

        var indices = new ushort[indexCount];
        var indexByteCount = indexCount * sizeof(ushort);
        var indexBytes = reader.ReadBytes(indexByteCount);
        MemoryMarshal.Cast<byte, ushort>(indexBytes).CopyTo(indices);

        return new MeshData { Vertices = vertices, Indices = indices };
    }
}
