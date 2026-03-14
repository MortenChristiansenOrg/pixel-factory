using Arch.Core;

namespace PixelFactory.Engine.Core.ECS;

/// <summary>
/// Thin wrapper around Arch's World providing game-specific lifecycle.
/// </summary>
public sealed class GameWorld : IDisposable
{
    public World World { get; } = World.Create();

    public void Dispose() => World.Dispose();
}
