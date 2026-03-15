using System.Numerics;

namespace PixelFactory.Engine.Core.Rendering;

public interface ICamera
{
    Matrix4x4 ViewMatrix { get; }
    Matrix4x4 ProjectionMatrix { get; }
}
