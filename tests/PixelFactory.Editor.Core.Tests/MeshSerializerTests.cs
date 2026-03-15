using System.Numerics;
using PixelFactory.Engine.Core.Rendering;
using Xunit;

namespace PixelFactory.Editor.Core.Tests;

public class MeshSerializerTests
{
    [Fact]
    public void RoundTrip_PreservesData()
    {
        var original = new MeshData
        {
            Vertices =
            [
                new VertexPositionColor(new Vector3(0, 0, 0), new Vector4(1, 0, 0, 1)),
                new VertexPositionColor(new Vector3(1, 0, 0), new Vector4(0, 1, 0, 1)),
                new VertexPositionColor(new Vector3(0, 1, 0), new Vector4(0, 0, 1, 1)),
            ],
            Indices = [0, 1, 2],
        };

        using var stream = new MemoryStream();
        MeshSerializer.Serialize(stream, original);
        stream.Position = 0;
        var deserialized = MeshSerializer.Deserialize(stream);

        Assert.Equal(original.Vertices.Length, deserialized.Vertices.Length);
        Assert.Equal(original.Indices.Length, deserialized.Indices.Length);

        for (var i = 0; i < original.Vertices.Length; i++)
        {
            Assert.Equal(original.Vertices[i].Position, deserialized.Vertices[i].Position);
            Assert.Equal(original.Vertices[i].Color, deserialized.Vertices[i].Color);
        }

        for (var i = 0; i < original.Indices.Length; i++)
            Assert.Equal(original.Indices[i], deserialized.Indices[i]);
    }
}
