using System.Text.Json;
using PixelFactory.Common;

namespace PixelFactory.Editor.Core;

public sealed class ProjectService
{
    public EditorProject CreateProject(string name, string directory)
    {
        Directory.CreateDirectory(directory);
        Directory.CreateDirectory(Path.Combine(directory, "Assets"));

        var projectFile = new ProjectFile { Name = name };
        var projectPath = Path.Combine(directory, $"{name}.pixproj");
        var json = JsonSerializer.Serialize(projectFile, JsonContext.Default.ProjectFile);
        File.WriteAllText(projectPath, json);

        return new EditorProject
        {
            Name = name,
            RootPath = directory,
            ProjectFilePath = projectPath,
        };
    }

    public EditorProject LoadProject(string projectFilePath)
    {
        var json = File.ReadAllText(projectFilePath);
        var projectFile = JsonSerializer.Deserialize(json, JsonContext.Default.ProjectFile)
            ?? throw new InvalidDataException("Invalid project file");

        var rootPath = Path.GetDirectoryName(Path.GetFullPath(projectFilePath))!;
        var project = new EditorProject
        {
            Name = projectFile.Name,
            RootPath = rootPath,
            ProjectFilePath = Path.GetFullPath(projectFilePath),
        };

        foreach (var entry in projectFile.Assets)
        {
            var metaPath = Path.Combine(rootPath, entry.MetaPath);
            if (!File.Exists(metaPath)) continue;

            var metaJson = File.ReadAllText(metaPath);
            var metaFile = JsonSerializer.Deserialize(metaJson, JsonContext.Default.AssetMetaFile)
                ?? throw new InvalidDataException($"Invalid meta file: {metaPath}");
            project.AddAsset(metaFile.ToAssetMeta());
        }

        return project;
    }

    public void SaveProject(EditorProject project)
    {
        var projectFile = new ProjectFile { Name = project.Name };

        foreach (var asset in project.Assets)
        {
            var assetDir = Path.Combine("Assets", asset.Id.ToString());
            var metaPath = Path.Combine(assetDir, $"{asset.Name}.meta.json");
            projectFile.Assets.Add(new ProjectFile.AssetEntry
            {
                Id = asset.Id.ToString(),
                MetaPath = metaPath,
            });
        }

        var json = JsonSerializer.Serialize(projectFile, JsonContext.Default.ProjectFile);
        File.WriteAllText(project.ProjectFilePath, json);
    }

    public AssetMeta AddAsset(EditorProject project, AssetType type, string name, string? dataFile = null)
    {
        var id = AssetId.New();
        var meta = new AssetMeta
        {
            Id = id,
            Type = type,
            Name = name,
        };

        var assetDir = Path.Combine(project.RootPath, "Assets", id.ToString());
        Directory.CreateDirectory(assetDir);

        var metaFile = AssetMetaFile.FromAssetMeta(meta, dataFile);
        var metaPath = Path.Combine(assetDir, $"{name}.meta.json");
        var json = JsonSerializer.Serialize(metaFile, JsonContext.Default.AssetMetaFile);
        File.WriteAllText(metaPath, json);

        project.AddAsset(meta);
        SaveProject(project);

        return meta;
    }

    public void DeleteAsset(EditorProject project, AssetId id)
    {
        var assetDir = GetAssetDirectory(project, id);
        if (Directory.Exists(assetDir))
            Directory.Delete(assetDir, true);

        project.RemoveAsset(id);
        SaveProject(project);
    }

    public void RenameAsset(EditorProject project, AssetId id, string newName)
    {
        var asset = project.Assets.FirstOrDefault(a => a.Id == id)
            ?? throw new InvalidOperationException($"Asset {id} not found");

        var assetDir = GetAssetDirectory(project, id);
        var oldMetaPath = Path.Combine(assetDir, $"{asset.Name}.meta.json");

        asset.Name = newName;

        var metaFile = AssetMetaFile.FromAssetMeta(asset);
        // preserve DataFile if old meta had one
        if (File.Exists(oldMetaPath))
        {
            var oldJson = File.ReadAllText(oldMetaPath);
            var oldMeta = System.Text.Json.JsonSerializer.Deserialize(oldJson, JsonContext.Default.AssetMetaFile);
            if (oldMeta?.DataFile is not null)
                metaFile.DataFile = oldMeta.DataFile;
            File.Delete(oldMetaPath);
        }

        var newMetaPath = Path.Combine(assetDir, $"{newName}.meta.json");
        var json = System.Text.Json.JsonSerializer.Serialize(metaFile, JsonContext.Default.AssetMetaFile);
        File.WriteAllText(newMetaPath, json);

        SaveProject(project);
    }

    public string GetAssetDirectory(EditorProject project, AssetId id) =>
        Path.Combine(project.RootPath, "Assets", id.ToString());

    public MeshData? LoadMeshData(EditorProject project, AssetId id)
    {
        var assetDir = GetAssetDirectory(project, id);
        var meshPath = Path.Combine(assetDir, "mesh.bin");
        if (!File.Exists(meshPath)) return null;

        using var stream = File.OpenRead(meshPath);
        return MeshSerializer.Deserialize(stream);
    }
}
