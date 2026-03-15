using System.CommandLine;
using PixelFactory.Common;
using PixelFactory.Editor.Core;
using Spectre.Console;

namespace PixelFactory.Editor.Cli;

public static class Program
{
    public static async Task<int> Main(string[] args)
    {
        var service = new ProjectService();

        // project create
        var createNameOpt = new Option<string>("--name");
        var createPathOpt = new Option<string>("--path");
        var projectCreateCmd = new Command("create", "Create a new project");
        projectCreateCmd.Add(createNameOpt);
        projectCreateCmd.Add(createPathOpt);
        projectCreateCmd.SetAction(ctx =>
        {
            var name = ctx.GetValue(createNameOpt)!;
            var path = ctx.GetValue(createPathOpt)!;
            var project = service.CreateProject(name, path);
            AnsiConsole.MarkupLine($"[green]Created[/] {project.ProjectFilePath}");
        });

        // project open
        var openPathOpt = new Option<string>("--path");
        var projectOpenCmd = new Command("open", "Open an existing project");
        projectOpenCmd.Add(openPathOpt);
        projectOpenCmd.SetAction(ctx =>
        {
            var path = ctx.GetValue(openPathOpt)!;
            var project = service.LoadProject(path);
            AnsiConsole.MarkupLine($"[green]{project.Name}[/] — {project.Assets.Count} asset(s)");
            foreach (var asset in project.Assets)
                AnsiConsole.MarkupLine($"  [{asset.Type}] {asset.Name} ({asset.Id})");
        });

        var projectCmd = new Command("project", "Manage projects");
        projectCmd.Add(projectCreateCmd);
        projectCmd.Add(projectOpenCmd);

        // asset create
        var assetProjectOpt = new Option<string>("--project");
        var assetTypeOpt = new Option<string>("--type");
        var assetNameOpt = new Option<string>("--name");
        var assetCreateCmd = new Command("create", "Create a new asset");
        assetCreateCmd.Add(assetProjectOpt);
        assetCreateCmd.Add(assetTypeOpt);
        assetCreateCmd.Add(assetNameOpt);
        assetCreateCmd.SetAction(ctx =>
        {
            var projectPath = ctx.GetValue(assetProjectOpt)!;
            var typeName = ctx.GetValue(assetTypeOpt)!;
            var name = ctx.GetValue(assetNameOpt)!;

            var proj = service.LoadProject(projectPath);
            var assetType = Enum.Parse<AssetType>(typeName, ignoreCase: true);
            var meta = service.AddAsset(proj, assetType, name, assetType == AssetType.Mesh ? "mesh.bin" : null);

            if (assetType == AssetType.Mesh)
            {
                var mesh = DungeonRoomGenerator.Generate();
                var meshPath = Path.Combine(service.GetAssetDirectory(proj, meta.Id), "mesh.bin");
                using var stream = File.Create(meshPath);
                MeshSerializer.Serialize(stream, mesh);
                AnsiConsole.MarkupLine($"[green]Generated[/] dungeon room mesh ({mesh.Vertices.Length} verts, {mesh.Indices.Length} indices)");
            }

            AnsiConsole.MarkupLine($"[green]Created[/] {assetType} asset '{name}' ({meta.Id})");
        });

        // asset list
        var listProjectOpt = new Option<string>("--project");
        var assetListCmd = new Command("list", "List assets");
        assetListCmd.Add(listProjectOpt);
        assetListCmd.SetAction(ctx =>
        {
            var projectPath = ctx.GetValue(listProjectOpt)!;
            var proj = service.LoadProject(projectPath);
            var table = new Table();
            table.AddColumn("Name");
            table.AddColumn("Type");
            table.AddColumn("ID");
            foreach (var asset in proj.Assets)
                table.AddRow(asset.Name, asset.Type.ToString(), asset.Id.ToString());
            AnsiConsole.Write(table);
        });

        // asset screenshot
        var ssProjectOpt = new Option<string>("--project");
        var ssNameOpt = new Option<string>("--name");
        var ssOutputOpt = new Option<string>("--output");
        var ssWidthOpt = new Option<int>("--width");
        var ssHeightOpt = new Option<int>("--height");
        var assetScreenshotCmd = new Command("screenshot", "Render a mesh asset to a BMP file");
        assetScreenshotCmd.Add(ssProjectOpt);
        assetScreenshotCmd.Add(ssNameOpt);
        assetScreenshotCmd.Add(ssOutputOpt);
        assetScreenshotCmd.Add(ssWidthOpt);
        assetScreenshotCmd.Add(ssHeightOpt);
        assetScreenshotCmd.SetAction(ctx =>
        {
            var projectPath = ctx.GetValue(ssProjectOpt)!;
            var name = ctx.GetValue(ssNameOpt)!;
            var output = ctx.GetValue(ssOutputOpt) ?? "screenshot.bmp";
            var w = ctx.GetValue(ssWidthOpt) is > 0 ? ctx.GetValue(ssWidthOpt) : 800;
            var h = ctx.GetValue(ssHeightOpt) is > 0 ? ctx.GetValue(ssHeightOpt) : 600;

            var proj = service.LoadProject(projectPath);
            var asset = proj.Assets.FirstOrDefault(a => a.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
            if (asset is null)
            {
                AnsiConsole.MarkupLine($"[red]Asset '{name}' not found[/]");
                return;
            }
            if (asset.Type != AssetType.Mesh)
            {
                AnsiConsole.MarkupLine($"[red]Asset '{name}' is not a mesh[/]");
                return;
            }

            var meshData = service.LoadMeshData(proj, asset.Id);
            if (meshData is null)
            {
                AnsiConsole.MarkupLine($"[red]Could not load mesh data[/]");
                return;
            }

            ScreenshotRenderer.RenderMeshToBmp(meshData, output, w, h);
            AnsiConsole.MarkupLine($"[green]Saved[/] {output} ({w}x{h})");
        });

        // asset delete
        var delProjectOpt = new Option<string>("--project");
        var delNameOpt = new Option<string>("--name");
        var assetDeleteCmd = new Command("delete", "Delete an asset");
        assetDeleteCmd.Add(delProjectOpt);
        assetDeleteCmd.Add(delNameOpt);
        assetDeleteCmd.SetAction(ctx =>
        {
            var projectPath = ctx.GetValue(delProjectOpt)!;
            var name = ctx.GetValue(delNameOpt)!;
            var proj = service.LoadProject(projectPath);
            var asset = proj.Assets.FirstOrDefault(a => a.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
            if (asset is null)
            {
                AnsiConsole.MarkupLine($"[red]Asset '{name}' not found[/]");
                return;
            }
            service.DeleteAsset(proj, asset.Id);
            AnsiConsole.MarkupLine($"[green]Deleted[/] {asset.Type} asset '{name}'");
        });

        // asset rename
        var renProjectOpt = new Option<string>("--project");
        var renNameOpt = new Option<string>("--name");
        var renNewNameOpt = new Option<string>("--new-name");
        var assetRenameCmd = new Command("rename", "Rename an asset");
        assetRenameCmd.Add(renProjectOpt);
        assetRenameCmd.Add(renNameOpt);
        assetRenameCmd.Add(renNewNameOpt);
        assetRenameCmd.SetAction(ctx =>
        {
            var projectPath = ctx.GetValue(renProjectOpt)!;
            var name = ctx.GetValue(renNameOpt)!;
            var newName = ctx.GetValue(renNewNameOpt)!;
            var proj = service.LoadProject(projectPath);
            var asset = proj.Assets.FirstOrDefault(a => a.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
            if (asset is null)
            {
                AnsiConsole.MarkupLine($"[red]Asset '{name}' not found[/]");
                return;
            }
            service.RenameAsset(proj, asset.Id, newName);
            AnsiConsole.MarkupLine($"[green]Renamed[/] '{name}' → '{newName}'");
        });

        var assetCmd = new Command("asset", "Manage assets");
        assetCmd.Add(assetCreateCmd);
        assetCmd.Add(assetListCmd);
        assetCmd.Add(assetDeleteCmd);
        assetCmd.Add(assetRenameCmd);
        assetCmd.Add(assetScreenshotCmd);

        var rootCommand = new RootCommand("Pixel Factory CLI — content editor and build tool");
        rootCommand.Add(projectCmd);
        rootCommand.Add(assetCmd);

        return await rootCommand.Parse(args).InvokeAsync();
    }
}
