using Xunit;

namespace PixelFactory.Common.Tests;

public class AssetMetaTests
{
    [Fact]
    public void RequiredProperties_AreSet()
    {
        var id = AssetId.New();
        var meta = new AssetMeta
        {
            Id = id,
            Type = AssetType.Mesh,
            Name = "TestMesh",
        };

        Assert.Equal(id, meta.Id);
        Assert.Equal(AssetType.Mesh, meta.Type);
        Assert.Equal("TestMesh", meta.Name);
        Assert.Null(meta.SourcePath);
    }
}
