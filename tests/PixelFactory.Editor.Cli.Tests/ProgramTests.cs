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

    [Fact]
    public async Task ProjectCreate_CreatesFiles()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), $"pf_cli_test_{Guid.NewGuid():N}");
        try
        {
            var exitCode = await Program.Main(["project", "create", "--name", "CliTest", "--path", tempDir]);
            Assert.Equal(0, exitCode);
            Assert.True(File.Exists(Path.Combine(tempDir, "CliTest.pixproj")));
        }
        finally
        {
            if (Directory.Exists(tempDir)) Directory.Delete(tempDir, true);
        }
    }

    [Fact]
    public async Task AssetCreate_ProducesAssetOnDisk()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), $"pf_cli_test_{Guid.NewGuid():N}");
        try
        {
            await Program.Main(["project", "create", "--name", "AssetTest", "--path", tempDir]);
            var projPath = Path.Combine(tempDir, "AssetTest.pixproj");

            var exitCode = await Program.Main(["asset", "create", "--project", projPath, "--type", "Mesh", "--name", "Room"]);
            Assert.Equal(0, exitCode);

            // Verify asset directory was created under Assets/
            var assetsDir = Path.Combine(tempDir, "Assets");
            var assetDirs = Directory.GetDirectories(assetsDir);
            Assert.Single(assetDirs);
            Assert.True(File.Exists(Path.Combine(assetDirs[0], "mesh.bin")));
        }
        finally
        {
            if (Directory.Exists(tempDir)) Directory.Delete(tempDir, true);
        }
    }

    [Fact]
    public async Task AssetDelete_RemovesAsset()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), $"pf_cli_test_{Guid.NewGuid():N}");
        try
        {
            await Program.Main(["project", "create", "--name", "DelTest", "--path", tempDir]);
            var projPath = Path.Combine(tempDir, "DelTest.pixproj");
            await Program.Main(["asset", "create", "--project", projPath, "--type", "Texture", "--name", "MyTex"]);

            var exitCode = await Program.Main(["asset", "delete", "--project", projPath, "--name", "MyTex"]);
            Assert.Equal(0, exitCode);

            var assetsDir = Path.Combine(tempDir, "Assets");
            Assert.Empty(Directory.GetDirectories(assetsDir));
        }
        finally
        {
            if (Directory.Exists(tempDir)) Directory.Delete(tempDir, true);
        }
    }

    [Fact]
    public async Task AssetRename_ChangesName()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), $"pf_cli_test_{Guid.NewGuid():N}");
        try
        {
            await Program.Main(["project", "create", "--name", "RenTest", "--path", tempDir]);
            var projPath = Path.Combine(tempDir, "RenTest.pixproj");
            await Program.Main(["asset", "create", "--project", projPath, "--type", "Texture", "--name", "OldName"]);

            var exitCode = await Program.Main(["asset", "rename", "--project", projPath, "--name", "OldName", "--new-name", "NewName"]);
            Assert.Equal(0, exitCode);

            var assetDirs = Directory.GetDirectories(Path.Combine(tempDir, "Assets"));
            Assert.Single(assetDirs);
            Assert.True(File.Exists(Path.Combine(assetDirs[0], "NewName.meta.json")));
            Assert.False(File.Exists(Path.Combine(assetDirs[0], "OldName.meta.json")));
        }
        finally
        {
            if (Directory.Exists(tempDir)) Directory.Delete(tempDir, true);
        }
    }
}
