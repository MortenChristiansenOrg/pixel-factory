namespace PixelFactory.Editor.Core;

public sealed class ProjectFile
{
    public string Name { get; set; } = "";
    public string Version { get; set; } = "0.1.0";
    public List<AssetEntry> Assets { get; set; } = [];

    public sealed class AssetEntry
    {
        public string Id { get; set; } = "";
        public string MetaPath { get; set; } = "";
    }
}
