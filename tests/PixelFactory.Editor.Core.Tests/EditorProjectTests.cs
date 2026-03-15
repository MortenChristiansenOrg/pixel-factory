using Xunit;

namespace PixelFactory.Editor.Core.Tests;

public class EditorProjectTests
{
    [Fact]
    public void ImplementsInterface()
    {
        var project = new EditorProject
        {
            Name = "Test",
            RootPath = "/tmp/test",
        };

        Assert.Equal("Test", project.Name);
        Assert.Empty(project.Assets);
    }
}
