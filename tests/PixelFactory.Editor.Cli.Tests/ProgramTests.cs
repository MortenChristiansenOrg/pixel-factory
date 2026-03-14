using Xunit;

namespace PixelFactory.Editor.Cli.Tests;

public class ProgramTests
{
    [Fact]
    public async Task RootCommand_WithHelp_ReturnsZero()
    {
        var exitCode = await Program.Main(["--help"]);
        Assert.Equal(0, exitCode);
    }
}
