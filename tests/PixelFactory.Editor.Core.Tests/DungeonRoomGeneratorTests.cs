using Xunit;

namespace PixelFactory.Editor.Core.Tests;

public class DungeonRoomGeneratorTests
{
    [Fact]
    public void Generate_ProducesValidMesh()
    {
        var mesh = DungeonRoomGenerator.Generate();

        Assert.NotEmpty(mesh.Vertices);
        Assert.NotEmpty(mesh.Indices);
    }

    [Fact]
    public void Generate_IndicesInRange()
    {
        var mesh = DungeonRoomGenerator.Generate();

        foreach (var index in mesh.Indices)
            Assert.True(index < mesh.Vertices.Length, $"Index {index} out of range (vertices: {mesh.Vertices.Length})");
    }

    [Fact]
    public void Generate_HasFourQuads()
    {
        // Floor + 3 walls = 4 quads = 16 vertices, 24 indices
        var mesh = DungeonRoomGenerator.Generate();

        Assert.Equal(16, mesh.Vertices.Length);
        Assert.Equal(24, mesh.Indices.Length);
    }
}
