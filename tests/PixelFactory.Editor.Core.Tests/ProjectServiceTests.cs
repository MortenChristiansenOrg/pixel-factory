using PixelFactory.Common;
using Xunit;

namespace PixelFactory.Editor.Core.Tests;

public class ProjectServiceTests : IDisposable
{
    private readonly string _tempDir = Path.Combine(Path.GetTempPath(), $"pf_test_{Guid.NewGuid():N}");
    private readonly ProjectService _service = new();

    public void Dispose() { if (Directory.Exists(_tempDir)) Directory.Delete(_tempDir, true); }

    [Fact]
    public void CreateProject_CreatesFiles()
    {
        var project = _service.CreateProject("TestProject", _tempDir);

        Assert.True(File.Exists(project.ProjectFilePath));
        Assert.True(Directory.Exists(Path.Combine(_tempDir, "Assets")));
        Assert.Equal("TestProject", project.Name);
    }

    [Fact]
    public void LoadProject_RoundTrips()
    {
        var created = _service.CreateProject("RoundTrip", _tempDir);
        var loaded = _service.LoadProject(created.ProjectFilePath);

        Assert.Equal("RoundTrip", loaded.Name);
        Assert.Empty(loaded.Assets);
    }

    [Fact]
    public void AddAsset_CreatesFilesAndAppearsInProject()
    {
        var project = _service.CreateProject("WithAsset", _tempDir);
        var meta = _service.AddAsset(project, AssetType.Mesh, "TestMesh");

        Assert.Single(project.Assets);
        Assert.Equal("TestMesh", meta.Name);
        Assert.Equal(AssetType.Mesh, meta.Type);

        var assetDir = _service.GetAssetDirectory(project, meta.Id);
        Assert.True(Directory.Exists(assetDir));
        Assert.True(File.Exists(Path.Combine(assetDir, "TestMesh.meta.json")));

        // Reload and verify persistence
        var reloaded = _service.LoadProject(project.ProjectFilePath);
        Assert.Single(reloaded.Assets);
        Assert.Equal("TestMesh", reloaded.Assets[0].Name);
    }

    [Fact]
    public void DeleteAsset_RemovesFilesAndFromProject()
    {
        var project = _service.CreateProject("DelTest", _tempDir);
        var meta = _service.AddAsset(project, AssetType.Mesh, "ToDelete");
        var assetDir = _service.GetAssetDirectory(project, meta.Id);

        _service.DeleteAsset(project, meta.Id);

        Assert.Empty(project.Assets);
        Assert.False(Directory.Exists(assetDir));

        var reloaded = _service.LoadProject(project.ProjectFilePath);
        Assert.Empty(reloaded.Assets);
    }

    [Fact]
    public void RenameAsset_UpdatesMetaFileAndProject()
    {
        var project = _service.CreateProject("RenTest", _tempDir);
        var meta = _service.AddAsset(project, AssetType.Texture, "OldName");
        var assetDir = _service.GetAssetDirectory(project, meta.Id);

        _service.RenameAsset(project, meta.Id, "NewName");

        Assert.Equal("NewName", project.Assets[0].Name);
        Assert.False(File.Exists(Path.Combine(assetDir, "OldName.meta.json")));
        Assert.True(File.Exists(Path.Combine(assetDir, "NewName.meta.json")));

        var reloaded = _service.LoadProject(project.ProjectFilePath);
        Assert.Single(reloaded.Assets);
        Assert.Equal("NewName", reloaded.Assets[0].Name);
    }

    [Fact]
    public void RenameAsset_PreservesDataFile()
    {
        var project = _service.CreateProject("RenData", _tempDir);
        var meta = _service.AddAsset(project, AssetType.Mesh, "MyMesh", "mesh.bin");
        var assetDir = _service.GetAssetDirectory(project, meta.Id);

        // Write a dummy mesh.bin so it exists
        File.WriteAllBytes(Path.Combine(assetDir, "mesh.bin"), [0x01]);

        _service.RenameAsset(project, meta.Id, "RenamedMesh");

        var newMetaPath = Path.Combine(assetDir, "RenamedMesh.meta.json");
        var json = File.ReadAllText(newMetaPath);
        Assert.Contains("mesh.bin", json);
    }
}
