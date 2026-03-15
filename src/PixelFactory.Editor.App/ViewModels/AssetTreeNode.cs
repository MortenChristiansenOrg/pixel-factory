using System.Collections.ObjectModel;
using System.Windows.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using PixelFactory.Common;

namespace PixelFactory.Editor.App.ViewModels;

public sealed partial class AssetTreeNode : ObservableObject
{
    [ObservableProperty]
    private string _name = "";

    public AssetMeta? Asset { get; init; }
    public bool IsProject => Asset is null;
    public ObservableCollection<AssetTreeNode> Children { get; } = [];
    public bool IsExpanded { get; set; } = true;

    public string Icon { get; init; } = "\uf07b"; // folder
    public Brush IconBrush { get; init; } = Brushes.Gray;

    private static readonly SolidColorBrush FolderBrush = new(Color.FromRgb(0xE2, 0xB7, 0x14));
    private static readonly SolidColorBrush TextureBrush = new(Color.FromRgb(0x3D, 0xB8, 0xE8));
    private static readonly SolidColorBrush MeshBrush = new(Color.FromRgb(0xE8, 0x8D, 0x2A));
    private static readonly SolidColorBrush MaterialBrush = new(Color.FromRgb(0xAB, 0x5C, 0xE8));
    private static readonly SolidColorBrush ShaderBrush = new(Color.FromRgb(0x4E, 0xC9, 0x5D));
    private static readonly SolidColorBrush SceneBrush = new(Color.FromRgb(0xE8, 0xD4, 0x4D));
    private static readonly SolidColorBrush AudioBrush = new(Color.FromRgb(0xE8, 0x5D, 0x8C));
    private static readonly SolidColorBrush ScriptBrush = new(Color.FromRgb(0x4D, 0xC9, 0xC9));

    public static (string Icon, Brush Brush) GetIconForType(AssetType? type) => type switch
    {
        AssetType.Texture  => ("\uf03e", TextureBrush),   // image
        AssetType.Mesh     => ("\uf1b2", MeshBrush),      // cube
        AssetType.Material => ("\uf53f", MaterialBrush),   // palette
        AssetType.Shader   => ("\uf121", ShaderBrush),     // code
        AssetType.Scene    => ("\uf008", SceneBrush),      // film
        AssetType.Audio    => ("\uf001", AudioBrush),      // music
        AssetType.Script   => ("\uf70e", ScriptBrush),     // scroll
        _                  => ("\uf07b", FolderBrush),     // folder
    };
}
