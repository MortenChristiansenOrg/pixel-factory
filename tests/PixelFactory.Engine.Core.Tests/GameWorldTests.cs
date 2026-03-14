using PixelFactory.Engine.Core.ECS;
using Xunit;

namespace PixelFactory.Engine.Core.Tests;

public class GameWorldTests
{
    [Fact]
    public void World_IsCreated()
    {
        using var gameWorld = new GameWorld();
        Assert.NotNull(gameWorld.World);
    }
}
