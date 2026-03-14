using PixelFactory.Common;
using Xunit;

namespace PixelFactory.Common.Tests;

public class AssetIdTests
{
    [Fact]
    public void New_ReturnsUniqueIds()
    {
        var a = AssetId.New();
        var b = AssetId.New();
        Assert.NotEqual(a, b);
    }

    [Fact]
    public void ToString_ReturnsHexWithoutDashes()
    {
        var id = AssetId.New();
        var str = id.ToString();
        Assert.Equal(32, str.Length);
        Assert.DoesNotContain("-", str);
    }
}
