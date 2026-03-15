using System.Numerics;
using PixelFactory.Engine.Core.Rendering;
using Xunit;

namespace PixelFactory.Engine.Core.Tests;

public class IsometricCameraTests
{
    [Fact]
    public void ViewMatrix_IsNotIdentity()
    {
        var camera = new IsometricCamera();
        Assert.NotEqual(Matrix4x4.Identity, camera.ViewMatrix);
    }

    [Fact]
    public void ProjectionMatrix_IsOrthographic()
    {
        var camera = new IsometricCamera();
        var proj = camera.ProjectionMatrix;
        // Orthographic projections have M34 == 0 (no perspective divide)
        Assert.Equal(0f, proj.M34);
    }
}
