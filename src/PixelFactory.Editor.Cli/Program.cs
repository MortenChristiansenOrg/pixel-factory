using System.CommandLine;

namespace PixelFactory.Editor.Cli;

public static class Program
{
    public static async Task<int> Main(string[] args)
    {
        var rootCommand = new RootCommand("Pixel Factory CLI — content editor and build tool");

        var projectCommand = new Command("project", "Manage projects");
        projectCommand.Add(new Command("create", "Create a new project"));
        projectCommand.Add(new Command("open", "Open an existing project"));
        rootCommand.Add(projectCommand);

        var assetCommand = new Command("asset", "Manage assets");
        assetCommand.Add(new Command("import", "Import an asset"));
        assetCommand.Add(new Command("list", "List assets"));
        rootCommand.Add(assetCommand);

        var buildCommand = new Command("build", "Build project assets");
        rootCommand.Add(buildCommand);

        var result = rootCommand.Parse(args);
        return await result.InvokeAsync();
    }
}
