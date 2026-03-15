using System.Numerics;
using PixelFactory.Common;

namespace PixelFactory.Engine.Core.ECS;

public struct MeshComponent
{
    public AssetId MeshId;
}

public struct TransformComponent
{
    public Matrix4x4 LocalToWorld;
}
