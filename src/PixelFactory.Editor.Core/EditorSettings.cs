using System.Text.Json;

namespace PixelFactory.Editor.Core;

public sealed class EditorSettings
{
    private static readonly string SettingsPath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "PixelFactory", "settings.json");

    public string? LastProjectPath { get; set; }

    public static EditorSettings Load()
    {
        if (!File.Exists(SettingsPath))
            return new EditorSettings();

        var json = File.ReadAllText(SettingsPath);
        return JsonSerializer.Deserialize(json, EditorSettingsJsonContext.Default.EditorSettings)
            ?? new EditorSettings();
    }

    public void Save()
    {
        var dir = Path.GetDirectoryName(SettingsPath)!;
        Directory.CreateDirectory(dir);
        var json = JsonSerializer.Serialize(this, EditorSettingsJsonContext.Default.EditorSettings);
        File.WriteAllText(SettingsPath, json);
    }
}

[System.Text.Json.Serialization.JsonSerializable(typeof(EditorSettings))]
[System.Text.Json.Serialization.JsonSourceGenerationOptions(WriteIndented = true, PropertyNamingPolicy = System.Text.Json.Serialization.JsonKnownNamingPolicy.CamelCase)]
internal partial class EditorSettingsJsonContext : System.Text.Json.Serialization.JsonSerializerContext;
