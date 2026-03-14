namespace PixelFactory.Common;

/// <summary>
/// Strongly-typed identifier for assets in the content pipeline.
/// </summary>
public readonly record struct AssetId(Guid Value)
{
    public static AssetId New() => new(Guid.NewGuid());

    public override string ToString() => Value.ToString("N");
}
