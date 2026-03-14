using PixelFactory.Editor.Core;
using Xunit;

namespace PixelFactory.Editor.Core.Tests;

public class EditorProjectTests
{
    [Fact]
    public void ImplementsInterface()
    {
        IEditorProject project = new EditorProject
        {
            Name = "Test",
            RootPath = "/tmp/test",
        };

        Assert.Equal("Test", project.Name);
        Assert.Empty(project.Assets);
    }
}
